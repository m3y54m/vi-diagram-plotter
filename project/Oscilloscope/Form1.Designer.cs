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
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.label8 = new System.Windows.Forms.Label();
            this.cmbPortName = new System.Windows.Forms.ComboBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.chkInvert = new System.Windows.Forms.CheckBox();
            this.numAmp = new System.Windows.Forms.NumericUpDown();
            this.rdoDC = new System.Windows.Forms.RadioButton();
            this.chkApply = new System.Windows.Forms.CheckBox();
            this.rdoHalfTri = new System.Windows.Forms.RadioButton();
            this.rdoHalfSin = new System.Windows.Forms.RadioButton();
            this.numFreq = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.rdoTri = new System.Windows.Forms.RadioButton();
            this.rdoSin = new System.Windows.Forms.RadioButton();
            this.lblFreq = new System.Windows.Forms.Label();
            this.lblAmp = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAmp)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFreq)).BeginInit();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 50;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // serialPort1
            // 
            this.serialPort1.BaudRate = 921600;
            this.serialPort1.PortName = "COM12";
            this.serialPort1.ReadBufferSize = 10000;
            this.serialPort1.ReceivedBytesThreshold = 9000;
            this.serialPort1.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.serialPort1_DataReceived);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.White;
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(12, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(400, 400);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.cmbPortName);
            this.groupBox1.Location = new System.Drawing.Point(433, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.groupBox1.Size = new System.Drawing.Size(239, 116);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Serial Port";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(14, 33);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(209, 23);
            this.button2.TabIndex = 22;
            this.button2.Text = "Scan Ports";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 74);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(57, 13);
            this.label8.TabIndex = 11;
            this.label8.Text = "Port Name";
            this.label8.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // cmbPortName
            // 
            this.cmbPortName.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPortName.FormattingEnabled = true;
            this.cmbPortName.Location = new System.Drawing.Point(114, 71);
            this.cmbPortName.Name = "cmbPortName";
            this.cmbPortName.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.cmbPortName.Size = new System.Drawing.Size(109, 21);
            this.cmbPortName.Sorted = true;
            this.cmbPortName.TabIndex = 10;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.chkInvert);
            this.groupBox2.Controls.Add(this.numAmp);
            this.groupBox2.Controls.Add(this.rdoDC);
            this.groupBox2.Controls.Add(this.chkApply);
            this.groupBox2.Controls.Add(this.rdoHalfTri);
            this.groupBox2.Controls.Add(this.rdoHalfSin);
            this.groupBox2.Controls.Add(this.numFreq);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.rdoTri);
            this.groupBox2.Controls.Add(this.rdoSin);
            this.groupBox2.Controls.Add(this.lblFreq);
            this.groupBox2.Controls.Add(this.lblAmp);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Location = new System.Drawing.Point(433, 144);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.groupBox2.Size = new System.Drawing.Size(239, 268);
            this.groupBox2.TabIndex = 13;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Signal Properties";
            // 
            // chkInvert
            // 
            this.chkInvert.AutoSize = true;
            this.chkInvert.Location = new System.Drawing.Point(180, 193);
            this.chkInvert.Name = "chkInvert";
            this.chkInvert.Size = new System.Drawing.Size(53, 17);
            this.chkInvert.TabIndex = 27;
            this.chkInvert.Text = "Invert";
            this.chkInvert.UseVisualStyleBackColor = true;
            this.chkInvert.CheckedChanged += new System.EventHandler(this.chkInvert_CheckedChanged);
            // 
            // numAmp
            // 
            this.numAmp.DecimalPlaces = 2;
            this.numAmp.Increment = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numAmp.Location = new System.Drawing.Point(14, 103);
            this.numAmp.Maximum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.numAmp.Name = "numAmp";
            this.numAmp.Size = new System.Drawing.Size(209, 20);
            this.numAmp.TabIndex = 26;
            this.numAmp.Value = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.numAmp.ValueChanged += new System.EventHandler(this.numAmp_ValueChanged);
            // 
            // rdoDC
            // 
            this.rdoDC.AutoSize = true;
            this.rdoDC.Location = new System.Drawing.Point(14, 51);
            this.rdoDC.Name = "rdoDC";
            this.rdoDC.Size = new System.Drawing.Size(40, 17);
            this.rdoDC.TabIndex = 25;
            this.rdoDC.Text = "DC";
            this.rdoDC.UseVisualStyleBackColor = true;
            this.rdoDC.CheckedChanged += new System.EventHandler(this.rdoDC_CheckedChanged);
            // 
            // chkApply
            // 
            this.chkApply.AutoSize = true;
            this.chkApply.Checked = true;
            this.chkApply.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkApply.Location = new System.Drawing.Point(14, 193);
            this.chkApply.Name = "chkApply";
            this.chkApply.Size = new System.Drawing.Size(101, 17);
            this.chkApply.TabIndex = 24;
            this.chkApply.Text = "Apply properties";
            this.chkApply.UseVisualStyleBackColor = true;
            // 
            // rdoHalfTri
            // 
            this.rdoHalfTri.AutoSize = true;
            this.rdoHalfTri.Location = new System.Drawing.Point(154, 51);
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
            this.rdoHalfSin.Location = new System.Drawing.Point(87, 51);
            this.rdoHalfSin.Name = "rdoHalfSin";
            this.rdoHalfSin.Size = new System.Drawing.Size(56, 17);
            this.rdoHalfSin.TabIndex = 22;
            this.rdoHalfSin.Text = "| Sine |";
            this.rdoHalfSin.UseVisualStyleBackColor = true;
            this.rdoHalfSin.CheckedChanged += new System.EventHandler(this.rdoHalfSin_CheckedChanged);
            // 
            // numFreq
            // 
            this.numFreq.DecimalPlaces = 2;
            this.numFreq.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numFreq.Location = new System.Drawing.Point(14, 155);
            this.numFreq.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numFreq.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.numFreq.Name = "numFreq";
            this.numFreq.Size = new System.Drawing.Size(209, 20);
            this.numFreq.TabIndex = 21;
            this.numFreq.Value = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.numFreq.ValueChanged += new System.EventHandler(this.numFreq_ValueChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 29);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(66, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Signal Type:";
            // 
            // rdoTri
            // 
            this.rdoTri.AutoSize = true;
            this.rdoTri.Location = new System.Drawing.Point(154, 29);
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
            this.rdoSin.Location = new System.Drawing.Point(87, 29);
            this.rdoSin.Name = "rdoSin";
            this.rdoSin.Size = new System.Drawing.Size(46, 17);
            this.rdoSin.TabIndex = 16;
            this.rdoSin.TabStop = true;
            this.rdoSin.Text = "Sine";
            this.rdoSin.UseVisualStyleBackColor = true;
            this.rdoSin.CheckedChanged += new System.EventHandler(this.rdoSin_CheckedChanged);
            // 
            // lblFreq
            // 
            this.lblFreq.AutoSize = true;
            this.lblFreq.Location = new System.Drawing.Point(11, 136);
            this.lblFreq.Name = "lblFreq";
            this.lblFreq.Size = new System.Drawing.Size(88, 13);
            this.lblFreq.TabIndex = 12;
            this.lblFreq.Text = "Frequncy: 0.1 Hz";
            // 
            // lblAmp
            // 
            this.lblAmp.AutoSize = true;
            this.lblAmp.Location = new System.Drawing.Point(11, 84);
            this.lblAmp.Name = "lblAmp";
            this.lblAmp.Size = new System.Drawing.Size(84, 13);
            this.lblAmp.TabIndex = 9;
            this.lblAmp.Text = "Amplitude: 0.5 V";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(14, 229);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(209, 23);
            this.button1.TabIndex = 8;
            this.button1.Text = "Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 424);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.pictureBox1);
            this.MaximumSize = new System.Drawing.Size(700, 462);
            this.MinimumSize = new System.Drawing.Size(700, 462);
            this.Name = "Form1";
            this.Text = "VI Plotter";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAmp)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numFreq)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox cmbPortName;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label lblAmp;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.RadioButton rdoTri;
        private System.Windows.Forms.RadioButton rdoSin;
        private System.Windows.Forms.Label lblFreq;
        private System.Windows.Forms.NumericUpDown numFreq;
        private System.Windows.Forms.RadioButton rdoHalfTri;
        private System.Windows.Forms.RadioButton rdoHalfSin;
        private System.Windows.Forms.CheckBox chkApply;
        private System.Windows.Forms.RadioButton rdoDC;
        private System.Windows.Forms.NumericUpDown numAmp;
        private System.Windows.Forms.CheckBox chkInvert;
    }
}

