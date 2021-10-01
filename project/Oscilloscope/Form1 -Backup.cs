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
        private int x, y, tempx = 255, tempy;
        private SolidBrush sb = new SolidBrush(Color.Red);
        private Pen pen = new Pen(Color.Red);
        private System.IO.StreamWriter objWriter;
        private static int ScreenWidth = 400;
        private static int ScreenHeight = 400;
        private Bitmap bmpScreen = new Bitmap(400, 400);
        private Graphics gfx;
        private static UInt32 sampleCount = 1000;
        private UInt16[] currentArray;
        private UInt16[] voltageArray;
        private bool buttonStart = false;
        private bool operationDone = false;
        private uint count = 0;
        private int periodCount = 0;
        private int offset = 32767;

        private Byte desiredType = 0;
        private float desiredAmplitude = 0.5f;
        private float desiredPeriod = 10.0f;
        private float desiredFrequency = 0.1f;
        private const UInt16 desiredRate = 1000;
        private SByte desiredSign = 1;

        private Byte tempType;
        private float tempAmplitude;
        private float tempPeriod;
        private float tempFrequency;
        private UInt16 tempRate;
        private SByte tempSign = 1;

        private PersianCalendar pc = new PersianCalendar();
        private DateTime thisDate;

        private bool overCurrent = false;

        Queue<byte> qBuffer = new Queue<byte>();
        AutoResetEvent dataAvailable = new AutoResetEvent(false);
        Thread processThread;
        bool threadStarted = false;
        byte data;
        byte[] receivedPacket = new byte[8];
        private bool packetProcessing = false;

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

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            this.BeginInvoke(new EventHandler(getChunk));
        }

        private void getChunk(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1.BytesToRead > 0)
                {
                    byte[] chunk = new byte[serialPort1.BytesToRead];
                    serialPort1.Read(chunk, 0, chunk.Length);

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
                    while (qBuffer.Count() >= 9)
                    {
                        data = qBuffer.Dequeue();

                        if (data == Convert.ToByte('S'))
                        {
                            byte[] receivedPacket = new byte[8];

                            for (int i = 0; i < 8; i++)
                            {
                                receivedPacket[i] = qBuffer.Dequeue();
                            }

                            #region Convert packet to numbers

                            UInt16[] voltageHexDigit = new UInt16[4];
                            UInt16[] currentHexDigit = new UInt16[4];

                            UInt16 adc0 = 17;
                            UInt16 adc1 = 17;

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
                                adc0 = 13;
                                adc1 = 13;
                            }
                            else
                            {
                                adc0 = (UInt16)(voltageHexDigit[3] * 4096 + voltageHexDigit[2] * 256 + voltageHexDigit[1] * 16 + voltageHexDigit[0]);
                                adc1 = (UInt16)(currentHexDigit[3] * 4096 + currentHexDigit[2] * 256 + currentHexDigit[1] * 16 + currentHexDigit[0]);
                            }

                            voltageArray[count] = adc0;
                            currentArray[count] = adc1;

                            count++;

                            //if (count == sampleCount)
                            //{
                            //    count = 0;
                            //    operationDone = true;
                            //    this.BeginInvoke(new EventHandler(test1));
                            //}

                            #endregion

                        }
                        else if (data == Convert.ToByte('P'))
                        {
                            sampleCount = count;
                            operationDone = true;
                            this.BeginInvoke(new EventHandler(test2));
                        }
                        else if (data == Convert.ToByte('W'))
                        {
                            // Over-Current or Over-Voltage
                            overCurrent = true;
                            sampleCount = count;
                            operationDone = true;
                            this.BeginInvoke(new EventHandler(test2));
                        }
                    }
                }
            }
        }
        private void test1(object sender, EventArgs e)
        {
            label8.Text = "Hello";
        }
        private void test2(object sender, EventArgs e)
        {
            label8.Text = "Pello";
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            if (!buttonStart)
            {
                gfx.Clear(pictureBox1.BackColor);
                ScopeInit();
                pictureBox1.Image = bmpScreen;
                buttonStart = true;
                btnStart.Text = "Stop";
                serialPort1.PortName = cmbPortName.Text;
                serialPort1.ReadBufferSize = 1000000;

                cmbPortName.Enabled = false;
                btnScan.Enabled = false;
                if (!Directory.Exists("OUTPUT"))
                {
                    Directory.CreateDirectory("OUTPUT");
                }

                timer1.Enabled = true;
            }
            else
            {
                buttonStart = false;
                if (threadStarted)
                {
                    processThread.Abort();
                    threadStarted = false;
                }
                operationDone = false;
                count = 0;
                periodCount = 0;
                btnStart.Text = "Start";
                cmbPortName.Enabled = true;
                btnScan.Enabled = true;
                timer1.Enabled = false;
                if (serialPort1.IsOpen)
                {
                    // Stop singnal genarating
                    serialPort1.Write("P");
                    serialPort1.Close();
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!operationDone)
            {
                if (!threadStarted)
                {
                    processThread = new Thread(processData);
                    processThread.IsBackground = true;
                    processThread.Start();
                    threadStarted = true;

                    if (!serialPort1.IsOpen)
                        serialPort1.Open();

                    if (chkApply.Checked)
                    {
                        Byte cmdType = desiredType;
                        Byte cmdAmplitude = (Byte)((desiredAmplitude / 8.0f) * 255);
                        //UInt16 cmdRate = desiredRate;
                        UInt16 cmdRate = ((desiredSign == -1) ? (UInt16)0 : (UInt16)1);
                        UInt16 cmdPeriod = (UInt16)((desiredPeriod / 100.0f) * 65535.0f);

                        tempType = desiredType;
                        tempAmplitude = desiredAmplitude;
                        tempPeriod = desiredPeriod;
                        tempFrequency = desiredFrequency;
                        tempRate = desiredRate;
                        tempSign = desiredSign;


                        //UInt16 golbalRate = cmdRate;
                        UInt16 golbalRate = desiredRate;
                        float globalPeriod = (float)(cmdPeriod) / 655.0f;

                        sampleCount = (UInt32)(golbalRate * globalPeriod);
                        //label4.Text = sampleCount.ToString();
                        //label8.Text = globalPeriod.ToString();
                        currentArray = new UInt16[sampleCount];
                        voltageArray = new UInt16[sampleCount];

                        // Send signal properties
                        serialPort1.Write("Q" + cmdType.ToString("X1") + cmdAmplitude.ToString("X2") + cmdRate.ToString("X4") + cmdPeriod.ToString("X4"));
                    }

                    // Start singnal genarating
                    serialPort1.Write("G");

                }
            }
            else
            {
                processThread.Abort();
                threadStarted = false;
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

                if (overCurrent)
                {
                    overCurrent = false;
                    gfx.DrawString("Over-Current", new Font("Tahoma", 10, FontStyle.Bold), new SolidBrush(Color.Red), new Point(10, 100));
                }

                gfx.DrawString("Period Number: " + periodCount.ToString(), new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 10));
                gfx.DrawString("Signal Type: " + tempStr, new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 30));
                gfx.DrawString("Amplitude: " + tempAmplitude.ToString() + " V", new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 50));
                gfx.DrawString("Frequency: " + tempFrequency.ToString() + " Hz", new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 70));

                thisDate = DateTime.Now;
                gfx.DrawString(pc.GetYear(thisDate).ToString("D4") + "/" + pc.GetMonth(thisDate).ToString("D2") + "/" + pc.GetDayOfMonth(thisDate).ToString("D2") + " " + pc.GetHour(thisDate).ToString("D2") + ":" + pc.GetMinute(thisDate).ToString("D2") + ":" + pc.GetSecond(thisDate).ToString("D2"),
                                new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(270, 370));

                for (int i = 0; i < sampleCount; i++)
                {
                    UInt16 adcVoltage = voltageArray[i];
                    UInt16 adcCurrent = currentArray[i];

                    double voltage = -(((double)(adcVoltage - offset) / 65535) * 3.3) * 4 * 1.325421391778252936 - 0.003021378303794;
                    double current = -(((double)(adcCurrent - offset) / 65535) * 3.3) * 4 * 0.001338338929378022 + 6.051442370329619e-07;

                    //objWriter.WriteLine(voltage.ToString() + "," + current.ToString());
                    objWriter.WriteLine(adcVoltage.ToString() + "," + adcCurrent.ToString());

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

                if (serialPort1.IsOpen)
                    serialPort1.Close();
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
               //gfx.DrawString((-i + 5).ToString(), tfont, new SolidBrush(Color.Yellow), new Point(200 ,i * ky), drawFormat);
            }

            int kx = ScreenWidth / 20;
            for (int i = 0; i * kx <= ScreenWidth; i++)
            {
                gfx.DrawLine(tpen, new Point(i * kx - 1, 0), new Point(i * kx - 1, ScreenHeight));
                //gfx.DrawString((i - 5).ToString(), tfont, new SolidBrush(Color.Yellow), new Point(i * kx, 200));
            }
            //gfx.DrawString("Current (A)", tfont, new SolidBrush(Color.Black), new Point(270, 10), drawFormat);
            //gfx.DrawString("Voltage (V)", tfont, new SolidBrush(Color.Black), new Point(325, 210));
            gfx.DrawLine(new Pen(Color.FromArgb(100, 100, 100)), new Point(0, 199), new Point(400, 199));
            gfx.DrawLine(new Pen(Color.FromArgb(100, 100, 100)), new Point(199, 0), new Point(199, 400));
        }

        private void btnScan_Click(object sender, EventArgs e)
        {
            if (!scanSerialPorts())
                MessageBox.Show("No serial port found!");
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                try 
                {
                    serialPort1.Close(); 
                }
                catch 
                { 
                    MessageBox.Show("Port can't be closed."); 
                }
            }
        }

        private void numFreq_ValueChanged(object sender, EventArgs e)
        {
            desiredFrequency = (float)numFreq.Value;
            desiredPeriod = (1.0f / desiredFrequency);
            lblFreq.Text = "Frequncey: " + desiredFrequency.ToString() + " Hz";
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
    }
}
