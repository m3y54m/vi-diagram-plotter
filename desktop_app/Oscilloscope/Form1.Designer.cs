namespace Oscilloscope
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.mySerialPort = new System.IO.Ports.SerialPort(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblPortStatus = new System.Windows.Forms.Label();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnScanPorts = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.cmbPortName = new System.Windows.Forms.ComboBox();
            this.grpOnePeriod = new System.Windows.Forms.GroupBox();
            this.rdoDC = new System.Windows.Forms.RadioButton();
            this.rdoHalfTri = new System.Windows.Forms.RadioButton();
            this.rdoHalfSin = new System.Windows.Forms.RadioButton();
            this.numPeriod = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.rdoTri = new System.Windows.Forms.RadioButton();
            this.rdoSin = new System.Windows.Forms.RadioButton();
            this.lblPeriod = new System.Windows.Forms.Label();
            this.numAmp = new System.Windows.Forms.NumericUpDown();
            this.lblAmp = new System.Windows.Forms.Label();
            this.numFreq = new System.Windows.Forms.NumericUpDown();
            this.lblFreq = new System.Windows.Forms.Label();
            this.chkInvert = new System.Windows.Forms.CheckBox();
            this.chkApply = new System.Windows.Forms.CheckBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.grpPeriodic = new System.Windows.Forms.GroupBox();
            this.grpMode = new System.Windows.Forms.GroupBox();
            this.rdoPeriodic = new System.Windows.Forms.RadioButton();
            this.rdoSingle = new System.Windows.Forms.RadioButton();
            this.grpConfig = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.grpOnePeriod.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPeriod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAmp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFreq)).BeginInit();
            this.grpPeriodic.SuspendLayout();
            this.grpMode.SuspendLayout();
            this.grpConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 50;
            // 
            // mySerialPort
            // 
            this.mySerialPort.BaudRate = 921600;
            this.mySerialPort.ReadBufferSize = 1000000;
            this.mySerialPort.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.mySerialPort_DataReceived);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(399, 399);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblPortStatus);
            this.groupBox1.Controls.Add(this.btnConnect);
            this.groupBox1.Controls.Add(this.btnScanPorts);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.cmbPortName);
            this.groupBox1.Location = new System.Drawing.Point(12, 419);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.groupBox1.Size = new System.Drawing.Size(676, 66);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Serial Port";
            // 
            // lblPortStatus
            // 
            this.lblPortStatus.BackColor = System.Drawing.Color.Orange;
            this.lblPortStatus.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPortStatus.ForeColor = System.Drawing.Color.White;
            this.lblPortStatus.Location = new System.Drawing.Point(328, 26);
            this.lblPortStatus.Name = "lblPortStatus";
            this.lblPortStatus.Size = new System.Drawing.Size(236, 21);
            this.lblPortStatus.TabIndex = 27;
            this.lblPortStatus.Text = "You must first connect to device!";
            this.lblPortStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(570, 24);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(92, 25);
            this.btnConnect.TabIndex = 23;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnScanPorts
            // 
            this.btnScanPorts.Location = new System.Drawing.Point(241, 24);
            this.btnScanPorts.Name = "btnScanPorts";
            this.btnScanPorts.Size = new System.Drawing.Size(79, 25);
            this.btnScanPorts.TabIndex = 22;
            this.btnScanPorts.Text = "Scan Ports";
            this.btnScanPorts.UseVisualStyleBackColor = true;
            this.btnScanPorts.Click += new System.EventHandler(this.btnScanPorts_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(15, 30);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(60, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Port Name:";
            this.label8.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // cmbPortName
            // 
            this.cmbPortName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPortName.FormattingEnabled = true;
            this.cmbPortName.Location = new System.Drawing.Point(84, 26);
            this.cmbPortName.Name = "cmbPortName";
            this.cmbPortName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmbPortName.Size = new System.Drawing.Size(151, 21);
            this.cmbPortName.Sorted = true;
            this.cmbPortName.TabIndex = 10;
            // 
            // grpOnePeriod
            // 
            this.grpOnePeriod.Controls.Add(this.rdoDC);
            this.grpOnePeriod.Controls.Add(this.rdoHalfTri);
            this.grpOnePeriod.Controls.Add(this.rdoHalfSin);
            this.grpOnePeriod.Controls.Add(this.numPeriod);
            this.grpOnePeriod.Controls.Add(this.label4);
            this.grpOnePeriod.Controls.Add(this.rdoTri);
            this.grpOnePeriod.Controls.Add(this.rdoSin);
            this.grpOnePeriod.Controls.Add(this.lblPeriod);
            this.grpOnePeriod.Location = new System.Drawing.Point(11, 73);
            this.grpOnePeriod.Name = "grpOnePeriod";
            this.grpOnePeriod.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.grpOnePeriod.Size = new System.Drawing.Size(239, 125);
            this.grpOnePeriod.TabIndex = 13;
            this.grpOnePeriod.TabStop = false;
            this.grpOnePeriod.Text = "Single Period";
            // 
            // rdoDC
            // 
            this.rdoDC.AutoSize = true;
            this.rdoDC.Location = new System.Drawing.Point(14, 45);
            this.rdoDC.Name = "rdoDC";
            this.rdoDC.Size = new System.Drawing.Size(40, 17);
            this.rdoDC.TabIndex = 25;
            this.rdoDC.Text = "DC";
            this.rdoDC.UseVisualStyleBackColor = true;
            this.rdoDC.CheckedChanged += new System.EventHandler(this.rdoDC_CheckedChanged);
            // 
            // rdoHalfTri
            // 
            this.rdoHalfTri.AutoSize = true;
            this.rdoHalfTri.Location = new System.Drawing.Point(154, 45);
            this.rdoHalfTri.Name = "rdoHalfTri";
            this.rdoHalfTri.Size = new System.Drawing.Size(73, 17);
            this.rdoHalfTri.TabIndex = 23;
            this.rdoHalfTri.Text = "| Triangle |";
            this.rdoHalfTri.UseVisualStyleBackColor = true;
            this.rdoHalfTri.CheckedChanged += new System.EventHandler(this.rdoHalfTri_CheckedChanged);
            // 
            // rdoHalfSin
            // 
            this.rdoHalfSin.AutoSize = true;
            this.rdoHalfSin.Location = new System.Drawing.Point(87, 45);
            this.rdoHalfSin.Name = "rdoHalfSin";
            this.rdoHalfSin.Size = new System.Drawing.Size(56, 17);
            this.rdoHalfSin.TabIndex = 22;
            this.rdoHalfSin.Text = "| Sine |";
            this.rdoHalfSin.UseVisualStyleBackColor = true;
            this.rdoHalfSin.CheckedChanged += new System.EventHandler(this.rdoHalfSin_CheckedChanged);
            // 
            // numPeriod
            // 
            this.numPeriod.DecimalPlaces = 2;
            this.numPeriod.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numPeriod.Location = new System.Drawing.Point(14, 90);
            this.numPeriod.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numPeriod.Name = "numPeriod";
            this.numPeriod.Size = new System.Drawing.Size(209, 20);
            this.numPeriod.TabIndex = 21;
            this.numPeriod.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numPeriod.ValueChanged += new System.EventHandler(this.numPeriod_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 23);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Signal Type:";
            // 
            // rdoTri
            // 
            this.rdoTri.AutoSize = true;
            this.rdoTri.Location = new System.Drawing.Point(154, 23);
            this.rdoTri.Name = "rdoTri";
            this.rdoTri.Size = new System.Drawing.Size(63, 17);
            this.rdoTri.TabIndex = 17;
            this.rdoTri.Text = "Triangle";
            this.rdoTri.UseVisualStyleBackColor = true;
            this.rdoTri.CheckedChanged += new System.EventHandler(this.rdoTri_CheckedChanged);
            // 
            // rdoSin
            // 
            this.rdoSin.AutoSize = true;
            this.rdoSin.Checked = true;
            this.rdoSin.Location = new System.Drawing.Point(87, 23);
            this.rdoSin.Name = "rdoSin";
            this.rdoSin.Size = new System.Drawing.Size(46, 17);
            this.rdoSin.TabIndex = 16;
            this.rdoSin.TabStop = true;
            this.rdoSin.Text = "Sine";
            this.rdoSin.UseVisualStyleBackColor = true;
            this.rdoSin.CheckedChanged += new System.EventHandler(this.rdoSin_CheckedChanged);
            // 
            // lblPeriod
            // 
            this.lblPeriod.AutoSize = true;
            this.lblPeriod.Location = new System.Drawing.Point(11, 71);
            this.lblPeriod.Name = "lblPeriod";
            this.lblPeriod.Size = new System.Drawing.Size(63, 13);
            this.lblPeriod.TabIndex = 12;
            this.lblPeriod.Text = "Period: 10 s";
            // 
            // numAmp
            // 
            this.numAmp.DecimalPlaces = 2;
            this.numAmp.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numAmp.Location = new System.Drawing.Point(12, 309);
            this.numAmp.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numAmp.Name = "numAmp";
            this.numAmp.Size = new System.Drawing.Size(238, 20);
            this.numAmp.TabIndex = 26;
            this.numAmp.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numAmp.ValueChanged += new System.EventHandler(this.numAmp_ValueChanged);
            // 
            // lblAmp
            // 
            this.lblAmp.AutoSize = true;
            this.lblAmp.Location = new System.Drawing.Point(11, 290);
            this.lblAmp.Name = "lblAmp";
            this.lblAmp.Size = new System.Drawing.Size(84, 13);
            this.lblAmp.TabIndex = 9;
            this.lblAmp.Text = "Amplitude: 0.5 V";
            // 
            // numFreq
            // 
            this.numFreq.DecimalPlaces = 2;
            this.numFreq.Location = new System.Drawing.Point(14, 41);
            this.numFreq.Maximum = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            this.numFreq.Name = "numFreq";
            this.numFreq.Size = new System.Drawing.Size(209, 20);
            this.numFreq.TabIndex = 29;
            this.numFreq.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numFreq.ValueChanged += new System.EventHandler(this.numFreq_ValueChanged);
            // 
            // lblFreq
            // 
            this.lblFreq.AutoSize = true;
            this.lblFreq.Location = new System.Drawing.Point(11, 22);
            this.lblFreq.Name = "lblFreq";
            this.lblFreq.Size = new System.Drawing.Size(91, 13);
            this.lblFreq.TabIndex = 28;
            this.lblFreq.Text = "Frequency: 10 Hz";
            // 
            // chkInvert
            // 
            this.chkInvert.AutoSize = true;
            this.chkInvert.Location = new System.Drawing.Point(200, 343);
            this.chkInvert.Name = "chkInvert";
            this.chkInvert.Size = new System.Drawing.Size(53, 17);
            this.chkInvert.TabIndex = 27;
            this.chkInvert.Text = "Invert";
            this.chkInvert.UseVisualStyleBackColor = true;
            this.chkInvert.CheckedChanged += new System.EventHandler(this.chkInvert_CheckedChanged);
            // 
            // chkApply
            // 
            this.chkApply.AutoSize = true;
            this.chkApply.Checked = true;
            this.chkApply.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkApply.Location = new System.Drawing.Point(12, 343);
            this.chkApply.Name = "chkApply";
            this.chkApply.Size = new System.Drawing.Size(101, 17);
            this.chkApply.TabIndex = 24;
            this.chkApply.Text = "Apply properties";
            this.chkApply.UseVisualStyleBackColor = true;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(11, 369);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(239, 23);
            this.btnStart.TabIndex = 8;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // grpPeriodic
            // 
            this.grpPeriodic.Controls.Add(this.lblFreq);
            this.grpPeriodic.Controls.Add(this.numFreq);
            this.grpPeriodic.Enabled = false;
            this.grpPeriodic.Location = new System.Drawing.Point(11, 204);
            this.grpPeriodic.Name = "grpPeriodic";
            this.grpPeriodic.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.grpPeriodic.Size = new System.Drawing.Size(239, 74);
            this.grpPeriodic.TabIndex = 30;
            this.grpPeriodic.TabStop = false;
            this.grpPeriodic.Text = "Priodic Pulse";
            // 
            // grpMode
            // 
            this.grpMode.Controls.Add(this.rdoPeriodic);
            this.grpMode.Controls.Add(this.rdoSingle);
            this.grpMode.Location = new System.Drawing.Point(12, 14);
            this.grpMode.Name = "grpMode";
            this.grpMode.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.grpMode.Size = new System.Drawing.Size(239, 53);
            this.grpMode.TabIndex = 31;
            this.grpMode.TabStop = false;
            this.grpMode.Text = "Mode";
            // 
            // rdoPeriodic
            // 
            this.rdoPeriodic.AutoSize = true;
            this.rdoPeriodic.Location = new System.Drawing.Point(136, 22);
            this.rdoPeriodic.Name = "rdoPeriodic";
            this.rdoPeriodic.Size = new System.Drawing.Size(92, 17);
            this.rdoPeriodic.TabIndex = 17;
            this.rdoPeriodic.Text = "Periodic Pulse";
            this.rdoPeriodic.UseVisualStyleBackColor = true;
            this.rdoPeriodic.CheckedChanged += new System.EventHandler(this.rdoPeriodic_CheckedChanged);
            // 
            // rdoSingle
            // 
            this.rdoSingle.AutoSize = true;
            this.rdoSingle.Checked = true;
            this.rdoSingle.Location = new System.Drawing.Point(15, 20);
            this.rdoSingle.Name = "rdoSingle";
            this.rdoSingle.Size = new System.Drawing.Size(86, 17);
            this.rdoSingle.TabIndex = 16;
            this.rdoSingle.TabStop = true;
            this.rdoSingle.Text = "Single period";
            this.rdoSingle.UseVisualStyleBackColor = true;
            this.rdoSingle.CheckedChanged += new System.EventHandler(this.rdoSingle_CheckedChanged);
            // 
            // grpConfig
            // 
            this.grpConfig.Controls.Add(this.numAmp);
            this.grpConfig.Controls.Add(this.grpMode);
            this.grpConfig.Controls.Add(this.chkApply);
            this.grpConfig.Controls.Add(this.grpPeriodic);
            this.grpConfig.Controls.Add(this.btnStart);
            this.grpConfig.Controls.Add(this.lblAmp);
            this.grpConfig.Controls.Add(this.grpOnePeriod);
            this.grpConfig.Controls.Add(this.chkInvert);
            this.grpConfig.Enabled = false;
            this.grpConfig.Location = new System.Drawing.Point(424, 6);
            this.grpConfig.Margin = new System.Windows.Forms.Padding(0);
            this.grpConfig.Name = "grpConfig";
            this.grpConfig.Padding = new System.Windows.Forms.Padding(0);
            this.grpConfig.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.grpConfig.Size = new System.Drawing.Size(264, 405);
            this.grpConfig.TabIndex = 32;
            this.grpConfig.TabStop = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(703, 498);
            this.Controls.Add(this.grpConfig);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "VI Plotter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.grpOnePeriod.ResumeLayout(false);
            this.grpOnePeriod.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numPeriod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAmp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFreq)).EndInit();
            this.grpPeriodic.ResumeLayout(false);
            this.grpPeriodic.PerformLayout();
            this.grpMode.ResumeLayout(false);
            this.grpMode.PerformLayout();
            this.grpConfig.ResumeLayout(false);
            this.grpConfig.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.IO.Ports.SerialPort mySerialPort;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cmbPortName;
        private System.Windows.Forms.Button btnScanPorts;
        private System.Windows.Forms.GroupBox grpOnePeriod;
        private System.Windows.Forms.Label lblAmp;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton rdoTri;
        private System.Windows.Forms.RadioButton rdoSin;
        private System.Windows.Forms.Label lblPeriod;
        private System.Windows.Forms.NumericUpDown numPeriod;
        private System.Windows.Forms.RadioButton rdoHalfTri;
        private System.Windows.Forms.RadioButton rdoHalfSin;
        private System.Windows.Forms.CheckBox chkApply;
        private System.Windows.Forms.RadioButton rdoDC;
        private System.Windows.Forms.NumericUpDown numAmp;
        private System.Windows.Forms.CheckBox chkInvert;
        private System.Windows.Forms.Label lblPortStatus;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.NumericUpDown numFreq;
        private System.Windows.Forms.Label lblFreq;
        private System.Windows.Forms.GroupBox grpPeriodic;
        private System.Windows.Forms.GroupBox grpMode;
        private System.Windows.Forms.RadioButton rdoPeriodic;
        private System.Windows.Forms.RadioButton rdoSingle;
        private System.Windows.Forms.GroupBox grpConfig;
    }
}

