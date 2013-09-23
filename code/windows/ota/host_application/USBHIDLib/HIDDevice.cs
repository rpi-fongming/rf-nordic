using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace USBHIDLib
{
	#region Custom exception
	/// <summary>
	/// Generic HID device exception
	/// </summary>
    public class HIDDeviceException : ApplicationException
    {
        public HIDDeviceException(string strMessage) : base(strMessage) { }

        public static HIDDeviceException GenerateWithWinError(string strMessage)
        {
            return new HIDDeviceException(string.Format("Msg:{0} WinEr:{1:X8}", strMessage, Marshal.GetLastWin32Error()));
        }

        public static HIDDeviceException GenerateError(string strMessage)
        {
            return new HIDDeviceException(string.Format("Msg:{0}", strMessage));
        }
    }
	#endregion
	/// <summary>
	/// Abstract HID device : Derive your new device controller class from this
	/// </summary>
    public abstract class HIDDevice : Win32Usb, IDisposable
    {
		#region Privates variables
		/// <summary>Filestream we can use to read/write from</summary>
        private FileStream m_oFile;
		/// <summary>Length of input report : device gives us this</summary>
		private int m_nInputReportLength;
		/// <summary>Length if output report : device gives us this</summary>
		private int m_nOutputReportLength;
        
		/// <summary>Handle to the device</summary>
		private IntPtr m_hHandle;
		#endregion

        #region IDisposable Members
		/// <summary>
		/// Dispose method
		/// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
		/// <summary>
		/// Disposer called by both dispose and finalise
		/// </summary>
		/// <param name="bDisposing">True if disposing</param>
        protected virtual void Dispose(bool bDisposing)
        {
            try
            {
                if (bDisposing)	// if we are disposing, need to close the managed resources
                {
                    if (m_oFile != null)
                    {
                        m_oFile.Close();
                        m_oFile = null;
                    }
                }
                if (m_hHandle != IntPtr.Zero)	// Dispose and finalize, get rid of unmanaged resources
                {

                    CloseHandle(m_hHandle);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        #endregion

		#region Privates/protected
		/// <summary>
		/// Initialises the device
		/// </summary>
		/// <param name="strPath">Path to the device</param>
		private void Initialise(string strPath)
		{
			// Create the file from the device path
            m_hHandle = CreateFile(strPath, GENERIC_READ | GENERIC_WRITE, 0, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_OVERLAPPED, IntPtr.Zero);

            if ( m_hHandle != InvalidHandleValue || m_hHandle == null)	// if the open worked...
			{
				IntPtr lpData;
				if (HidD_GetPreparsedData(m_hHandle, out lpData))	// get windows to read the device data into an internal buffer
				{
                    try
                    {
                        HidCaps oCaps;
                        HidP_GetCaps(lpData, out oCaps);	// extract the device capabilities from the internal buffer
                        m_nInputReportLength = oCaps.InputReportByteLength;	// get the input...
                        m_nOutputReportLength = oCaps.OutputReportByteLength;	// ... and output report lengths
                        m_oFile = new FileStream(new SafeFileHandle(m_hHandle, false), FileAccess.Read | FileAccess.Write, m_nInputReportLength, true);

                        BeginAsyncRead();	// kick off the first asynchronous read                              
                    }
                    catch (Exception ex)
                    {
                        throw HIDDeviceException.GenerateWithWinError("Failed to get the detailed data from the hid.");
                    }
					finally
					{
						HidD_FreePreparsedData(ref lpData);	// before we quit the funtion, we must free the internal buffer reserved in GetPreparsedData
					}
				}
				else	// GetPreparsedData failed? Chuck an exception
				{
					throw HIDDeviceException.GenerateWithWinError("GetPreparsedData failed");
				}
			}
			else	// File open failed? Chuck an exception
			{
				m_hHandle = IntPtr.Zero;
				throw HIDDeviceException.GenerateWithWinError("Failed to create device file");
			}
		}
		/// <summary>
		/// Kicks off an asynchronous read which completes when data is read or when the device
		/// is disconnected. Uses a callback.
		/// </summary>
        private void BeginAsyncRead()
        {
                byte[] arrInputReport = new byte[m_nInputReportLength];
                // put the buff we used to receive the stuff as the async state then we can get at it when the read completes

                m_oFile.BeginRead(arrInputReport, 0, m_nInputReportLength, new AsyncCallback(ReadCompleted), arrInputReport);
        }
		/// <summary>
		/// Callback for above. Care with this as it will be called on the background thread from the async read
		/// </summary>
		/// <param name="iResult">Async result parameter</param>
        protected void ReadCompleted(IAsyncResult iResult)
        {
            byte[] arrBuff = (byte[])iResult.AsyncState;	// retrieve the read buffer
            try
            {
                m_oFile.EndRead(iResult);	// call end read : this throws any exceptions that happened during the read
                try
                {
					HandleDataReceived(arrBuff);	// pass the new input report on to the higher level handler
                }
                finally
                {
                    BeginAsyncRead();	// when all that is done, kick off another read for the next report
                }                
            }
            catch(IOException ex)	// if we got an IO exception, the device was removed
            {
                HandleDeviceRemoved();
                if (OnDeviceRemoved != null)
                {
                    OnDeviceRemoved(this, new EventArgs());
                }
                Dispose();
            }
        }
		/// <summary>
		/// Write an output report to the device.
		/// </summary>
		/// <param name="oOutRep">Output report to write</param>
        protected void Write(byte[] oOutRep)
        {
            try
            {
                m_oFile.Write(oOutRep, 0, OutputReportLength);
            }
            catch (IOException ex)
            {
                //Console.WriteLine(ex.ToString());
                // The device was removed!
                throw new HIDDeviceException("The device was probably removed");
            }
			catch(Exception exx)
			{
                Console.WriteLine(exx.ToString());	
			}
        }
		/// <summary>
		/// virtual handler for any action to be taken when data is received. Override to use.
		/// </summary>
		/// <param name="oInRep">The input report that was received</param>
		protected virtual void HandleDataReceived(byte[] oInRep)
		{
		}
		/// <summary>
		/// Virtual handler for any action to be taken when a device is removed. Override to use.
		/// </summary>
		protected virtual void HandleDeviceRemoved()
		{
		}
		/// <summary>
		/// Helper method to return the device path given a DeviceInterfaceData structure and an InfoSet handle.
		/// Used in 'FindDevice' so check that method out to see how to get an InfoSet handle and a DeviceInterfaceData.
		/// </summary>
		/// <param name="hInfoSet">Handle to the InfoSet</param>
		/// <param name="oInterface">DeviceInterfaceData structure</param>
		/// <returns>The device path or null if there was some problem</returns>
        private static string GetDevicePath(IntPtr hInfoSet, ref DeviceInterfaceData oInterface)
        {
            DeviceInterfaceDetailData oDetail = new DeviceInterfaceDetailData();
            // Size workaround
            if (IntPtr.Size == 8)
                oDetail.Size = 8;
            else
                oDetail.Size = 5;
            Console.WriteLine("Size of struct: {0}", Marshal.SizeOf(oDetail)); // 4 + 256 = 260

            uint nRequiredSize = 0;

            // Error 0
            if (!SetupDiGetDeviceInterfaceDetail(hInfoSet, ref oInterface, IntPtr.Zero, 0, ref nRequiredSize, IntPtr.Zero))
                // Error 122 - ERROR_INSUFFICIENT_BUFFER (not a problem, just used to set nRequiredSize)
                if (SetupDiGetDeviceInterfaceDetail(hInfoSet, ref oInterface, ref oDetail, nRequiredSize, ref nRequiredSize, IntPtr.Zero))
                    return oDetail.DevicePath;
            // Error 1784 - ERROR_INVALID_USER_BUFFER (unless size=5 on 32bit, size=8 on 64bit)
            return null;
        }
		#endregion

		#region Public static
		/// <summary>
		/// Finds a device given its PID and VID
		/// </summary>
		/// <param name="nVid">Vendor id for device (VID)</param>
		/// <param name="nPid">Product id for device (PID)</param>
		/// <param name="oType">Type of device class to create</param>
        /// <param name="nInterface"></param>
		/// <returns>A new device class of the given type or null</returns>
		public static HIDDevice FindDevice(int nVid, int nPid, int nInterface, Type oType)
        {
            string strPath = string.Empty;
            string strSearch = string.Empty;

            if (nInterface < 0x100)
            {
                strSearch = string.Format("vid_{0:x4}&pid_{1:x4}&mi_{2:x2}", nVid, nPid, nInterface); // first, build the path search string
            }
            else
            {
                strSearch = string.Format("vid_{0:x4}&pid_{1:x4}", nVid, nPid); // first, build the path search string
            }

            Guid gHid = HIDGuid;
            //HidD_GetHidGuid(out gHid);	// next, get the GUID from Windows that it uses to represent the HID USB interface
            IntPtr hInfoSet = SetupDiGetClassDevs(ref gHid, null, IntPtr.Zero, DIGCF_DEVICEINTERFACE | DIGCF_PRESENT);	// this gets a list of all HID devices currently connected to the computer (InfoSet)
            try
            {
                DeviceInterfaceData oInterface = new DeviceInterfaceData();	// build up a device interface data block
                oInterface.Size = Marshal.SizeOf(oInterface);
                // Now iterate through the InfoSet memory block assigned within Windows in the call to SetupDiGetClassDevs
                // to get device details for each device connected
                int nIndex = 0;
                while (SetupDiEnumDeviceInterfaces(hInfoSet, 0, ref gHid, (uint)nIndex, ref oInterface))	// this gets the device interface information for a device at index 'nIndex' in the memory block
                {
                    string strDevicePath = GetDevicePath(hInfoSet, ref oInterface);	// get the device path (see helper method 'GetDevicePath')
                    if (strDevicePath.IndexOf(strSearch) >= 0)	// do a string search, if we find the VID/PID string then we found our device!
                    {
                        HIDDevice oNewDevice = (HIDDevice)Activator.CreateInstance(oType);	// create an instance of the class for this device
                        oNewDevice.Initialise(strDevicePath);	// initialise it with the device path
                        return oNewDevice;	// and return it
                    }
                    nIndex++;	// if we get here, we didn't find our device. So move on to the next one.
                }
            }
            catch(Exception ex)
            {
                throw HIDDeviceException.GenerateError(ex.ToString());
                //Console.WriteLine(ex.ToString());
            }
            finally
            {
				// Before we go, we have to free up the InfoSet memory reserved by SetupDiGetClassDevs
                SetupDiDestroyDeviceInfoList(hInfoSet);
            }
            return null;	// oops, didn't find our device
        }
		#endregion

		#region Publics
		/// <summary>
		/// Event handler called when device has been removed
		/// </summary>
		public event EventHandler OnDeviceRemoved;
		/// <summary>
		/// Accessor for output report length
		/// </summary>
		public int OutputReportLength
		{
			get
			{
				return m_nOutputReportLength;
			}
		}
		/// <summary>
		/// Accessor for input report length
		/// </summary>
		public int InputReportLength
		{
			get
			{
				return m_nInputReportLength;
			}
		}
		#endregion
    }
}
