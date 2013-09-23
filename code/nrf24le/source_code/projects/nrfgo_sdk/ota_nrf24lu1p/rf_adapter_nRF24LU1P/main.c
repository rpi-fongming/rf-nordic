#include "nrf24lu1p.h"
#include <stdint.h>
#include <stdbool.h>
#include <string.h>
#include "hal_nrf.h"
#include "hal_usb.h"
#include "hal_usb_hid.h"
#include "usb_map.h"
#include "hal_flash.h"
#include "protocol.h"
#include "hal_delay.h"


//-----------------------------------------------------------------------------
// Application specific constants
//-----------------------------------------------------------------------------

#define RF_SEARCHING_AUTO_RETRIES 2 // Auto-retries when searching for remote device
#define RF_SEARCHING_AUTO_RETRY_DELAY 250 // us between auto-retries when searching
#define RF_CONNECTED_AUTO_RETRIES 8 // Increased retries as transactions become more critical
#define RF_CONNECTED_AUTO_RETRY_DELAY 500

#define LINE_SIZE 24 // Maximum Hex Line size
#define PING_INTERVAL 20000
#define RETRIES 4

#define USB_IN_CMD usb_in_buf[0]
#define USB_OUT_CMD usb_out_buf[0]
#define ERROR_CODE usb_in_buf[1]

#define RF_OUT_CMD rf_out_buf[0]
#define RF_IN_CMD rf_in_buf[0]

#define SEND_USB() app_send_usb_in_data(0, 0)
#define FORWARD_RF_TO_USB() app_send_usb_in_data(rf_in_buf, 32)

//-----------------------------------------------------------------------------
// Enumerators
//-----------------------------------------------------------------------------

typedef enum
{
  APP_IDLE = 0,
  APP_SEARCHING,
  APP_CONNECTED,  
  APP_WAIT_ACK,
  APP_AWAITING_LINE,
  APP_FORWARD,
  APP_EXIT
} app_states_t;

//-----------------------------------------------------------------------------
// Global variables
//-----------------------------------------------------------------------------
app_states_t xdata app_state;
uint8_t xdata lineBuffer[LINE_SIZE];

//-----------------------------------------------------------------------------
// USB variables
//-----------------------------------------------------------------------------
xdata uint8_t usb_in_buf[EP1_2_PACKET_SIZE];
xdata uint8_t usb_out_buf[EP1_2_PACKET_SIZE];
bool xdata app_usb_out_data_ready = false;
extern code usb_string_desc_templ_t g_usb_string_desc;
bool xdata app_pending_usb_write = false;

//-----------------------------------------------------------------------------
// RF variables
//-----------------------------------------------------------------------------
uint8_t xdata rf_in_buf[32];
uint8_t xdata rf_out_buf[32];
bool xdata packet_received = false;
bool xdata radio_busy = false;
bool xdata transmitted = false;
uint16_t xdata rf_ping_timer = PING_INTERVAL;
uint8_t xdata retry_counter = RETRIES;
uint8_t xdata default_channels[CHANNELS_SIZE] = CHANNELS;
uint8_t xdata default_pipe_address[5] = PIPE_ADDRESS;
uint8_t xdata channel_index;

//-----------------------------------------------------------------------------
// Internal function prototypes
//-----------------------------------------------------------------------------
void app_send_usb_in_data(uint8_t * buf, uint8_t size);
void app_parse_usb_out_packet();
void app_wait_while_usb_pending();
void forward_usb_to_rf(); 
void rf_send_buffer();
void rf_receive_ms(uint16_t ms);
void rf_init();


//-----------------------------------------------------------------------------
// USB callback function prototypes
//-----------------------------------------------------------------------------
static hal_usb_dev_req_resp_t device_req_cb(hal_usb_device_req* req, uint8_t** data_ptr, uint8_t* size) reentrant;
static void suspend_cb(uint8_t allow_remote_wu) reentrant;
static void resume_cb() reentrant;
static void reset_cb() reentrant;
static uint8_t ep_1_in_cb(uint8_t *adr_ptr, uint8_t* size) reentrant;
static uint8_t ep_2_out_cb(uint8_t *adr_ptr, uint8_t* size) reentrant;

