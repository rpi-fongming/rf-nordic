using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using USBHIDLib;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO;

namespace nRFupdate
{

    #region Custom event arguments

    public class BoolEventArgs : System.EventArgs
    {

        private bool status;

        public BoolEventArgs(bool a)
        {
            this.status = a;
        }

        public bool Status()
        {
            return status;
        }
    }
    public class ProgressEventArgs : System.EventArgs
    {

        private double progress;

        public ProgressEventArgs(double p)
        {
            this.progress = p;
        }

        public double Progress()
        {
            return progress;
        }
    }
    public class MessageEventArgs : System.EventArgs
    {

        private string message;

        public MessageEventArgs(string m)
        {
            this.message = m;
        }

        public string Message()
        {
            return message;
        }
    }
    public class ConnectionEventArgs : System.EventArgs
    {
        private byte model;
        private byte version;

        public ConnectionEventArgs(byte mod, byte ver)
        {
            this.model = mod;
            this.version = ver;
        }

        public byte Model()
        {
            return model;
        }
        public byte Version()
        {
            return version;
        }
    }

    #endregion

    public class nRFupdateControl
    {

        #region Constants

        // Must match vendor and product id of USB device
        private const int USB_VENDOR_ID = 0x1915;
        private const int USB_PRODUCT_ID = 0x007B;
        
        private const int MAX_BINARY_SIZE = 1024 * 13;      // Change when bootloader size determined
        private const int MAX_BYTES_PER_HEX_LINE = 21;      // Maximum size of the Hex lines (Excluding colon)
        private const int LONG_JUMP_INSTRUCTION_SIZE = 3;   // Size of the starting long jump instruction.

        private const int HEX_LINE_SIZE_INDEX = 0;          // Index of Hex line length 
        private const int HEX_LINE_MSB_ADDR_INDEX = 1;      // Index of MSB of Hex line address
        private const int HEX_LINE_LSB_ADDR_INDEX = 2;      // Index of LSB of Hex line address
        private const int HEX_LINE_DATA_INDEX = 4;          // Starting index of Hex line data

        
        #endregion

        #region Enumerators
        public enum Commands
        {
            /* Default, initialization value. Not to be sent between host and device. */
            CMD_NO_CMD = 0x00,
            /* Host sends exit signal to device, connection to be terminated after device
             * has sent ACK. */
            CMD_EXIT,
            /* Host initiates a connection, device should send ACK, and then both will be
             * in the CONNECTED state. */
            CMD_INIT,
            /* Host initiates a firmware update, device should send ACK if ready. */
            CMD_UPDATE_START,
            /* Host requests a portion of the flash memory. 
             * 2B start address, 1B number of requested bytes.
             * ACK respons should contain the data as payload */
            CMD_READ,
            /* Host sends a HEX line, device should send ACK after line has been 
             * written to FLASH, and is ready for another. */
            CMD_WRITE,
            /* Host sends confirmation that update has been completed and 
             * grants permission to enable the new firmware. */
            CMD_UPDATE_COMPLETE,
            /* Acknowledge, might contain data payload depending on corresponding request */
            CMD_ACK,
            /* Negative acknowledge */
            CMD_NACK,
            /* Check for connection integrity. Should be answered with CMD_PONG. */
            CMD_PING,
            /* Answer for CMD_PING. */
            CMD_PONG
        }
        public enum ErrorCodes
        {
            /* Generic nack error message defined by nature of request */
            ERROR_GENERIC = 0,
            /* The LU1p has lost contact with the LE1 */
            ERROR_LOST_CONNECTION,
            /* LE1 reports wrong checksum on received line. Line should be sent again */
            ERROR_CHECKSUM_FAIL,
            /* Hex line passed checksum, but contains illegal address. 
             * Update should be cancelled */
            ERROR_ILLEGAL_ADDRESS,
            /* Nack error message when CMD_UPDATE_INIT provides a size which is 
             * to big for the flash memory */
            ERROR_ILLEGAL_SIZE
        }

        public enum AckTypes
        {
            NO_ACK,
            LINE_ACK,
            UPDATE_START_ACK,
            COMPLETE_ACK,
            READ_ACK,
            INIT_ACK
        }
        #endregion

