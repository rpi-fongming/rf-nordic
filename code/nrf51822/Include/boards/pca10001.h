/* Copyright (c) 2012 Nordic Semiconductor. All Rights Reserved.
 *
 * The information contained herein is property of Nordic Semiconductor ASA.
 * Terms and conditions of usage are described in detail in NORDIC
 * SEMICONDUCTOR STANDARD SOFTWARE LICENSE AGREEMENT.
 *
 * Licensees are granted free, non-transferable use of the information. NO
 * WARRANTY of ANY KIND is provided. This heading must NOT be removed from
 * the file.
 *
 */
#ifndef PCA10001_H
#define PCA10001_H

#define LED_START      18	
#define LED_STOP       19
#define LED_PORT       NRF_GPIO_PORT_SELECT_PORT2		// Port 2 (GPIO pin 16-23)
#define LED_OFFSET     2

#define LED_BLUE       04
#define LED_RED        07
#define LED_PORT_BR		 NRF_GPIO_PORT_SELECT_PORT0		// Port 0 (GPIO pin 0-7)


#define BUTTON_START   16
#define BUTTON0        16
#define BUTTON_STOP    17
#define BUTTON1        17

#define RX_PIN_NUMBER  11
#define TX_PIN_NUMBER  9
#define CTS_PIN_NUMBER 10
#define RTS_PIN_NUMBER 8
#define HWFC           true

#define BLINKY_STATE_MASK   0x01

#endif
