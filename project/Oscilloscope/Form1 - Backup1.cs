using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Linq;
using System.Globalization;

namespace Oscilloscope
{
    public partial class Form1 : Form
    {
        int x, y, tempx = 255, tempy;
        SolidBrush sb = new SolidBrush(Color.Red);
        Pen pen = new Pen(Color.Red);
        System.IO.StreamWriter objWriter;
        static int ScreenWidth = 400;
        static int ScreenHeight = 400;
        Bitmap bmpScreen = new Bitmap(400, 400);
        Graphics gfx;
        static UInt32 sampleCount = 1000;
        UInt16[] currentArray;
        UInt16[] voltageArray;
        bool buttonStart = false;
        bool operationDone = false;
        int periodCount = 0;
        int offset = 32767;

        Byte desiredType = 0;
        float desiredAmplitude = 0.5f;
        float desiredPeriod = 10.0f;
        UInt32 desiredFrequency = 1000;
        UInt16 desiredRate = 1000;
        SByte desiredSign = 1;
        bool IsHighFreq = false;

        Byte tempType;
        float tempAmplitude;
        float tempPeriod;
        UInt32 tempFrequency;
        UInt16 tempRate;
        SByte tempSign;
        bool tempIsHighFreq;

        PersianCalendar pc = new PersianCalendar();
        DateTime thisDate;

        bool overCurrent = false;

        Queue<byte> qBuffer = new Queue<byte>();
        AutoResetEvent dataAvailable = new AutoResetEvent(false);
        Thread processThread;
        bool threadStarted = false;
        byte data;
        byte[] receivedPacket = new byte[8];
        bool packetProcessing = false;
        byte packetIndex = 0;
        UInt32 count = 0;
        UInt32 totalSamples = 0;

        Queue<Point> VI_Buffer = new Queue<Point>(); // voltage and current adc values buffer

        Boolean scanSerialPorts()
        {
            string[] ports = SerialPort.GetPortNames();

            cmbPortName.Items.Clear();
            cmbPortName.Items.AddRange(ports);

            if (ports.GetLength(0) == 0)
            {
                return false;
            }
            else
            {
                cmbPortName.Text = ports[ports.GetLength(0) - 1];
                return true;
            }
        }

        public Form1()
        {
            InitializeComponent();
            scanSerialPorts();
            gfx = Graphics.FromImage(bmpScreen);
            ScopeInit();
            pictureBox1.Image = bmpScreen;
        }

        void StartStop()
        {
            if (!buttonStart)
            {
                gfx.Clear(pictureBox1.BackColor);
                ScopeInit();
                pictureBox1.Image = bmpScreen;
                buttonStart = true;
                btnStart.Text = "Stop";

                if (!Directory.Exists("OUTPUT"))
                {
                    Directory.CreateDirectory("OUTPUT");
                }

                overCurrent = false;
                timer1.Enabled = true;

            }
            else
            {
                buttonStart = false;

                stopThread();

                operationDone = false;
                count = 0;
                periodCount = 0;
                btnStart.Text = "Start";

                timer1.Enabled = false;

                if (!mySerialPort.IsOpen)
                    mySerialPort.Open();

                mySerialPort.Write("P"); // Stop singnal genarating

            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            StartStop();
        }

        byte getHexValue(byte chr)
        {
            byte n = 16;

            switch (Convert.ToChar(chr))
            {
                case 'F':
                    n = 15;
                    break;
                case 'E':
                    n = 14;
                    break;
                case 'D':
                    n = 13;
                    break;
                case 'C':
                    n = 12;
                    break;
                case 'B':
                    n = 11;
                    break;
                case 'A':
                    n = 10;
                    break;
                case '9':
                    n = 9;
                    break;
                case '8':
                    n = 8;
                    break;
                case '7':
                    n = 7;
                    break;
                case '6':
                    n = 6;
                    break;
                case '5':
                    n = 5;
                    break;
                case '4':
                    n = 4;
                    break;
                case '3':
                    n = 3;
                    break;
                case '2':
                    n = 2;
                    break;
                case '1':
                    n = 1;
                    break;
                case '0':
                    n = 0;
                    break;
            }

            return n;
        }

        private void mySerialPort_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            this.BeginInvoke(new EventHandler(getChunk));
        }