        #region Event Handlers

        public event EventHandler DongleConnected;
        public event EventHandler DongleDisconnected;
        public event EventHandler<ConnectionEventArgs> DeviceConnected;
        public event EventHandler DeviceDisconnected;
        public event EventHandler<BoolEventArgs> HexVerified;
        public event EventHandler<BoolEventArgs> UpdateComplete;
        public event EventHandler UpdateStarted;
        public event EventHandler VerificationStarted;
        public event EventHandler ReadyToRun;
        public event EventHandler<BoolEventArgs> FlashVerified;
        public event EventHandler<ProgressEventArgs> UpdateProgressChanged;
        public event EventHandler<MessageEventArgs> StatusUpdate;

        #endregion

        #region Variables

        // USB variables
        public USBHIDLib.UsbHidPort usbHidPort1;
        private bool usbConnected = false;
        private bool pendingTransfer = false;
        private bool autoVerify;

        // 
        private byte model, version, newVersion;

        private List<List<byte>> hexLines;
        private byte[] longJumpAddress;
        private UInt16 sentLines = 0;
        private int reSends = 0;

        private int verifiedLines = 0;
        private byte[] receivedLine;
        private bool aborting = false;
        private bool reRequesting = false;

        private byte[] hexArray;
        private UInt16 hexSize = 0;
        private UInt16 lineCount = 0;
        private UInt16 byteCount = 0;
        private bool validHexLoaded = false;

        private AckTypes expectedAck = AckTypes.NO_ACK;
        #endregion

        #region Constructor
        public nRFupdateControl()
        {
            usbHidPort1 = new USBHIDLib.UsbHidPort();

            // Specific device vendor and product id
            usbHidPort1.VendorId = USB_VENDOR_ID;
            usbHidPort1.ProductId = USB_PRODUCT_ID;
            usbHidPort1.PreferredInterface = 0x100;

            usbHidPort1.OnDataRecieved += new USBHIDLib.DataRecievedEventHandler(usb_OnDataRecieved);
            usbHidPort1.OnSpecifiedDeviceArrived += new EventHandler(usb_OnSpecifiedDeviceArrived);
            usbHidPort1.OnSpecifiedDeviceRemoved += new EventHandler(usb_OnSpecifiedDeviceRemoved);
            usbHidPort1.OnDataSent += new EventHandler(usb_OnDataSent);
        }
        #endregion

        #region USB functions
        // Usb dongle is disconnected, notify application
        void usb_OnSpecifiedDeviceRemoved(object sender, EventArgs e)
        {
            DongleDisconnected(this, new EventArgs());
            usbConnected = false;
            StatusUpdate(this, new MessageEventArgs("USB dongle disconnected."));
        }

        // Usb dongle is connected, notify application
        void usb_OnSpecifiedDeviceArrived(object sender, EventArgs e)
        {
            DongleConnected(this, new EventArgs());
            usbConnected = true;
            StatusUpdate(this, new MessageEventArgs("USB dongle connected."));
        }

        // Send usb data
        private void SendDataAndWait(byte[] transferData)
        {
            if (aborting == false)
            {
                pendingTransfer = true;
                this.usbHidPort1.SpecifiedDevice.SendData(transferData);

                for (int i = 0; i < 5; i++)
                {
                    if (!pendingTransfer)
                    {
                        break;
                    }
                    System.Threading.Thread.Sleep(10);
                }
            }
        }

        // USB event handlers
        void usb_OnDataSent(object sender, EventArgs e)
        {
            pendingTransfer = false;
        }

        // Handle received data
        private void usb_OnDataRecieved(object sender, DataRecievedEventArgs args)
        {
            this.ParseReceivePacket(args.data);
        }
#endregion

        #region Communication functions
        // Handle received data from dongle
        private void ParseReceivePacket(byte[] packet)
        {
            if (aborting)
                return;
            switch ((Commands)packet[0])
            {

                case Commands.CMD_ACK:
                    // Handle incoming ack according to what is expected
                    HandleAck(packet);
                    break;

                case Commands.CMD_NACK:
                    HandleNack((ErrorCodes)packet[1]);
                    break;
            }

        }