//-----------------------------------------------------------------------------
// Main routine
//-----------------------------------------------------------------------------
void main()
{
  // USB HAL initialization
  hal_usb_init(true, device_req_cb, reset_cb, resume_cb, suspend_cb);   
  hal_usb_endpoint_config(0x81, EP1_2_PACKET_SIZE, ep_1_in_cb);  // Configure 32 byte IN endpoint 1
  hal_usb_endpoint_config(0x02, EP1_2_PACKET_SIZE, ep_2_out_cb); // Configure 32 byte OUT endpoint 2

  // Initialize RF device
  rf_init();

  // Set initial state
  app_state = APP_IDLE;
  // Enable global interrupt
  EA = 1;
  
  #ifdef DEBUG
  P0DIR = 0x00;
  #endif

  while(true)                                                                               
  {
    
    if(hal_usb_get_state() == CONFIGURED)
    { 
      // If "vendor specific" data received on USB
      if(app_usb_out_data_ready)
      {
        app_parse_usb_out_packet();
        app_usb_out_data_ready = false;
      }

      switch (app_state) 
      {
                
        case APP_IDLE:
          // Do nothing. Waiting for Commands from Host
          break;
            
        // In the SEARCHING state the USB dongle will send INIT commands
        // to the remote device on the specified channels.
        // If it receives an ACK from the remote device it will enter CONNECTED
        // state.
        case APP_SEARCHING:
          // Start searching for a compatible device
          if (!radio_busy)
          {
            // Go to the next channel and send INIT command
            (channel_index > 1) ? channel_index = 0 : channel_index++;
            hal_nrf_set_rf_channel(default_channels[channel_index]);
            RF_OUT_CMD = CMD_INIT;
            rf_send_buffer();
            if (transmitted)
            {
              // On successful INIT transmit, wait for ACK and send it to app. 
              rf_receive_ms(10);
              if (packet_received && RF_IN_CMD == (uint8_t)CMD_ACK)
              {
                hal_nrf_set_auto_retr(RF_CONNECTED_AUTO_RETRIES, RF_CONNECTED_AUTO_RETRY_DELAY);
                FORWARD_RF_TO_USB();
                rf_ping_timer = PING_INTERVAL;
                retry_counter = RETRIES;
                app_state = APP_CONNECTED;
              }
            }
          }
          break;
            
        // In the CONNECTED state the USB Dongle will periodically send PING
        // commands to the remote device, to make sure they are still
        // connected. Remote device should respond with PONG. After sending
        // a RETRIES number of PING without getting PONG, the connection
        // is considered lost, and it notify the host with a NACK and enter 
        // IDLE state.
        case APP_CONNECTED:
          if (rf_ping_timer == 0)
          {
            rf_ping_timer = PING_INTERVAL;
            // Send PING to remote device.
            RF_OUT_CMD = CMD_PING;
            rf_send_buffer();   
            if (transmitted)
            {
              // Wait for PONG
              rf_receive_ms(10);
              if (packet_received && RF_IN_CMD == (uint8_t)CMD_PONG)
              {
                break;
              }
            }
            // Did not receive PONG.
            if (retry_counter == 0)
            {
              // Connection is considered lost, enter IDLE state.
              retry_counter = RETRIES;
              USB_IN_CMD = CMD_NACK;
              ERROR_CODE = ERROR_LOST_CONNECTION;
              SEND_USB();
              app_state = APP_IDLE;
            }
            else
              retry_counter--;
          }
          else
            rf_ping_timer--;
          break;
           
        // In the FORWARD state the USB Dongle just forward whatever
        // commands and data it receives from the PC application to
        // the remote device. It will enter the WAIT_ACK state on 
        // successful transmit.
        case APP_FORWARD:
          forward_usb_to_rf();
          if (transmitted)
          {
            retry_counter = RETRIES;
            app_state = APP_WAIT_ACK;
          }
          else 
          {
            USB_IN_CMD = CMD_NACK;
            ERROR_CODE = ERROR_LOST_CONNECTION;
            SEND_USB();
            app_state = APP_IDLE;
          }
          break;
    
        // Forward command received on USB to remote device and enter IDLE
        // state.
        case APP_EXIT:
          forward_usb_to_rf();
          app_state = APP_IDLE;
          break;
            

        // Wait for ACK or NACK and send it over USB to PC application.
        case APP_WAIT_ACK: 
          rf_receive_ms(500); 
          if (packet_received && (RF_IN_CMD == (uint8_t)CMD_ACK || 
                                  RF_IN_CMD == (uint8_t)CMD_NACK))
          {
            FORWARD_RF_TO_USB();
            rf_ping_timer = PING_INTERVAL;
            retry_counter = RETRIES;
            app_state = APP_CONNECTED;
          }
          else 
          {
            if (retry_counter > 0)
            {
              retry_counter--;
            }
            else
            {
              // Did not receive ACK or NACK after 4 listening periods, 
              // connection lost.
              USB_IN_CMD = CMD_NACK;
              ERROR_CODE = ERROR_LOST_CONNECTION;
              SEND_USB();
              app_state = APP_IDLE;
            }
          }
          break;

      }
      #ifdef DEBUG
      P0 = app_state;
      #endif
    }
    else
    {
      #ifdef DEBUG
      P0 = 0xF0;
      #endif
    }
  }
}

//-----------------------------------------------------------------------------
// Handle commands from Host application
//-----------------------------------------------------------------------------
void app_parse_usb_out_packet()
{
  switch(USB_OUT_CMD)
  {
    case CMD_INIT:
      hal_nrf_set_auto_retr(RF_CONNECTED_AUTO_RETRY_DELAY, RF_CONNECTED_AUTO_RETRY_DELAY);
      app_state = APP_SEARCHING;
      break;

    case CMD_EXIT:
      app_state = APP_EXIT;
      break;
    
    case CMD_UPDATE_START:
    case CMD_UPDATE_COMPLETE:
    case CMD_WRITE:
    case CMD_READ:
      app_state = APP_FORWARD;
      break;
  }
}

