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
        Bitmap bmpScreen = new Bitmap(399, 399);
        Graphics gfx;
        bool buttonStart = false;
        bool operationDone = false;
        int periodCount = 0;
        int testCount = 0;
        int adcCurrentOffset = 2047;
        int adcVoltageOffset = 2047;
        int dacOffset = 512;
        float VREF = 2.495f;

        double currentCorrectionFactor = 1.022328;
        double voltageCorrectionFactor = 1;

        float maxAmplitude = 2.5f;
        //float maxAmplitude = 10.0f;

        float outputVoltageCorrectionFactor = 1.0075567f;

        Byte desiredType = 0;
        float desiredAmplitude = 1.0f;
        float desiredPeriod = 1.0f;
        UInt32 desiredFrequency = 100;
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

        Point VI;
        Queue<Point> VI_Buffer = new Queue<Point>(); // voltage and current adc values buffer. x -> V , y -> I
        UInt32 pointIndex = 0;

        UInt16 adcVoltage;
        UInt16 adcCurrent;

        double voltage;
        double current;

        double resistance;

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

        void sendCommand()
        {
            float tempFloat;

            Byte cmdType = desiredType;
            UInt32 cmdAmplitude;
            UInt32 cmdPeriod;
            UInt32 cmdFrequency;
            UInt16 cmdRate;
            Byte cmdConfig;

            unsafe
            {
                cmdType = desiredType;

                tempFloat = (desiredAmplitude / maxAmplitude) * outputVoltageCorrectionFactor;
                if (tempFloat > 1.0f) tempFloat = 1.0f;
                if (tempFloat < 0) tempFloat = 0;
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

            // Send signal properties
            mySerialPort.Write("Q" + cmdType.ToString("X2") + cmdAmplitude.ToString("X8") + cmdPeriod.ToString("X8") + cmdFrequency.ToString("X8") + cmdRate.ToString("X4") + cmdConfig.ToString("X2"));

            // Start singnal genarating
            mySerialPort.Write("G");
        }

        void StartStop()
        {
            if (!buttonStart)
            {
                buttonStart = true;

                btnStart.Text = "Stop";

                if (!Directory.Exists("OUTPUT"))
                {
                    Directory.CreateDirectory("OUTPUT");
                }

                overCurrent = false;

                if (IsHighFreq)
                {
                    testCount++;

                    int name = 0;

                    while (File.Exists("OUTPUT\\PERIODIC_" + testCount.ToString() + "_" + name.ToString() + ".txt"))
                    {
                        name++;
                    }

                    objWriter = new StreamWriter("OUTPUT\\PERIODIC_" + testCount.ToString() + "_" + name.ToString() + ".txt");
                    objWriter.WriteLine("V,I");

                    gfx.Clear(pictureBox1.BackColor);
                    //ScopeInit();
                    pictureBox1.Image = bmpScreen;

                    gfx.DrawString("Test Number: " + testCount.ToString(), new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 10));
                    gfx.DrawString("Signal Type: Positive Pulse", new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 30));
                    gfx.DrawString("Amplitude: " + desiredAmplitude.ToString() + " V", new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 50));
                    gfx.DrawString("Frequency: " + desiredFrequency.ToString() + " Hz", new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 70));

                    thisDate = DateTime.Now;
                    gfx.DrawString(pc.GetYear(thisDate).ToString("D4") + "/" + pc.GetMonth(thisDate).ToString("D2") + "/" + pc.GetDayOfMonth(thisDate).ToString("D2") + " " + pc.GetHour(thisDate).ToString("D2") + ":" + pc.GetMinute(thisDate).ToString("D2") + ":" + pc.GetSecond(thisDate).ToString("D2"),
                                    new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(270, 370));
                }
                else
                {
                    periodCount++;

                    int name = 0;

                    while (File.Exists("OUTPUT\\SINGLE_PERIOD_" + periodCount.ToString() + "_" + name.ToString() + ".txt"))
                    {
                        name++;
                    }

                    objWriter = new StreamWriter("OUTPUT\\SINGLE_PERIOD_" + periodCount.ToString() + "_" + name.ToString() + ".txt");
                    objWriter.WriteLine("V,I");

                    gfx.Clear(pictureBox1.BackColor);
                    ScopeInit();
                    pictureBox1.Image = bmpScreen;

                    string tempStr = "";

                    switch (desiredType)
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
                    gfx.DrawString("Amplitude: " + desiredAmplitude.ToString() + " V", new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 50));
                    gfx.DrawString("Period: " + desiredPeriod.ToString() + " s", new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 70));

                    thisDate = DateTime.Now;
                    gfx.DrawString(pc.GetYear(thisDate).ToString("D4") + "/" + pc.GetMonth(thisDate).ToString("D2") + "/" + pc.GetDayOfMonth(thisDate).ToString("D2") + " " + pc.GetHour(thisDate).ToString("D2") + ":" + pc.GetMinute(thisDate).ToString("D2") + ":" + pc.GetSecond(thisDate).ToString("D2"),
                                    new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(270, 370));
                }

                pointIndex = 0;

                startThread();

                sendCommand(); // send signal properties to the device
            }
            else
            {
                buttonStart = false;

                stopThread();

                operationDone = false;

                if (tempIsHighFreq)
                {

                    if (overCurrent)
                    {
                        overCurrent = false;
                        gfx.DrawString("Over-Current", new Font("Tahoma", 10, FontStyle.Bold), new SolidBrush(Color.Red), new Point(10, 100));
                    }

                    pictureBox1.Image = bmpScreen;

                    int name2 = 0;

                    while (File.Exists("OUTPUT\\PERIODIC_" + testCount.ToString() + "_" + name2.ToString() + ".gif"))
                    {
                        name2++;
                    }

                    bmpScreen.Save("OUTPUT\\PERIODIC_" + testCount.ToString() + "_" + name2.ToString() + ".gif", System.Drawing.Imaging.ImageFormat.Gif);

                    objWriter.Close();

                    pointIndex = 0;
                }

                btnStart.Text = "Start";

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
                                operationDone = true;
                                this.BeginInvoke(new EventHandler(putPoint));
                            }
                            else if (data == Convert.ToByte('W')) // W indicates error condition
                            {
                                // Over-Current or Over-Voltage
                                overCurrent = true;
                                operationDone = true;
                                this.BeginInvoke(new EventHandler(putPoint));
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

                                lock (VI_Buffer)
                                {
                                    VI_Buffer.Enqueue(new Point(adc0, adc1));
                                    this.BeginInvoke(new EventHandler(putPoint));
                                }
                            }
                        }
                    }
                }
            }
        }

        private void putPoint(object sender, EventArgs e)
        {
            if (threadStarted)
            {
                if (tempIsHighFreq)
                {
                    lock (VI_Buffer)
                    {
                        while (VI_Buffer.Count > 0)
                        {
                            VI = VI_Buffer.Dequeue();

                            adcVoltage = (UInt16)(VI.X);
                            adcCurrent = (UInt16)(VI.Y);

                            voltage = ConvertADCtoVoltage(adcVoltage);
                            current = ConvertADCtoCurrent(adcCurrent);

                            if (current != 0)
                            {
                                resistance = voltage / current;
                                objWriter.WriteLine(voltage.ToString() + "," + current.ToString() + "," + resistance.ToString());
                            }
                            else
                            {
                                objWriter.WriteLine(voltage.ToString() + "," + current.ToString() + ", NaN");
                            }

                            gfx.FillRectangle(new SolidBrush(Color.White), 0, 170, 398, 80);

                            string strResistance = "NaN";
                            int intResistance = (int)resistance;


                            strResistance = intResistance.ToString("N0") + " Ω";


                            gfx.DrawString("R = " + strResistance, new Font("Tahoma", 20, FontStyle.Bold), new SolidBrush(Color.Green), new Point(120, 200));
                            //resistance.ToString("G4")
                            pointIndex++;
                        }
                    }

                    pictureBox1.Image = bmpScreen;

                    if (operationDone)
                    {
                        StartStop();
                    }
                }
                else
                {
                    lock (VI_Buffer)
                    {
                        while (VI_Buffer.Count > 0)
                        {
                            VI = VI_Buffer.Dequeue();

                            adcVoltage = (UInt16)(VI.X);
                            adcCurrent = (UInt16)(VI.Y);

                            voltage = ConvertADCtoVoltage(adcVoltage);
                            current = ConvertADCtoCurrent(adcCurrent);

                            objWriter.WriteLine(voltage.ToString() + "," + current.ToString());

                            x = -(int)(((double)(adcVoltage - adcVoltageOffset) / 4096) * 390) + 199;
                            y = -(int)(((double)(adcCurrent - adcCurrentOffset) / 4096) * 390) + 199;

                            if (pointIndex != 0)
                            {
                                gfx.DrawLine(pen, new Point(tempx, tempy), new Point(x, y));
                            }

                            tempx = x;
                            tempy = y;

                            pointIndex++;
                        }
                    }

                    pictureBox1.Image = bmpScreen;

                    if (operationDone)
                    {
                        stopThread();

                        operationDone = false;

                        if (overCurrent)
                        {
                            overCurrent = false;
                            gfx.DrawString("Over-Current", new Font("Tahoma", 10, FontStyle.Bold), new SolidBrush(Color.Red), new Point(10, 100));
                        }

                        pictureBox1.Image = bmpScreen;

                        int name2 = 0;

                        while (File.Exists("OUTPUT\\SINGLE_PERIOD_" + periodCount.ToString() + "_" + name2.ToString() + ".gif"))
                        {
                            name2++;
                        }

                        bmpScreen.Save("OUTPUT\\SINGLE_PERIOD_" + periodCount.ToString() + "_" + name2.ToString() + ".gif", System.Drawing.Imaging.ImageFormat.Gif);

                        objWriter.Close();

                        pointIndex = 0;

                        StartStop();
                    }
                }
            }
        }

        double ConvertADCtoVoltage(UInt16 adcV)
        {
            //return -(((double)(adcV - adcVoltageOffset) / 4096) * 2 * VREF) * voltageCorrectionFactor;
            return -(((double)(adcV - adcVoltageOffset) / 4096) * 2 * VREF) * 4 * voltageCorrectionFactor;
        }
        double ConvertADCtoCurrent(UInt16 adcI)
        {
            //return ((((double)(adcI - adcCurrentOffset) / 4096) * 2 * VREF) * currentCorrectionFactor) / 1500;
            return ((((double)(adcI - adcCurrentOffset) / 4096) * 10 * VREF) * currentCorrectionFactor) / 1500;
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
                    buttonStart = true;
                    StartStop();

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
            qBuffer.Clear();
            VI_Buffer.Clear();
            mySerialPort.DiscardInBuffer();
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

            periodCount = 0;
            testCount = 0;
        }

        void connectedMode()
        {
            btnConnect.Text = "Disconnect";

            grpConfig.Enabled = true;

            cmbPortName.Enabled = false;
            btnScanPorts.Enabled = false;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            numPeriod.Value = (decimal)desiredPeriod;
            lblPeriod.Text = "Period: " + desiredPeriod.ToString() + " s";

            numFreq.Value = desiredFrequency;
            lblFreq.Text = "Frequency: " + desiredFrequency.ToString() + " Hz";

            desiredType = 0;
            rdoSin.Checked = true;

            numAmp.Maximum = (decimal)maxAmplitude;

            numAmp.Value = (decimal)desiredAmplitude;
            lblAmp.Text = "Amplitude: " + desiredAmplitude.ToString() + " V";

            if (desiredSign == -1) chkInvert.Checked = true; else chkInvert.Checked = false;

            desiredRate = 1000;

            if (IsHighFreq)
            {
                rdoSingle.Checked = false;
                rdoPeriodic.Checked = true;

                grpOnePeriod.Enabled = false;
                grpPeriodic.Enabled = true;
            }
            else
            {
                rdoSingle.Checked = true;
                rdoPeriodic.Checked = false;

                grpOnePeriod.Enabled = true;
                grpPeriodic.Enabled = false;
            }

            tempType = desiredType;
            tempAmplitude = desiredAmplitude;
            tempPeriod = desiredPeriod;
            tempFrequency = desiredFrequency;
            tempRate = desiredRate;
            tempSign = desiredSign;
            tempIsHighFreq = IsHighFreq;
        }
    }
}