        // Determines suitable response for received ack
        private void HandleAck(byte[] packet)
        {
            switch (expectedAck)
            {
                case AckTypes.NO_ACK:
                    // Got ack when none expected, do nothing
                    break;

                case AckTypes.INIT_ACK:
                    // Awaiting initial ack. Store provided info and notify gui.
                    {
                        model = packet[1];
                        version = packet[2];
                        this.DeviceConnected(this, new ConnectionEventArgs(model, version));
                        StatusUpdate(this, new MessageEventArgs("Connected to remote device."));
                    }
                    break;

                case AckTypes.UPDATE_START_ACK:
                    // Our request for firmware update approved.
                    {
                        // Notify gui
                        if (UpdateStarted != null)
                            UpdateStarted(this, EventArgs.Empty);
                        expectedAck = AckTypes.NO_ACK;
                        StatusUpdate(this, new MessageEventArgs("Got update approval from device."));
                        // Send first line
                        sendLine();
                    }
                    break;

                case AckTypes.LINE_ACK:
                    {
                        // Sent line was confirmed received
                        reSends = 0;
                        sentLines++;
                        // Notify gui of current progress
                        UpdateProgressChanged(this, new ProgressEventArgs((double)sentLines / lineCount));

                        if (sentLines != lineCount)
                        {
                            sendLine();
                        }
                        else
                        {
                            // This was the final line, start verifying if verify box checked
                            UpdateComplete(this, new BoolEventArgs(true));
                            if (autoVerify)
                            {
                                startVerification();
                            }
                            else
                            {
                                // If not, notify device of complete transfer
                                sendUpdateComplete();
                            }
                        }
                    }
                    break;

                case AckTypes.COMPLETE_ACK:
                    // Ack for update complete message, notify gui.
                    {
                        expectedAck = AckTypes.NO_ACK;
                        StatusUpdate(this, new MessageEventArgs("Device confirmed update complete."));
                        if (ReadyToRun != null) ReadyToRun(this, EventArgs.Empty);
                    }
                    break;

                case AckTypes.READ_ACK:
                    // Answer to request for a flash memory line.
                    // Copy and start verification 
                    {
                        expectedAck = AckTypes.NO_ACK;
                        packet.CopyTo(receivedLine, 0);
                        verifyFlashLine();
                    }
                    break;
            }
        }

        // Determines suitable response for received nack
        private void HandleNack(ErrorCodes error)
        {
            switch (error)
            {
                case ErrorCodes.ERROR_GENERIC:
                    if (expectedAck == AckTypes.COMPLETE_ACK)
                    {
                        if (StatusUpdate != null)
                            StatusUpdate(this, new MessageEventArgs("ERROR: Remote device reported incomplete firmware transmission."));
                        UpdateComplete(this, new BoolEventArgs(false));
                    }
                    break;
                    
                case ErrorCodes.ERROR_LOST_CONNECTION:
                    // The usb dongle lost connection with the remote device
                    if (DeviceDisconnected != null)
                        DeviceDisconnected(this, EventArgs.Empty);
                    if (StatusUpdate != null)
                        StatusUpdate(this, new MessageEventArgs("ERROR: Lost connection with remote device."));
                    break;

                case ErrorCodes.ERROR_CHECKSUM_FAIL:
                    if (expectedAck == AckTypes.LINE_ACK)
                    {
                        // Error introduced in transmission, try resending.
                        if (reSends < 2)
                        {
                            reSends++;
                            sendLine();
                        }
                        else
                        {
                            // Line failed checksum at remote 3 times, something wrong, disconnect.
                            if (DeviceDisconnected != null)
                                DeviceDisconnected(this, EventArgs.Empty);
                        }
                    }
                    else if (expectedAck == AckTypes.UPDATE_START_ACK)
                    {
                        if (reSends < 2)
                        {
                            reSends++;
                            flashRemote(autoVerify);
                        }
                        else
                        {
                            //Line failed checksum at remote 3 times, something wrong, disconnect.
                            if (DeviceDisconnected != null)
                                DeviceDisconnected(this, EventArgs.Empty);
                        }
                    }
                    break;

                case ErrorCodes.ERROR_ILLEGAL_ADDRESS:
                    // Sent line passed checksum test, but contained an illegal address. 
                    if (expectedAck == AckTypes.LINE_ACK)
                    {
                        if (StatusUpdate != null)
                            StatusUpdate(this, new MessageEventArgs("ERROR: Remote device reported verified line with invalid address. Aborting."));
                        if (DeviceDisconnected != null)
                            DeviceDisconnected(this, EventArgs.Empty);
                    }
                    break;

                default:
                    if (StatusUpdate != null)
                        StatusUpdate(this, new MessageEventArgs("Got unknown nack."));
                    break;
            }
        }