        private void getChunk(object sender, EventArgs e)
        {
            try
            {
                if (mySerialPort.BytesToRead > 0)
                {
                    byte[] chunk = new byte[mySerialPort.BytesToRead];
                    mySerialPort.Read(chunk, 0, chunk.Length);

                    lock (qBuffer)
                    {
                        for (int i = 0; i < chunk.Length; i++)
                            qBuffer.Enqueue(chunk[i]);
                    }

                    dataAvailable.Set();
                }
            }
            catch
            {
                //...
            }
        }

        //This thread processes the stored chunks doing the less locking possible
        void processData(object state)
        {
            while (true)
            {
                dataAvailable.WaitOne();

                lock (qBuffer)
                {
                    while (qBuffer.Count() > 0)  // process each single byte
                    {
                        data = qBuffer.Dequeue();

                        if (!packetProcessing)
                        {
                            if (data == Convert.ToByte('S'))
                            {
                                packetProcessing = true;
                                packetIndex = 0;
                            }
                            else if (data == Convert.ToByte('P')) // P indicates that one period has ended
                            {
                                totalSamples = count;
                                count = 0;
                                operationDone = true;
                            }
                            else if (data == Convert.ToByte('W')) // W indicates error condition
                            {
                                // Over-Current or Over-Voltage
                                totalSamples = count;
                                this.BeginInvoke(new EventHandler(test1));
                                count = 0;
                                overCurrent = true;
                                operationDone = true;
                            }
                        }
                        else if (packetProcessing)
                        {
                            receivedPacket[packetIndex] = data;

                            packetIndex++;

                            if (packetIndex == 8)
                            {
                                packetProcessing = false;

                                UInt16[] voltageHexDigit = new UInt16[4];
                                UInt16[] currentHexDigit = new UInt16[4];

                                UInt16 adc0 = 0;
                                UInt16 adc1 = 0;

                                voltageHexDigit[3] = getHexValue(receivedPacket[0]);
                                voltageHexDigit[2] = getHexValue(receivedPacket[1]);
                                voltageHexDigit[1] = getHexValue(receivedPacket[2]);
                                voltageHexDigit[0] = getHexValue(receivedPacket[3]);

                                currentHexDigit[3] = getHexValue(receivedPacket[4]);
                                currentHexDigit[2] = getHexValue(receivedPacket[5]);
                                currentHexDigit[1] = getHexValue(receivedPacket[6]);
                                currentHexDigit[0] = getHexValue(receivedPacket[7]);

                                if (voltageHexDigit[3] == 16 || voltageHexDigit[2] == 16 || voltageHexDigit[1] == 16 || voltageHexDigit[0] == 16 || currentHexDigit[3] == 16 || currentHexDigit[2] == 16 || currentHexDigit[1] == 16 || currentHexDigit[0] == 16)
                                {
                                    // ERROR
                                }
                                else
                                {
                                    adc0 = (UInt16)(voltageHexDigit[3] * 4096 + voltageHexDigit[2] * 256 + voltageHexDigit[1] * 16 + voltageHexDigit[0]);
                                    adc1 = (UInt16)(currentHexDigit[3] * 4096 + currentHexDigit[2] * 256 + currentHexDigit[1] * 16 + currentHexDigit[0]);
                                }

                                voltageArray[count] = adc0;
                                currentArray[count] = adc1;

                                count++;
                            }
                        }
                    }
                }
            }
        }
        private void test1(object sender, EventArgs e)
        {
            label8.Text = totalSamples.ToString();
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            float tempFloat;

            if (!operationDone)
            {
                if (!threadStarted)
                {
                    if (!overCurrent)
                    {
                        startThread();

                        if (chkApply.Checked)
                        {
                            Byte cmdType = desiredType;
                            UInt32 cmdAmplitude;
                            UInt32 cmdPeriod;
                            UInt32 cmdFrequency;
                            UInt16 cmdRate;
                            Byte cmdConfig;

                            unsafe
                            {
                                cmdType = desiredType;

                                tempFloat = desiredAmplitude / 8.0f;
                                cmdAmplitude = *((UInt32*)(&tempFloat));

                                tempFloat = desiredPeriod;
                                cmdPeriod = *((UInt32*)(&tempFloat));

                                cmdFrequency = desiredFrequency;

                                cmdRate = desiredRate;

                                cmdConfig = 0;
                                if (desiredSign == 1) cmdConfig |= 0x80;
                                if (IsHighFreq) cmdConfig |= 0x40;
                            }

                            tempType = desiredType;
                            tempAmplitude = desiredAmplitude;
                            tempPeriod = desiredPeriod;
                            tempFrequency = desiredFrequency;
                            tempRate = desiredRate;
                            tempSign = desiredSign;
                            tempIsHighFreq = IsHighFreq;

                            UInt16 golbalRate = desiredRate;
                            float globalPeriod = desiredPeriod;

                            sampleCount = (UInt32)(golbalRate * globalPeriod);

                            currentArray = new UInt16[2 * sampleCount];
                            voltageArray = new UInt16[2 * sampleCount];

                            // Send signal properties
                            mySerialPort.Write("Q" + cmdType.ToString("X2") + cmdAmplitude.ToString("X8") + cmdPeriod.ToString("X8") + cmdFrequency.ToString("X8") + cmdRate.ToString("X4") + cmdConfig.ToString("X2"));
                        }

                        // Start singnal genarating
                        mySerialPort.Write("G");
                    }
                    else
                    {
                        StartStop();
                    }
                }
            }
            else
            {
                stopThread();

                operationDone = false;

                periodCount++;

                int name = 0;

                while (File.Exists("OUTPUT\\PERIOD_" + periodCount.ToString() + "_" + name.ToString() + ".txt"))
                {
                    name++;
                }

                objWriter = new StreamWriter("OUTPUT\\PERIOD_" + periodCount.ToString() + "_" + name.ToString() + ".txt");
                objWriter.WriteLine("V,I");

                gfx.Clear(pictureBox1.BackColor);
                ScopeInit();

                string tempStr = "";

                switch (tempType)
                {
                    case 0:
                        tempStr = "Sine";
                        break;
                    case 1:
                        tempStr = "Triangle";
                        break;
                    case 2:
                        tempStr = "| Sine |";
                        break;
                    case 3:
                        tempStr = "| Triangle |";
                        break;
                    case 4:
                        tempStr = "DC";
                        break;
                }

                gfx.DrawString("Period Number: " + periodCount.ToString(), new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 10));
                gfx.DrawString("Signal Type: " + tempStr, new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 30));
                gfx.DrawString("Amplitude: " + tempAmplitude.ToString() + " V", new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 50));
                gfx.DrawString("Period: " + tempPeriod.ToString() + " s", new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 70));

                thisDate = DateTime.Now;
                gfx.DrawString(pc.GetYear(thisDate).ToString("D4") + "/" + pc.GetMonth(thisDate).ToString("D2") + "/" + pc.GetDayOfMonth(thisDate).ToString("D2") + " " + pc.GetHour(thisDate).ToString("D2") + ":" + pc.GetMinute(thisDate).ToString("D2") + ":" + pc.GetSecond(thisDate).ToString("D2"),
                                new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(270, 370));

                if (overCurrent)
                {
                    gfx.DrawString("Over-Current", new Font("Tahoma", 10, FontStyle.Bold), new SolidBrush(Color.Red), new Point(10, 100));
                }

                for (int i = 0; i < totalSamples; i++)
                {
                    UInt16 adcVoltage = voltageArray[i];
                    UInt16 adcCurrent = currentArray[i];

                    double voltage = -(((double)(adcVoltage - offset) / 65535) * 3.3) * 4 * 1.325421391778252936 - 0.003021378303794;
                    double current = -(((double)(adcCurrent - offset) / 65535) * 3.3) * 4 * 0.001338338929378022 + 6.051442370329619e-07;

                    objWriter.WriteLine(voltage.ToString() + "," + current.ToString());
                    //objWriter.WriteLine(adcVoltage.ToString() + "," + adcCurrent.ToString());

                    x = -(int)(((double)(voltageArray[i] - offset) / 65535) * 420) + 199;
                    y = (int)(((double)(currentArray[i] - offset) / 65535) * 420) + 199;

                    if (i != 0)
                    {
                        gfx.DrawLine(pen, new Point(tempx, tempy), new Point(x, y));
                    }

                    tempx = x;
                    tempy = y;
                }

                pictureBox1.Image = bmpScreen;

                int name2 = 0;

                while (File.Exists("OUTPUT\\PERIOD_" + periodCount.ToString() + "_" + name2.ToString() + ".gif"))
                {
                    name2++;
                }

                bmpScreen.Save("OUTPUT\\PERIOD_" + periodCount.ToString() + "_" + name2.ToString() + ".gif", System.Drawing.Imaging.ImageFormat.Gif);

                objWriter.Close();
            }
        }


        private void ScopeInit()
        {
            Pen tpen = new Pen(Color.FromArgb(230, 230, 230));
            Font tfont = new Font("Segoe UI", 8);

            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;

            int ky = ScreenHeight / 20;
            for (int i = 0; i * ky <= ScreenHeight; i++)
            {
                drawFormat.Alignment = StringAlignment.Far;
                drawFormat.FormatFlags = StringFormatFlags.NoWrap;
                gfx.DrawLine(tpen, new Point(0, i * ky - 1), new Point(ScreenWidth, i * ky - 1));
            }

            int kx = ScreenWidth / 20;
            for (int i = 0; i * kx <= ScreenWidth; i++)
            {
                gfx.DrawLine(tpen, new Point(i * kx - 1, 0), new Point(i * kx - 1, ScreenHeight));
            }

            gfx.DrawLine(new Pen(Color.FromArgb(100, 100, 100)), new Point(0, 199), new Point(400, 199));
            gfx.DrawLine(new Pen(Color.FromArgb(100, 100, 100)), new Point(199, 0), new Point(199, 400));
        }

        private void btnScanPorts_Click(object sender, EventArgs e)
        {
            if (!scanSerialPorts())
                MessageBox.Show("No serial port found!");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mySerialPort.IsOpen)
            {
                try 
                {
                    mySerialPort.Close(); 
                }
                catch 
                { 
                    MessageBox.Show("Port can't be closed."); 
                }
            }
        }

        private void numPeriod_ValueChanged(object sender, EventArgs e)
        {
            desiredPeriod = (float)numPeriod.Value;
            lblPeriod.Text = "Period: " + desiredPeriod.ToString() + " s";
        }

        private void numFreq_ValueChanged(object sender, EventArgs e)
        {
            desiredFrequency = (UInt32)numFreq.Value;
            lblFreq.Text = "Frequency: " + desiredFrequency.ToString() + " Hz";
        }

        private void rdoSin_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoSin.Checked)
                desiredType = 0;
            else if (rdoTri.Checked)
                desiredType = 1;
            else if (rdoHalfSin.Checked)
                desiredType = 2;
            else if (rdoHalfTri.Checked)
                desiredType = 3;
            else if (rdoDC.Checked)
                desiredType = 4;
        }

        private void rdoTri_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoSin.Checked)
                desiredType = 0;
            else if (rdoTri.Checked)
                desiredType = 1;
            else if (rdoHalfSin.Checked)
                desiredType = 2;
            else if (rdoHalfTri.Checked)
                desiredType = 3;
            else if (rdoDC.Checked)
                desiredType = 4;
        }

        private void rdoHalfTri_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoSin.Checked)
                desiredType = 0;
            else if (rdoTri.Checked)
                desiredType = 1;
            else if (rdoHalfSin.Checked)
                desiredType = 2;
            else if (rdoHalfTri.Checked)
                desiredType = 3;
            else if (rdoDC.Checked)
                desiredType = 4;
        }

        private void rdoHalfSin_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoSin.Checked)
                desiredType = 0;
            else if (rdoTri.Checked)
                desiredType = 1;
            else if (rdoHalfSin.Checked)
                desiredType = 2;
            else if (rdoHalfTri.Checked)
                desiredType = 3;
            else if (rdoDC.Checked)
                desiredType = 4;
        }

        private void rdoDC_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoSin.Checked)
                desiredType = 0;
            else if (rdoTri.Checked)
                desiredType = 1;
            else if (rdoHalfSin.Checked)
                desiredType = 2;
            else if (rdoHalfTri.Checked)
                desiredType = 3;
            else if (rdoDC.Checked)
                desiredType = 4;
        }

        private void numAmp_ValueChanged(object sender, EventArgs e)
        {
            desiredAmplitude = (float)numAmp.Value;
            lblAmp.Text = "Amplitude: " + desiredAmplitude.ToString() + " V";
        }

        private void chkInvert_CheckedChanged(object sender, EventArgs e)
        {
            if (chkInvert.Checked) desiredSign = -1; else desiredSign = 1;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (mySerialPort.IsOpen)
            {
                try
                {
                    mySerialPort.Write("P"); // Stop singnal genarating

                    mySerialPort.Close();

                    lblPortStatus.BackColor = Color.Red;
                    lblPortStatus.Text = mySerialPort.PortName.ToString() + " Disconnected";

                    disconnectedMode();
                }
                catch
                {
                    btnConnect.Text = "Disconnect";
                    lblPortStatus.BackColor = Color.Green;
                    lblPortStatus.Text = mySerialPort.PortName.ToString() + " can't be closed.";

                    connectedMode();
                }
            }
            else
            {
                try
                {
                    mySerialPort.PortName = cmbPortName.Text;
                    mySerialPort.ReadBufferSize = 1000000;

                    mySerialPort.Open();
                    mySerialPort.DiscardInBuffer();

                    lblPortStatus.BackColor = Color.Green;
                    lblPortStatus.Text = mySerialPort.PortName.ToString() + " Connected";

                    connectedMode();
                }
                catch
                {
                    lblPortStatus.BackColor = Color.Red;
                    lblPortStatus.Text = mySerialPort.PortName.ToString() + " can't be opened!";

                    disconnectedMode();
                }
            }
        }

        void startThread()
        {
            processThread = new Thread(processData);
            processThread.IsBackground = true; // terminate the thread when form is closed
            processThread.Start();
            threadStarted = true;
        }

        void stopThread()
        {
            if (threadStarted)
            {
                processThread.Abort();
                threadStarted = false;
            }
        }

        void disconnectedMode()
        {
            btnConnect.Text = "Connect";

            grpConfig.Enabled = false;

            cmbPortName.Enabled = true;
            btnScanPorts.Enabled = true;

        }

        void connectedMode()
        {
            btnConnect.Text = "Disconnect";

            grpConfig.Enabled = true;

            cmbPortName.Enabled = false;
            btnScanPorts.Enabled = false;
        }

        private void rdoSingle_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoSingle.Checked)
            {
                IsHighFreq = false;
                grpOnePeriod.Enabled = true;
                grpPeriodic.Enabled = false;
            }

            else if (rdoPeriodic.Checked)
            {
                IsHighFreq = true;
                grpOnePeriod.Enabled = false;
                grpPeriodic.Enabled = true;
            }
        }

        private void rdoPeriodic_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoSingle.Checked)
            {
                IsHighFreq = false;
                grpOnePeriod.Enabled = true;
                grpPeriodic.Enabled = false;
            }

            else if (rdoPeriodic.Checked)
            {
                IsHighFreq = true;
                grpOnePeriod.Enabled = false;
                grpPeriodic.Enabled = true;
            }
        }

    }
}
