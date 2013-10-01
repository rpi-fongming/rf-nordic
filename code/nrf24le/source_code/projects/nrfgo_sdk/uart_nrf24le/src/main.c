/* Copyright (c) 2009 Nordic Semiconductor. All Rights Reserved.
 *
 * The information contained herein is confidential property of Nordic
 * Semiconductor ASA.Terms and conditions of usage are described in detail
 * in NORDIC SEMICONDUCTOR STANDARD SOFTWARE LICENSE AGREEMENT.
 *
 * Licensees are granted free, non-transferable use of the information. NO
 * WARRENTY of ANY KIND is provided. This heading must NOT be removed from
 * the file.
 *
 * $LastChangedRevision: 133 $
 */

/** @file
 * @brief UART example
 * @defgroup uart_example UART "Hello World" example
 * @{
 * @ingroup nrf_examples
 *
 * @brief This example writes the string "Hello World" on start-up. After this all
 * characters received on the RXD input are echoed to the TXD output.
 *
 * The example implements the low level stdio functions putchar() and getchar() so that standard
 * IO functions such as printf() and gets() can be used by the application.
 *
*/

//lint -e732
//lint -e713
//lint -e640

#include <stdio.h>
#include "nrf24le1.h"
#include "hal_uart.h"
#include "hal_clk.h"
#include "application.h"
#include "utility.h"


void main(void)
{

	app_init();
  // Enable global interrupts
  EA = 1;

  // Print "Hello World" at start-up
//  u_puts("\r\Enter Main Loop!\r\n");
	
	app_main_loop();

	// Print "Hello World" at start-up
//  u_puts("\r\Exit Main Loop!\r\n");

}
/** @} */