        // Called when cancel button of progress dialog pushed. 
        // Aborts transfers and sends new init to remote device.
        public void cancel()
        {
            StatusUpdate(this, new MessageEventArgs("Cancelling and reconnecting."));
            // Disable sending and receiving of data
            aborting = true;
            // Wait while other thread is finishing it's current calls.
            System.Threading.Thread.Sleep(200);
            // Enable communication and send a init command to restart the connection.
            aborting = false;
            byte[] transferData = new byte[this.usbHidPort1.SpecifiedDevice.OutputReportLength - 1];
            transferData[0] = (byte)Commands.CMD_INIT;
            expectedAck = AckTypes.INIT_ACK;
            SendDataAndWait(transferData);
        }

        // Initiate search for compatible RF device
        public void connect()
        {
            byte[] transferData = new byte[this.usbHidPort1.SpecifiedDevice.OutputReportLength-1];
            transferData[0] = (byte)Commands.CMD_INIT;
            SendDataAndWait(transferData);
            StatusUpdate(this, new MessageEventArgs("Searching for compatible device."));
            expectedAck = AckTypes.INIT_ACK;
        }

        // Disconnect from the RF device
        public void disconnect()
        {
            if (usbConnected)
            {
                byte[] transferData = new byte[this.usbHidPort1.SpecifiedDevice.OutputReportLength - 1];
                transferData[0] = (byte)Commands.CMD_EXIT;
                SendDataAndWait(transferData);
            }
            StatusUpdate(this, new MessageEventArgs("Disconnected."));
            this.DeviceDisconnected(this, new EventArgs());
        }
        #endregion

        #region Firmware update functions
        // Set firmware version which is sent upon firmware update init
        public void setVersion(byte ver)
        {
            newVersion = ver;
        }

        // Sends initiate update command along with number of bytes and hex lines, 
        // the starting long jump instruction of the Hex and a checksum.
        public void flashRemote(bool autoVerify)
        {
            this.autoVerify = autoVerify;
            if (validHexLoaded)
            {
                byte checksum = 0;
                byte[] transferData = new byte[this.usbHidPort1.SpecifiedDevice.OutputReportLength - 1];
                transferData[0] = (byte)Commands.CMD_UPDATE_START;

                // Split 16 bit bytecount in two bytes
                transferData[1] = (byte)(byteCount >> 8);
                transferData[2] = (byte)byteCount;

                // Send Long jump as part of init packet
                transferData[3] = longJumpAddress[0];
                transferData[4] = longJumpAddress[1];
                transferData[5] = longJumpAddress[2];

                // Byte indicating version of new firmware
                transferData[6] = newVersion;

                // Generate checksum for linecount and Long jump address
                for (int i = 1; i <= 6; i++)
                {
                    checksum += transferData[i];
                }
                transferData[7] = (byte)-checksum;

                if (StatusUpdate != null)
                    StatusUpdate(this, new MessageEventArgs("Initiating firmware update."));
                sentLines = 0;
                expectedAck = AckTypes.UPDATE_START_ACK;
                SendDataAndWait(transferData);
            }
            else
            {
                if (StatusUpdate != null)
                    StatusUpdate(this, new MessageEventArgs("ERROR: Tried flashing invalid hex."));
            }
        }

