//----------------------------------------------------------------
//----------------------------------------------------------------
// Module name: User Utility
//
// Copyright 2008 Fong Ming as an  unpublished work. 
// All Rights Reserved.
//
// The information contained herein is confidential 
// property of Company. The user, copying, transfer or 
// disclosure of such information is prohibited except
// by express written agreement with Company.
//
// First written on 2013-09-30 by Fong Ming
//
//----------------------------------------------------------------
//----------------------------------------------------------------


#include <stdio.h>
#include "nrf24le1.h"
#include "hal_uart.h"
#include "hal_clk.h"
#include "application.h"
#include "utility.h"

void app_init(void)
{
	app_init_hardware();
	app_init_variable();

}

void app_init_hardware(void)
{
  // Configure TXD pin as output.
  // P0.5, P0.3 and P1.0 are configured as outputs to make the example run on
  // either 24-pin, 32-pin or 48-pin nRF24LE1 variants.
  P0DIR = 0xD7;
  P1DIR = 0xFE;
	
	// Initializes the UART
  hal_uart_init(UART_BAUD_9K6);
  // Wait for XOSC to start to ensure proper UART baudrate
  while(hal_clk_get_16m_source() != HAL_CLK_XOSC16M)
  {}
}

void app_init_variable(void)
{

}


void app_main_loop(void)
{

	for(;;)
		{
			// If any characters received
			if( hal_uart_chars_available() )
			{
				P3 = 0x11;
				// Echo received characters
				u_putch(getchar());
			}
		}

}