//-----------------------------------------------------------------------------
// RF helper functions
//-----------------------------------------------------------------------------

// Initialize radio module
void rf_init()
{
  // Enable radio SPI and clock
  RFCTL = 0x10;
  RFCKEN = 1;
  // Set payload width to 32 bytes
  hal_nrf_set_rx_payload_width((int)HAL_NRF_PIPE0, 32);
  // Set pipe address
  hal_nrf_set_address(HAL_NRF_PIPE0, default_pipe_address);
  hal_nrf_set_address(HAL_NRF_TX, default_pipe_address);
  // Clear and flush radio state
  hal_nrf_get_clear_irq_flags();
  hal_nrf_flush_rx();
  hal_nrf_flush_tx();
  CE_LOW();
  transmitted = false;
  packet_received = false;
  // Power up radio
  hal_nrf_set_power_mode(HAL_NRF_PWR_UP);
  // Enable RF interrupt
  RF = 1;
}

// Start transmission of the RF buffer
void rf_send_buffer()
{
  CE_LOW();
  // Configure radio as primary transmitter (PTX)
  hal_nrf_set_operation_mode(HAL_NRF_PTX);
  transmitted = false;
  // Write payload to radio TX FIFO
  hal_nrf_write_tx_payload(rf_out_buf, 32);
  // Toggle radio CE signal to start transmission
  CE_PULSE();
  radio_busy = true;
  while(radio_busy) ; 
}

// Wait for a RF packet the specified amount of time
void rf_receive_ms(uint16_t time_ms)
{
  packet_received = false;
  // Configure radio as primary receiver (PRX)
  hal_nrf_set_operation_mode(HAL_NRF_PRX);
  CE_HIGH();
  //P0 = 0xFF;
  while(time_ms--)
  {
    if(packet_received)
    {
      break;
    }
    delay_ms(1);
  } 
  CE_LOW();
}

// Interrupt handler for RF module
NRF_ISR()
{
  uint8_t irq_flags;
  // Read and clear IRQ flags from radio
  irq_flags = hal_nrf_get_clear_irq_flags();

  switch (irq_flags) 
  {
    // Transmission success.
    case (1 << (uint8_t)HAL_NRF_TX_DS):
      radio_busy = false;
      transmitted = true;
      break;

    // Transmission failed (maximum re-transmits)
    case (1 << (uint8_t)HAL_NRF_MAX_RT):
      hal_nrf_flush_tx();
      radio_busy = false;
      transmitted = false;
      break;

    // Data received 
    case (1 << (uint8_t)HAL_NRF_RX_DR):
      // Read payload
      while (!hal_nrf_rx_fifo_empty()) { 
        hal_nrf_read_rx_payload(rf_in_buf);
      }
      packet_received = true;
      break;

  }
}

//-----------------------------------------------------------------------------
// USB Helper functions
//-----------------------------------------------------------------------------  

// Send data to host  
void app_send_usb_in_data(uint8_t * buf, uint8_t size)
{
  app_wait_while_usb_pending();
  app_pending_usb_write = true;  
  memcpy(usb_in_buf, buf, size);
  hal_usb_send_data(1, usb_in_buf, EP1_2_PACKET_SIZE);
}

void forward_usb_to_rf()
{
  memcpy(rf_out_buf, usb_out_buf, 32);
  rf_send_buffer();
}

void app_wait_while_usb_pending()
{    
  uint16_t timeout = 50000;   // Will equal ~ 50-100 ms timeout 
  while(timeout--)
  {
    if(!app_pending_usb_write)
    {
      break;
    }
  }    
}

//-----------------------------------------------------------------------------
// USB Callbacks
//-----------------------------------------------------------------------------  

static hal_usb_dev_req_resp_t device_req_cb(hal_usb_device_req* req, uint8_t** data_ptr, uint8_t* size) reentrant
{
  hal_usb_dev_req_resp_t retval;

  if( hal_usb_hid_device_req_proc(req, data_ptr, size, &retval) == true ) 
  {
    // The request was processed with the result stored in the retval variable
    return retval;
  }
  else
  {
    // The request was *not* processed by the HID subsystem
    return STALL;
  }
}

static void suspend_cb(uint8_t allow_remote_wu) reentrant
{
  USBSLP = 1; // Disable USB clock (auto clear)
  allow_remote_wu = 0;  
}

static void resume_cb() reentrant
{
}

static void reset_cb() reentrant
{
}

//-----------------------------------------------------------------------------
// USB Endpoint Callbacks
//-----------------------------------------------------------------------------  
uint8_t ep_1_in_cb(uint8_t *adr_ptr, uint8_t* size) reentrant
{  
  app_pending_usb_write = false;
  return 0x60; // NAK
  adr_ptr = adr_ptr;
  size = size;
}

uint8_t ep_2_out_cb(uint8_t *adr_ptr, uint8_t* size) reentrant
{
  memcpy(usb_out_buf, adr_ptr, *size);
  app_usb_out_data_ready = true;
  //P0 = *size;
  return 0xff; // ACK
}


