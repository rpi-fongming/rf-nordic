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

static unsigned char mTFlag=0;
static unsigned char mFlag1S=0;

void timer0_ISR (void) interrupt 1
{
	static unsigned char tCnt=0;
	static unsigned char tCnt1S=0;

	// 16MHz/12 = 0.75uS
	// 0.75uS * 65536 = 49.152mS

	// 0.75uS * 8 * 1024 = 6.144mS
	// 6.144mS * 8 = 49.152mS

   	tCnt ++;
	if (tCnt >=8)
	{
		mTFlag = 1;			// Set TimerFlag	
		tCnt = 0;
	}

	tCnt1S ++;
	if (tCnt1S >=81)
	{
		mFlag1S = 1;			// Set Flag1S	
		tCnt1S = 0;
	}

}


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

	/*--------------------------------------
	Set Timer0 for 16-bit timer mode.  The
	timer counts to 65535, overflows, and
	generates an interrupt.

	Set the Timer0 Run control bit.
	--------------------------------------*/
	TMOD = (TMOD & 0xF0) | 0x00;  /* Set T/C0 Mode 13bit timer mode*/
	ET0 = 1;                      /* Enable Timer 0 Interrupts */
	TR0 = 1;                      /* Start Timer 0 Running */
	
	// Initializes the UART
  hal_uart_init(UART_BAUD_9K6);
  // Wait for XOSC to start to ensure proper UART baudrate
//  while(hal_clk_get_16m_source() != HAL_CLK_XOSC16M)
//  {}	

}

void app_init_variable(void)
{
	mTFlag = 0;
	mFlag1S = 0;
}

void app_uart_cmd(void)
{
			// If any characters received
			if( hal_uart_chars_available() )
			{
				P3 = 0x11;
				// Echo received characters
				u_putch(getchar());
			}
}

void app_timer_task(void)
{
		if (mFlag1S==1)				// 100mS flag
		{
			mFlag1S = 0;
			LED0 = !LED0;	
		}
		
		if (mTFlag==1)		// 100mS flag
		{
			mTFlag = 0;
			LED1 = !LED1;	
		}
}


void app_main_loop(void)
{

	for(;;)
		{
			app_uart_cmd();
			app_timer_task();
		}

}