        // Send a Hex line
        private void sendLine()
        {
            byte[] transferData = new byte[this.usbHidPort1.SpecifiedDevice.OutputReportLength - 1];
            transferData[0] = (byte)Commands.CMD_WRITE;
            List<byte> tempLine = hexLines[sentLines];
            for (int B = 0; B < tempLine.Count; B++)
            {
                transferData[1 + B] = tempLine[B];
            }
            expectedAck = AckTypes.LINE_ACK;
            //StatusUpdate(this, new MessageEventArgs("Sending Line."));
            SendDataAndWait(transferData);

        }

        // Confirm to remote device that firmware was completely transmitted
        private void sendUpdateComplete()
        {
            byte[] transferData = new byte[this.usbHidPort1.SpecifiedDevice.OutputReportLength - 1];
            transferData[0] = (byte)Commands.CMD_UPDATE_COMPLETE;
            expectedAck = AckTypes.COMPLETE_ACK;
            SendDataAndWait(transferData);
        }
        #endregion

        #region Verification functions
        // Function initiating verification of remote flash memory
        public void startVerification()
        {
            if (validHexLoaded)
            {
                StatusUpdate(this, new MessageEventArgs("Started verification of remote firmware."));
                VerificationStarted(this, new EventArgs());
                // Initiate counter
                verifiedLines = 0;
                receivedLine = new byte[usbHidPort1.SpecifiedDevice.InputReportLength-1];
                requestLine();
            }
            else
            {
                StatusUpdate(this, new MessageEventArgs("ERROR: Tried validating remote hex with no file loaded."));
            }
        }

        // Compares received flash line with line from loaded hex file
        private void verifyFlashLine()
        {
            List<byte> tempLine = hexLines[verifiedLines];
            bool valid = true;
            for (int B = 0; B < tempLine[HEX_LINE_SIZE_INDEX]; B++)
            {
                if (tempLine[HEX_LINE_DATA_INDEX + B] != receivedLine[1 + B])
                {
                    valid = false;
                    break;
                }
            }
            if (valid) // Line was verified
            {
                verifiedLines++;
                UpdateProgressChanged(this, new ProgressEventArgs(verifiedLines / (double)lineCount));
                reRequesting = false;
                if (verifiedLines == lineCount)
                {
                    // Done
                    FlashVerified(this, new BoolEventArgs(true));
                    sendUpdateComplete();
                    StatusUpdate(this, new MessageEventArgs("Remote firmware integrity verified!"));
                    if (autoVerify)
                    {
                        sendUpdateComplete();
                        autoVerify = false;
                    }
                }
                else
                {
                    // Send next request
                    requestLine();
                }
            }
            else // Line failed verification
            {
                // Request line once more in case of communication error
                if (!reRequesting)
                {
                    reRequesting = true;
                    requestLine();
                }
                else
                {
                    // Notify gui of failed verification
                    FlashVerified(this, new BoolEventArgs(false));
                }
            }

        }

        // Starts verification, called once for every line. Sends requests for hex Lines to device.
        public void requestLine()
        {
            byte[] transferData = new byte[this.usbHidPort1.SpecifiedDevice.OutputReportLength-1];
            transferData[0] = (byte)Commands.CMD_READ;
            // Attach size of line and msb and lsb of address .
            transferData[1] = hexLines[verifiedLines][HEX_LINE_SIZE_INDEX];
            transferData[2] = hexLines[verifiedLines][HEX_LINE_MSB_ADDR_INDEX];
            transferData[3] = hexLines[verifiedLines][HEX_LINE_LSB_ADDR_INDEX];

            expectedAck = AckTypes.READ_ACK;

            SendDataAndWait(transferData);
        }
        #endregion

