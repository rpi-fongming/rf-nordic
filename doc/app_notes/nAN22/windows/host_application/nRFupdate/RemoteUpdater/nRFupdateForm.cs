using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace nRFupdate
{
    public partial class nRFupdateForm : Form
    {
        private nRFupdateControl usbDongle;
        private string filepath = "";
        private string filename = "";
        private bool fileLoaded = false;
        private bool deviceConnected = false;
        private bool searching = false;

        private ProgressForm pform1;

        public nRFupdateForm()
        {
            InitializeComponent();
            // Instantiate data dongle logic class
            usbDongle = new nRFupdateControl();

            // Add listeners to events triggered by USB logic
            usbDongle.DongleConnected += new EventHandler(usbDongle_Connected);
            usbDongle.DongleDisconnected += new EventHandler(usbDongle_Disconnected);
            usbDongle.DeviceConnected += new EventHandler<ConnectionEventArgs>(rfDevice_Connected);
            usbDongle.DeviceDisconnected += new EventHandler(rfDevice_Disconnected);
            usbDongle.HexVerified += new EventHandler<BoolEventArgs>(hex_Verified);
            usbDongle.UpdateStarted += new EventHandler(usbDongle_UpdateStarted);
            usbDongle.ReadyToRun += new EventHandler(usbDongle_ReadyToRun);
            
            // Status update event used to pass text messages to gui textbox
            usbDongle.StatusUpdate += new EventHandler<MessageEventArgs>(usbDongle_StatusUpdate);
            
            newVersionTextbox.KeyPress += new KeyPressEventHandler(newVersionTextbox_KeyPress);

            // Progress popup window
            pform1 = new ProgressForm(usbDongle);
        }

        
        // Display status message from USB class
        void usbDongle_StatusUpdate(object sender, MessageEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<MessageEventArgs>(usbDongle_StatusUpdate), new object[] { sender, e });
            }
            else
            {
                textBox1.AppendText(e.Message() + '\n');
            }
        }

        // Updates gui on dongle disconnection
        void usbDongle_Disconnected(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(usbDongle_Disconnected), new object[] { sender, e });
            }
            else
            {
                rfDevice_Disconnected(this, new EventArgs());
                dongleStatusLabel.Text = "Disconnected";
                dongleStatusLabel.ForeColor = Color.Red;
                connectButton.Enabled = false;
            }
        }

        // Updates gui on dongle connection
        void usbDongle_Connected(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(usbDongle_Connected), new object[] { sender, e });
            }
            else
            {
                dongleStatusLabel.Text = "Connected";
                dongleStatusLabel.ForeColor = Color.Green;
                connectButton.Enabled = true;
            }
        }

        // Updates gui on remote device connection
        void rfDevice_Connected(object sender, ConnectionEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<ConnectionEventArgs>(rfDevice_Connected), new object[] { sender, e });
            }
            else
            {
                // Display the value of received byte as model. 
                // Could be used as index for string model descriptors.
                modelTextbox.Text = e.Model().ToString();
                modelTextbox.Enabled = true;
                // Displays a 1 byte value indicating the version of the current installed firmware.
                if (e.Version() != 0xFF)
                    versionTextbox.Text = e.Version().ToString();
                else
                    versionTextbox.Text = "N/A";
                versionTextbox.Enabled = true;
                connectButton.Text = "Disconnect";
                deviceStatusLabel.Text = "Connected";
                deviceStatusLabel.ForeColor = Color.Green;
                deviceConnected = true;
                if (fileLoaded == true)
                {
                    // Enable these buttons if a valid hex file is also loaded.
                    updateButton.Enabled = true;
                    verifyButton.Enabled = true;
                }
            }
        }

        // Open the progress window when starting update
        void usbDongle_UpdateStarted(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new EventHandler(usbDongle_UpdateStarted), new object[] { sender, e });
            }
            else
            {
                if (!pform1.Visible)
                {
                    pform1.ShowDialog();
                }
            }
        }
        
        // Update gui when connection with remote device lost
        void rfDevice_Disconnected(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(rfDevice_Disconnected), new object[] { sender, e });
            }
            else
            {
                modelTextbox.Clear();
                modelTextbox.Enabled = false;
                versionTextbox.Clear();
                versionTextbox.Enabled = false;
                deviceStatusLabel.Text = "Disconnected";
                deviceConnected = false;
                searching = false;
                deviceStatusLabel.ForeColor = Color.Red;
                connectButton.Enabled = true;
                connectButton.Text = "Connect";
                updateButton.Enabled = false;
                verifyButton.Enabled = false;
                runButton.Enabled = false;
            }
        }

        // Update gui when loaded hex is verified
        void hex_Verified(object sender, BoolEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<BoolEventArgs>(hex_Verified), new object[] { sender, e });
            }
            else
            {
                if (e.Status()) // Hex was valid
                {
                    fileLoaded = true;
                    if (deviceConnected)
                    {
                        // Enable buttons if device is also connected
                        updateButton.Enabled = true;
                        verifyButton.Enabled = true;
                    }
                    validFileLabel.Text = "Valid";
                    validFileLabel.ForeColor = Color.Green;
                }
                else // Hex was not valid
                {
                    fileLoaded = false;
                    updateButton.Enabled = false;
                    verifyButton.Enabled = false;
                    validFileLabel.Text = "Invalid";
                    validFileLabel.ForeColor = Color.Red;
                }
            }
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            usbDongle.usbHidPort1.RegisterHandle(Handle);
        }

        protected override void WndProc(ref Message m)
        {
            try
            {
                usbDongle.usbHidPort1.ParseMessages(ref m);
                base.WndProc(ref m); // pass message on to base form
            }
            catch (Exception e)
            {

            }
        }

        // Notify device and close application
        private void exitButton_Click(object sender, EventArgs e)
        {
            usbDongle.disconnect();
            this.Close();
        }

        private void connectButton_Click(object sender, EventArgs e)
        {
            // When already connected or searching, change function to disconnect
            if (deviceConnected || searching)
            {
                usbDongle.disconnect();
                searching = false;
            }
            // If not, notify LU1+ to start scanning for device
            else
            {
                deviceStatusLabel.ForeColor = Color.Blue;
                deviceStatusLabel.Text = "Searching..";
                searching = true;
                usbDongle.connect();
                connectButton.Text = "Cancel";
            }
        }

        // Open an open file dialog and forward filename
        private void loadButton_Click(object sender, EventArgs e)
        {
            if (openBinaryDialog.ShowDialog() == DialogResult.OK)
            {
                filepath = openBinaryDialog.FileName;
                filename = openBinaryDialog.SafeFileName;
                newFWtext.Text = filename;
                usbDongle.loadHex(filepath);
            }

        }

        // Start sending firmware
        private void updateButton_Click(object sender, EventArgs e)
        {
            usbDongle.flashRemote(verifyCheckbox.Checked);
            runButton.Enabled = false;
        }

        // Start verifying firmware on remote device and open progress bar.
        private void verifyButton_Click(object sender, EventArgs e)
        {
            usbDongle.startVerification();
            if (!pform1.Visible)
                pform1.ShowDialog(this);
        }

        // Verify that the typed version value fits in one byte
        void newVersionTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            byte num = 0;
            if (byte.TryParse(newVersionTextbox.Text + e.KeyChar.ToString(), out num))
                usbDongle.setVersion(num);
            else
                e.Handled = true;
        }

        // Run the succesfully installed firmware. 
        // Same functionality as disconnect button, but more intuitive in gui.
        private void runButton_Click(object sender, EventArgs e)
        {
            usbDongle.disconnect();
            runButton.Enabled = false;
        }

        // Enable the run button when firmware has been confirmed installed.
        void usbDongle_ReadyToRun(object sender, EventArgs e)
        {
            if (InvokeRequired)
                Invoke(new EventHandler(usbDongle_ReadyToRun), new object[] { sender, e });
            else
                runButton.Enabled = true;
        }

    }
}
