using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using nRFupdate;
using System.Threading;


namespace nRFupdate
{
    public partial class ProgressForm : Form
    {
        private string activity = "";
        private bool inProgress = false;
        private nRFupdateControl usbDongle;
        private Thread abortThread;

        public ProgressForm(nRFupdateControl d)
        {
            InitializeComponent();
            usbDongle = d;
            d.UpdateProgressChanged += new EventHandler<ProgressEventArgs>(usbDongle_UpdateProgressChanged);
            d.FlashVerified += new EventHandler<BoolEventArgs>(usbDongle_FlashVerified);
            d.UpdateComplete += new EventHandler<BoolEventArgs>(usbDongle_UpdateComplete);
            d.UpdateStarted += new EventHandler(usbDongle_UpdateStarted);
            d.DongleDisconnected += new EventHandler(d_Disconnected);
            d.DeviceDisconnected += new EventHandler(d_Disconnected);
            d.VerificationStarted += new EventHandler(usbDongle_VerificationStarted);
        }

        void d_Disconnected(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(d_Disconnected), new object[] { sender, e });
            }
            else
            {

                inProgress = false;
                closeButton.Text = "Close";
                progressBar1.Value = 0;
                percentLabel.Text = "Connection lost!";
            }
        }

        void usbDongle_VerificationStarted(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(usbDongle_VerificationStarted), new object[] { sender, e });
            }
            else
            {
                closeButton.Text = "Cancel";
                inProgress = true;
                activity = "Verifying ";
                progressBar1.Value = 0;
                percentLabel.Text = activity + "0 %";
            }
        }

        void usbDongle_UpdateStarted(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(usbDongle_UpdateStarted), new object[] { sender, e });
            }
            else
            {
                closeButton.Text = "Cancel";
                inProgress = true;
                activity = "Programming ";
                progressBar1.Value = 0;
                percentLabel.Text = activity + "0 %";
            }
        }

        void usbDongle_UpdateComplete(object sender, BoolEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<BoolEventArgs>(usbDongle_UpdateComplete), new object[] { sender, e });
            }
            else
            {

                inProgress = false;
                closeButton.Text = "Close";
                if (e.Status() == true)
                {
                    percentLabel.Text = "Update Complete";
                }
                else
                {
                    progressBar1.Value = 0;
                    percentLabel.Text = "Update failed";
                }
            }
        }

        void usbDongle_FlashVerified(object sender, BoolEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<BoolEventArgs>(usbDongle_FlashVerified), new object[] { sender, e });
            }
            else
            {
                inProgress = false;
                closeButton.Text = "Close";
                if (e.Status() == true)
                {
                    percentLabel.Text = "Verification Done";
                }
                else
                {
                    progressBar1.Value = 0;
                    percentLabel.Text = "Verification failed!";
                }
            }
        }

        void usbDongle_UpdateProgressChanged(object sender, ProgressEventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler<ProgressEventArgs>(usbDongle_UpdateProgressChanged), new object[] { sender, e });
            }
            else
            {
                progressBar1.Value = (int)(e.Progress() * 100);
                percentLabel.Text = activity + progressBar1.Value + " %";
            }
        }

        private void cancelProgress()
        {
            usbDongle.cancel();
            stopProgressBar(this, EventArgs.Empty);
            
        }

        private void stopProgressBar(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                Invoke(new EventHandler(stopProgressBar), new object[] { sender, e });
            }
            else
            {
                progressBar1.Value = 0;
                percentLabel.Text = "Cancelled!";
                closeButton.Text = "Close";
                inProgress = false;
            }
        }
        private void closeButton_Click(object sender, EventArgs e)
        {
            if (inProgress)
            {
                abortThread = new Thread(new ThreadStart(cancelProgress));
                abortThread.Start();
            }
            else
            {
                this.Close();
            }
        }
    }
}