        #region Hex Functions
        // Loads intel hex file. Performs partitioning and verifies contents.
        public void loadHex(string hexFile)
        {
            int errLine;
            string status = "";
            bool valid = true;
            try
            {
                // Load file into array of bytes
                FileStream hexFileStream;
                hexFileStream = new FileStream(hexFile, FileMode.Open, FileAccess.Read);
                hexSize = (UInt16)hexFileStream.Length;
                hexArray = new byte[hexSize];
                hexFileStream.Read(hexArray, 0, hexSize);
                hexFileStream.Close();
            }
            catch (Exception e)
            {
                status = "ERROR: " + e.Message + "Could not read file.";
                valid = false;
            }
            if (valid)
            {
                // Parse array of character bytes into binary hex lines
                errLine = extractHexLines();
                if (errLine != -1)
                {
                    if (errLine == -2)
                        status = "ERROR: Hex file contained no end-of-file marker.";
                    else
                        status = "ERROR: Hex Line " + (errLine + 1) + ": Illegal Hex symbol.";
                    valid = false;
                }
            }
            if (valid)
            {
                // Controls checksum and size of loaded hex lines.
                // Looks for Starting long jump to be sent in init package.
                errLine = checkHexlines();
                if (errLine != -1)
                {
                    status = "ERROR: Hex line " + (errLine + 1) + ": Invalid checksum or size.";
                    valid = false;
                }
            }
            if (valid)
            {
                status = byteCount + " byte valid hex file loaded";
            }
            validHexLoaded = valid;
            if (StatusUpdate != null)
                StatusUpdate(this, new MessageEventArgs(status));
            if (HexVerified != null)
                HexVerified(this, new BoolEventArgs(valid));
        }

        // Verifies size and checksum bytes of hex file.
        // Returns -1 if successful, line number if error found.
        private int checkHexlines()
        {
            List<byte> line;
            byte checkSum;
            byteCount = 0;
            longJumpAddress = new byte[LONG_JUMP_INSTRUCTION_SIZE];

            for (int l = 0; l < lineCount; l++)
            {
                checkSum = 0;
                line = hexLines[l];
                if (line.Count != line[0] + 5)
                    return l;
                byteCount += line[0]; // Add to total byte size of program
                for (int B = 0; B < line.Count - 1; B++)
                {
                    checkSum += line[B];
                }
                byte check = (byte)(checkSum + line[line.Count - 1]);
                if (check != 0)
                    return l;
                // If address 0, store long jump address
                if (line[HEX_LINE_SIZE_INDEX] >= 3 &&
                    line[HEX_LINE_MSB_ADDR_INDEX] == 0 &&
                    line[HEX_LINE_LSB_ADDR_INDEX] == 0)
                {
                    longJumpAddress[0] = line[HEX_LINE_DATA_INDEX];
                    longJumpAddress[1] = line[HEX_LINE_DATA_INDEX + 1];
                    longJumpAddress[2] = line[HEX_LINE_DATA_INDEX + 2];
                }
            }
            return -1;
        }

        // Method parsing Intel Hex into binary format.
        // Returns -1 if successful, -2 if no EOF found, line number if error found.
        private int extractHexLines()
        {
            string tempString;
            byte tempByte;
            hexLines = new List<List<byte>>();
            UInt16 byteIndex = 0, lineBytes = 0;
            lineCount = 0;
            // File must start with colon
            if (hexArray[byteIndex] != ':')
                return 0;
            byteIndex++;
            hexLines.Add(new List<byte>());
            while (byteIndex < hexSize)
            {
                // Check that Hex line is not too long
                if (lineBytes > MAX_BYTES_PER_HEX_LINE)
                    return lineCount;
                switch ((char)hexArray[byteIndex])
                {
                    case '\n':
                    case '\r':
                        byteIndex++;
                        break;
                    case ':':
                        // Start next line after each colon.
                        lineCount++;
                        // Check for EOF line
                        if ((char)hexArray[byteIndex + 1] == '0' &&
                            (char)hexArray[byteIndex + 2] == '0')
                        {
                            return -1; // Return success
                        }
                        hexLines.Add(new List<byte>());
                        byteIndex++;
                        lineBytes = 0;
                        break;
                    default:
                        {
                            // Try parsing the two next chars as a hex value.
                            tempString = ((char)hexArray[byteIndex]).ToString() + ((char)hexArray[byteIndex + 1]).ToString();
                            if (byte.TryParse(tempString, System.Globalization.NumberStyles.HexNumber, null, out tempByte))
                            {
                                hexLines[lineCount].Add(tempByte);
                                byteIndex += 2; // 2 chars per binary byte
                                lineBytes++;
                            }
                            else
                            {
                                // The two chars could not be parsed into a hex value.
                                return lineCount;
                            }
                        } break;
                }
            }
            return -2;
        }
        #endregion

   }
}