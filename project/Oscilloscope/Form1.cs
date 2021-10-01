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
        private int[] currentArray;
        private int[] voltageArray;
        private Queue<int> adc0Q = new Queue<int>();
        private Queue<int> adc1Q = new Queue<int>();
        private bool buttonStart = false;
        private bool threadStarted = false;
        private bool operationDone = false;
        private int count = 0;
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

        private int stateCounter = 0;
        private int stateCounter2 = 0;

        private PersianCalendar pc = new PersianCalendar();
        private DateTime thisDate;

        private bool overCurrent = false;

        List<byte[]> buffer = new List<byte[]>();
        AutoResetEvent dataAvailable = new AutoResetEvent(false);
        Thread processThread;

        void scanSerialPorts()
        {
            string[] ports = SerialPort.GetPortNames();

            cmbPortName.Items.Clear();
            cmbPortName.Items.AddRange(ports);

            if (ports.GetLength(0) == 0)
                MessageBox.Show("No Serial Port Found!");
            else
                cmbPortName.Text = ports[ports.GetLength(0) - 1];
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

        //This thread processes the stored chunks doing the less locking possible
        void processData(object state)
        {

            while (true)
            {
                dataAvailable.WaitOne();

                while (buffer.Count > 0)
                {

                    byte[] chunk;

                    lock (buffer)
                    {
                        chunk = buffer[0];
                        buffer.RemoveAt(0);
                    }

                    int tempchunkLength = chunk.Length;

                    for (int i = 0; i < tempchunkLength; i++)
                    {                        
                        // read packet "S%04X%04X"
                        if ((chunk[i] == Convert.ToByte('S')) && (tempchunkLength > i + 8))
                        {
                            byte[] ad = new byte[4];
                            byte[] bd = new byte[4];

                            ad[3] = getHexValue(chunk[i + 1]);
                            ad[2] = getHexValue(chunk[i + 2]);
                            ad[1] = getHexValue(chunk[i + 3]);
                            ad[0] = getHexValue(chunk[i + 4]);

                            bd[3] = getHexValue(chunk[i + 5]);
                            bd[2] = getHexValue(chunk[i + 6]);
                            bd[1] = getHexValue(chunk[i + 7]);
                            bd[0] = getHexValue(chunk[i + 8]);

                            if (ad[3] == 16 || ad[2] == 16 || ad[1] == 16 || ad[0] == 16 || bd[3] == 16 || bd[2] == 16 || bd[1] == 16 || bd[0] == 16)
                            {
                                i = i + 8;
                                continue;
                            }

                            int adc0 = ad[3] * 4096 + ad[2] * 256 + ad[1] * 16 + ad[0];
                            int adc1 = bd[3] * 4096 + bd[2] * 256 + bd[1] * 16 + bd[0];

                            voltageArray[count] = adc0;
                            currentArray[count] = adc1;

                            count++;

                            if (count == sampleCount)
                            {
                                count = 0;
                                operationDone = true;
                            }

                            i = i + 8;
                        }
                        else
                        {
                            if ((chunk[i] == Convert.ToByte('W')))
                            {
                                overCurrent = true;
                                operationDone = true;
                            }
                            continue;
                        }
                    }
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
                    processThread.Start();
                    threadStarted = true;

                    if (!serialPort1.IsOpen)
                        serialPort1.Open();

                    if (chkApply.Checked)
                    {
                        
                        //*****************************************
                        //stateCounter++;
                        //if (stateCounter <= 5)
                        //{
                        //    desiredType = 3;
                        //    desiredAmplitude = 2.0f;
                        //    desiredSign = 1;
                        //    desiredPeriod = 5.0f;
                        //    desiredFrequency = 0.2f;
                        //}
                        //else if (stateCounter <= 10)
                        //{
                        //    //desiredType = 3;
                        //    //desiredAmplitude = 0.5f;
                        //    desiredSign = -1;
                        //    //desiredPeriod = 2.0f;
                        //    if (stateCounter == 10) stateCounter = 0;
                        //}
                        //*****************************************
                        Byte enableMix = 1;

                        Byte cmdType = desiredType;
                        Byte cmdAmplitude = (Byte)((desiredAmplitude / 8.0f) * 255);
                        //UInt16 cmdRate = desiredRate;
                        UInt16 cmdRate = ((desiredSign == -1) ? (UInt16)0 : (UInt16)1);
                        UInt16 cmdTemp = ((enableMix == 0) ? (UInt16)0 : (UInt16)4);
                        cmdRate = (UInt16)(cmdTemp + cmdRate);
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
                        currentArray = new int[sampleCount];
                        voltageArray = new int[sampleCount];

                        serialPort1.ReceivedBytesThreshold = (int)(sampleCount * 9);

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
                    double voltage = -(((double)(voltageArray[i] - offset) / 65535) * 3.3) * 4 * 1.325421391778252936 - 0.003021378303794;
                    double current = -(((double)(currentArray[i] - offset) / 65535) * 3.3) * 4 * 0.001338338929378022 + 6.051442370329619e-07;

                    objWriter.WriteLine(voltage.ToString() + "," + current.ToString());

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

                bmpScreen.Save("OUTPUT\\PERIOD_" + periodCount.ToString() + "_" + name2.ToString() + ".gif",System.Drawing.Imaging.ImageFormat.Gif);

                objWriter.Close();

                if (serialPort1.IsOpen)
                    serialPort1.Close();
            }        
        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            while (serialPort1.BytesToRead > 0)
            {
                byte[] chunk = new byte[serialPort1.BytesToRead];
                serialPort1.Read(chunk, 0, chunk.Length);

                lock (buffer)
                    buffer.Add(chunk);

                dataAvailable.Set();
            }
        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            if (!buttonStart)
            {
                gfx.Clear(pictureBox1.BackColor);
                ScopeInit();
                pictureBox1.Image = bmpScreen;
                buttonStart = true;
                button1.Text = "Stop";
                serialPort1.PortName = cmbPortName.Text;
                serialPort1.ReadBufferSize = 100000;

                cmbPortName.Enabled = false;
                button2.Enabled = false;
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
                button1.Text = "Start";
                cmbPortName.Enabled = true;
                button2.Enabled = true;
                timer1.Enabled = false;
                if (serialPort1.IsOpen)
                { 
                    // Stop singnal genarating
                    serialPort1.Write("P");
                    serialPort1.Close();
                }
                buffer.Clear();
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

        private void button2_Click(object sender, EventArgs e)
        {
            scanSerialPorts();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (threadStarted)
                processThread.Abort();
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
