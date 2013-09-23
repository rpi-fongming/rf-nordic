using System;
using System.Collections.Generic;
using System.Text;

namespace USBHIDLib
{
    public class DataRecievedEventArgs : EventArgs
    {
        public readonly byte[] data;

        public DataRecievedEventArgs(byte[] newData)
        {
            // Received data starts at second element because of USB command.
            // We abstract this away by copying only the actual data elements.
            data = new byte[newData.Length - 1];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = newData[i + 1];
            }
        }
    }

    public delegate void DataRecievedEventHandler(object sender, DataRecievedEventArgs args);
    public delegate void DataSendEventHandler(object sender, EventArgs args);

    public class SpecifiedDevice : HIDDevice
    {
        public event DataRecievedEventHandler DataRecieved;
        public event DataSendEventHandler DataSend;

        private byte[] outputReportBuffer;

        public static SpecifiedDevice FindSpecifiedDevice(int vendor_id, int product_id, int pref_interface)
        {
            return (SpecifiedDevice)FindDevice(vendor_id, product_id, pref_interface, typeof(SpecifiedDevice));
        }

        protected override void HandleDataReceived(byte[] oInRep)
        {
            // Fire the event handler if assigned
            if (DataRecieved != null)
            {
                DataRecieved(this, new DataRecievedEventArgs(oInRep));
            }
        }

        public void SendData(byte[] data)
        {
            outputReportBuffer = new byte[OutputReportLength];
            // Copy data into USB report format, which must have the first element free for USB command.
            for (int i = 0; i < OutputReportLength-1; i++)
            {
                outputReportBuffer[i+1] = data[i];
            }
                try
                {
                    Write(outputReportBuffer); // write the output report
                    if (DataSend != null)
                    {
                        DataSend(this, EventArgs.Empty);
                    }
                }
                catch (HIDDeviceException ex)
                {
                    // Device may have been removed!
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
        }

        protected override void Dispose(bool bDisposing)
        {
            base.Dispose(bDisposing);
        }

    }
}
