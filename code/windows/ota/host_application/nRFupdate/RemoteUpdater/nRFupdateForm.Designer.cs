namespace nRFupdate
{
    partial class nRFupdateForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(nRFupdateForm));
            this.titleLabel = new System.Windows.Forms.Label();
            this.instructionLabel1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.exitButton = new System.Windows.Forms.Button();
            this.updateButton = new System.Windows.Forms.Button();
            this.connectButton = new System.Windows.Forms.Button();
            this.loadButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.dongleStatusLabel = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.newVersionTextbox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.verifyCheckbox = new System.Windows.Forms.CheckBox();
            this.verifyButton = new System.Windows.Forms.Button();
            this.runButton = new System.Windows.Forms.Button();
            this.validFileLabel = new System.Windows.Forms.Label();
            this.newFWtext = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.deviceStatusLabel = new System.Windows.Forms.Label();
            this.versionTextbox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.modelTextbox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.openBinaryDialog = new System.Windows.Forms.OpenFileDialog();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // titleLabel
            // 
            this.titleLabel.AutoSize = true;
            this.titleLabel.Font = new System.Drawing.Font("Arial", 21.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(68, 9);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(238, 33);
            this.titleLabel.TabIndex = 0;
            this.titleLabel.Text = "nRF Update - FM";
            this.titleLabel.Click += new System.EventHandler(this.titleLabel_Click);
            // 
            // instructionLabel1
            // 
            this.instructionLabel1.AutoSize = true;
            this.instructionLabel1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.instructionLabel1.Location = new System.Drawing.Point(6, 16);
            this.instructionLabel1.Name = "instructionLabel1";
            this.instructionLabel1.Size = new System.Drawing.Size(350, 26);
            this.instructionLabel1.TabIndex = 1;
            this.instructionLabel1.Text = "To connect to remote device, make sure USB RF adapter is connected, \r\npush connec" +
                "t button and turn device off and on again.";
            // 
            // groupBox1
            // 
            this.groupBox1.AutoSize = true;
            this.groupBox1.Controls.Add(this.instructionLabel1);
            this.groupBox1.Location = new System.Drawing.Point(12, 67);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(375, 58);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Connection instructions";
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.Location = new System.Drawing.Point(297, 387);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(75, 23);
            this.exitButton.TabIndex = 3;
            this.exitButton.Text = "Exit";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.exitButton_Click);
            // 
            // updateButton
            // 
            this.updateButton.Enabled = false;
            this.updateButton.Location = new System.Drawing.Point(101, 76);
            this.updateButton.Name = "updateButton";
            this.updateButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.updateButton.Size = new System.Drawing.Size(76, 23);
            this.updateButton.TabIndex = 4;
            this.updateButton.Text = "Start update";
            this.updateButton.UseVisualStyleBackColor = true;
            this.updateButton.Click += new System.EventHandler(this.updateButton_Click);
            // 
            // connectButton
            // 
            this.connectButton.Enabled = false;
            this.connectButton.Location = new System.Drawing.Point(285, 49);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(75, 23);
            this.connectButton.TabIndex = 5;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // loadButton
            // 
            this.loadButton.Location = new System.Drawing.Point(285, 17);
            this.loadButton.Name = "loadButton";
            this.loadButton.Size = new System.Drawing.Size(75, 23);
            this.loadButton.TabIndex = 8;
            this.loadButton.Text = "Load";
            this.loadButton.UseVisualStyleBackColor = true;
            this.loadButton.Click += new System.EventHandler(this.loadButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.dongleStatusLabel);
            this.groupBox2.Location = new System.Drawing.Point(243, 331);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(144, 49);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "RF Adapter";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 21);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(40, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Status:";
            // 
            // dongleStatusLabel
            // 
            this.dongleStatusLabel.AutoSize = true;
            this.dongleStatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dongleStatusLabel.ForeColor = System.Drawing.Color.Red;
            this.dongleStatusLabel.Location = new System.Drawing.Point(52, 19);
            this.dongleStatusLabel.Name = "dongleStatusLabel";
            this.dongleStatusLabel.Size = new System.Drawing.Size(82, 15);
            this.dongleStatusLabel.TabIndex = 0;
            this.dongleStatusLabel.Text = "Disconnected";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.newVersionTextbox);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.verifyCheckbox);
            this.groupBox3.Controls.Add(this.verifyButton);
            this.groupBox3.Controls.Add(this.runButton);
            this.groupBox3.Controls.Add(this.updateButton);
            this.groupBox3.Controls.Add(this.validFileLabel);
            this.groupBox3.Controls.Add(this.newFWtext);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.loadButton);
            this.groupBox3.Location = new System.Drawing.Point(12, 215);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(375, 110);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "New firmware";
            // 
            // newVersionTextbox
            // 
            this.newVersionTextbox.Location = new System.Drawing.Point(177, 50);
            this.newVersionTextbox.Name = "newVersionTextbox";
            this.newVersionTextbox.Size = new System.Drawing.Size(48, 20);
            this.newVersionTextbox.TabIndex = 18;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(111, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "New firmware version:";
            // 
            // verifyCheckbox
            // 
            this.verifyCheckbox.AutoSize = true;
            this.verifyCheckbox.Location = new System.Drawing.Point(19, 80);
            this.verifyCheckbox.Name = "verifyCheckbox";
            this.verifyCheckbox.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.verifyCheckbox.Size = new System.Drawing.Size(76, 17);
            this.verifyCheckbox.TabIndex = 16;
            this.verifyCheckbox.Text = "Auto verify";
            this.verifyCheckbox.UseVisualStyleBackColor = true;
            // 
            // verifyButton
            // 
            this.verifyButton.Enabled = false;
            this.verifyButton.Location = new System.Drawing.Point(183, 76);
            this.verifyButton.Name = "verifyButton";
            this.verifyButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.verifyButton.Size = new System.Drawing.Size(74, 23);
            this.verifyButton.TabIndex = 14;
            this.verifyButton.Text = "Verify";
            this.verifyButton.UseVisualStyleBackColor = true;
            this.verifyButton.Click += new System.EventHandler(this.verifyButton_Click);
            // 
            // runButton
            // 
            this.runButton.Enabled = false;
            this.runButton.Location = new System.Drawing.Point(263, 76);
            this.runButton.Name = "runButton";
            this.runButton.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.runButton.Size = new System.Drawing.Size(97, 23);
            this.runButton.TabIndex = 15;
            this.runButton.Text = "Run Firmware";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // validFileLabel
            // 
            this.validFileLabel.AutoSize = true;
            this.validFileLabel.ForeColor = System.Drawing.SystemColors.AppWorkspace;
            this.validFileLabel.Location = new System.Drawing.Point(237, 22);
            this.validFileLabel.Name = "validFileLabel";
            this.validFileLabel.Size = new System.Drawing.Size(27, 13);
            this.validFileLabel.TabIndex = 13;
            this.validFileLabel.Text = "N/A";
            // 
            // newFWtext
            // 
            this.newFWtext.Enabled = false;
            this.newFWtext.Location = new System.Drawing.Point(88, 19);
            this.newFWtext.Name = "newFWtext";
            this.newFWtext.ReadOnly = true;
            this.newFWtext.Size = new System.Drawing.Size(137, 20);
            this.newFWtext.TabIndex = 10;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 22);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Loaded hex:";
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.connectButton);
            this.groupBox4.Controls.Add(this.deviceStatusLabel);
            this.groupBox4.Controls.Add(this.versionTextbox);
            this.groupBox4.Controls.Add(this.label1);
            this.groupBox4.Controls.Add(this.modelTextbox);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Location = new System.Drawing.Point(12, 128);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(375, 81);
            this.groupBox4.TabIndex = 11;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Remote Device";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(42, 54);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Status:";
            // 
            // deviceStatusLabel
            // 
            this.deviceStatusLabel.AutoSize = true;
            this.deviceStatusLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.deviceStatusLabel.ForeColor = System.Drawing.Color.Red;
            this.deviceStatusLabel.Location = new System.Drawing.Point(85, 52);
            this.deviceStatusLabel.Name = "deviceStatusLabel";
            this.deviceStatusLabel.Size = new System.Drawing.Size(82, 15);
            this.deviceStatusLabel.TabIndex = 14;
            this.deviceStatusLabel.Text = "Disconnected";
            // 
            // versionTextbox
            // 
            this.versionTextbox.Enabled = false;
            this.versionTextbox.Location = new System.Drawing.Point(311, 19);
            this.versionTextbox.Name = "versionTextbox";
            this.versionTextbox.ReadOnly = true;
            this.versionTextbox.Size = new System.Drawing.Size(49, 20);
            this.versionTextbox.TabIndex = 15;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(216, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Firmware version:";
            // 
            // modelTextbox
            // 
            this.modelTextbox.Enabled = false;
            this.modelTextbox.Location = new System.Drawing.Point(88, 19);
            this.modelTextbox.Name = "modelTextbox";
            this.modelTextbox.ReadOnly = true;
            this.modelTextbox.Size = new System.Drawing.Size(122, 20);
            this.modelTextbox.TabIndex = 13;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Device Model:";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(12, 11);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(50, 50);
            this.pictureBox1.TabIndex = 12;
            this.pictureBox1.TabStop = false;
            // 
            // openBinaryDialog
            // 
            this.openBinaryDialog.DefaultExt = "hex";
            this.openBinaryDialog.Filter = "Firmware binary (*.hex)|*.hex";
            this.openBinaryDialog.Title = "Select firmware binary ";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(12, 331);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(225, 78);
            this.textBox1.TabIndex = 13;
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.descriptionLabel.Location = new System.Drawing.Point(70, 42);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(212, 19);
            this.descriptionLabel.TabIndex = 14;
            this.descriptionLabel.Text = "Firmware update over-the-air";
            // 
            // nRFupdateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(399, 422);
            this.Controls.Add(this.descriptionLabel);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.textBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "nRFupdateForm";
            this.Text = "Nordic Semiconductor nRF Update";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label instructionLabel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Button updateButton;
        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.Button loadButton;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label dongleStatusLabel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.TextBox newFWtext;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox modelTextbox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.OpenFileDialog openBinaryDialog;
        private System.Windows.Forms.TextBox versionTextbox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label deviceStatusLabel;
        private System.Windows.Forms.Label validFileLabel;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.CheckBox verifyCheckbox;
        private System.Windows.Forms.Button runButton;
        private System.Windows.Forms.Button verifyButton;
        private System.Windows.Forms.TextBox newVersionTextbox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label descriptionLabel;
    }
}

