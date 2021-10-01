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
        float t = 0;
        SolidBrush sb = new SolidBrush(Color.Red);
        Pen pen = new Pen(Color.Red);
        System.IO.StreamWriter objWriter;
        int ScreenWidth = 900;
        int ScreenHeight = 300;
        Bitmap bmpScreen = new Bitmap(900, 300);
        Graphics gfx;
        UInt32 sampleCount = 100;
        int[] currentArray;
        int[] voltageArray;
        Queue<int> adc0Q = new Queue<int>();
        Queue<int> adc1Q = new Queue<int>();
        bool buttonStart = false;
        bool threadStarted = false;
        bool overCurrent = false;
        bool resistance = false;
        int count = 0;
        float time = 0;
        float dcEnd = 0;
        int offset = 32767;

        UInt16 golbalRate = 100;

        Byte desiredType = 4;
        float desiredAmplitude = 1.5f;
        double desiredPeriod = 45.0f;
        float desiredFrequency = 0.1f;
        SByte desiredSign = 1;

        Byte tempType;
        float tempAmplitude;
        double tempPeriod;
        float tempFrequency;
        SByte tempSign = 1;

        PersianCalendar pc = new PersianCalendar();
        DateTime thisDate;

        Queue<byte> qBuffer = new Queue<byte>();
        List<byte[]> buffer = new List<byte[]>();
        AutoResetEvent dataAvailable = new AutoResetEvent(false);
        Thread processThread;

        byte data;

        byte[] packet = new byte[8];

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

        // This thread processes the stored chunks doing the less locking possible
        void processSerialData(object state)
        {
            while (true)
            {
                dataAvailable.WaitOne();

                lock (qBuffer)
                {
                    while (qBuffer.Count() > 8)
                    {
                        data = qBuffer.Dequeue();

                        if (data == Convert.ToByte('S'))
                        {
                            packet[7] = getHexValue(qBuffer.Dequeue());
                            packet[6] = getHexValue(qBuffer.Dequeue());
                            packet[5] = getHexValue(qBuffer.Dequeue());
                            packet[4] = getHexValue(qBuffer.Dequeue());

                            packet[3] = getHexValue(qBuffer.Dequeue());
                            packet[2] = getHexValue(qBuffer.Dequeue());
                            packet[1] = getHexValue(qBuffer.Dequeue());
                            packet[0] = getHexValue(qBuffer.Dequeue());

                            this.Invoke(new EventHandler(processData));
                        }
                        else
                        {
                            if (data == Convert.ToByte('W'))
                                overCurrent = true;
                            else
                            {
                                qBuffer.Dequeue();
                            }
                        }
                    }
                 }
            }
        }

        //private void timer1_Tick(object sender, EventArgs e)
        //{
        //    if (!operationDone)
        //    {
        //        if (!threadStarted)
        //        {
        //            //processThread = new Thread(processData);
        //            //processThread.Start();
        //            threadStarted = true;

        //            if (!serialPort1.IsOpen)
        //                serialPort1.Open();

        //            if (chkApply.Checked)
        //            {
        //                Byte cmdType = desiredType;
        //                Byte cmdSign = ((desiredSign == -1) ? (Byte)0 : (Byte)15);
        //                Byte cmdAmplitude = (Byte)((desiredAmplitude / 1.5f) * 255);
        //                UInt32 cmdPeriod = (UInt32)((desiredPeriod / 100.0) * 4294967295.0);

        //                tempType = desiredType;
        //                tempSign = desiredSign;
        //                tempAmplitude = desiredAmplitude;
        //                tempPeriod = desiredPeriod;
        //                tempFrequency = desiredFrequency;                     

                        
        //                double globalPeriod = (double)(cmdPeriod) / 42949672.0;

        //                sampleCount = (UInt32)(golbalRate * globalPeriod);

        //                //label4.Text = sampleCount.ToString();
        //                //label8.Text = globalPeriod.ToString();
        //                label8.Text = overCurrent.ToString();

        //                currentArray = new int[sampleCount];
        //                voltageArray = new int[sampleCount];

        //                serialPort1.ReceivedBytesThreshold = (int)(sampleCount * 9);
        //                label4.Text = serialPort1.ReceivedBytesThreshold.ToString();

        //                // Send signal properties
        //                serialPort1.Write("Q" + cmdType.ToString("X1") + cmdSign.ToString("X1") + cmdAmplitude.ToString("X2") + cmdPeriod.ToString("X8"));
        //            }

        //            // Start singnal genarating
        //            serialPort1.Write("G");

        //            overCurrent = false;
        //        }  
        //    }
        //    else
        //    {
        //        //processThread.Abort();
        //        threadStarted = false;
        //        operationDone = false;

        //        periodCount++;

        //        int name = 0;

        //        while (File.Exists("OUTPUT\\PERIOD_" + periodCount.ToString() + "_" + name.ToString() + ".txt"))
        //        {
        //            name++;
        //        }

        //        objWriter = new StreamWriter("OUTPUT\\PERIOD_" + periodCount.ToString() + "_" + name.ToString() + ".txt");
        //        objWriter.WriteLine("V,I");

        //        gfx.Clear(pictureBox1.BackColor);
        //        ScopeInit();

        //        string tempStr = "";

        //        switch (tempType)
        //        {
        //            case 0:
        //                tempStr = "Sine";
        //                break;
        //            case 1:
        //                tempStr = "Triangle";
        //                break;
        //            case 2:
        //                tempStr = "| Sine |";
        //                break;
        //            case 3:
        //                tempStr = "| Triangle |";
        //                break;
        //            case 4:
        //                tempStr = "DC";
        //                break;
        //        }

        //        gfx.DrawString("Period Number: " + periodCount.ToString(), new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 10));
        //        gfx.DrawString("Signal Type: " + tempStr, new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 30));
        //        gfx.DrawString("Amplitude: " + tempAmplitude.ToString() + " V", new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 50));
        //        gfx.DrawString("Frequency: " + tempFrequency.ToString() + " Hz", new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(10, 70));

        //        thisDate = DateTime.Now;
        //        gfx.DrawString(pc.GetYear(thisDate).ToString("D4") + "/" + pc.GetMonth(thisDate).ToString("D2") + "/" + pc.GetDayOfMonth(thisDate).ToString("D2") + " " + pc.GetHour(thisDate).ToString("D2") + ":" + pc.GetMinute(thisDate).ToString("D2") + ":" + pc.GetSecond(thisDate).ToString("D2"),
        //                        new Font("Tahoma", 10, FontStyle.Regular), new SolidBrush(Color.Black), new Point(270, 370));

        //        //if (periodCount == 1)
        //        //    offset = (int)currentArray.Average();
        //        //label8.Text = offset.ToString();

        //        for (int i = 0; i < sampleCount; i++)
        //        {
        //            double voltage = -((double)(voltageArray[i] - offset) / 65535) * 3.3;
        //            double current = -(((double)(currentArray[i] - offset) / 65535) * 3.3 / 1007);

        //            objWriter.WriteLine(voltage.ToString() + "," + current.ToString());

        //            x = -(int)(((double)(voltageArray[i] - offset) / 65535) * 441) + 199;
        //            y = (int)(((double)(currentArray[i] - offset) / 65535) * 441) + 199;

        //            if (i != 0)
        //            {
        //                gfx.DrawLine(pen, new Point(tempx, tempy), new Point(x, y));
        //            }

        //            tempx = x;
        //            tempy = y;
        //        }

        //        pictureBox1.Image = bmpScreen;
                
        //        int name2 = 0;

        //        while (File.Exists("OUTPUT\\PERIOD_" + periodCount.ToString() + "_" + name2.ToString() + ".gif"))
        //        {
        //            name2++;
        //        }

        //        bmpScreen.Save("OUTPUT\\PERIOD_" + periodCount.ToString() + "_" + name2.ToString() + ".gif",System.Drawing.Imaging.ImageFormat.Gif);

        //        objWriter.Close();

        //        if (serialPort1.IsOpen)
        //            serialPort1.Close();
        //    }        
        //}

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
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

        private void processData(object sender, EventArgs e)
        {
            if (buttonStart)
            {
                UInt16 adc0 = (UInt16)(packet[7] * 4096 + packet[6] * 256 + packet[5] * 16 + packet[4]);
                UInt16 adc1 = (UInt16)(packet[3] * 4096 + packet[2] * 256 + packet[1] * 16 + packet[0]);


                double voltage = -((double)(adc0 - offset) / 65535) * 3.3 * 1.669632881712438;
                double current = -(((double)(adc1 - offset) / 65535) * 3.3 * 1.36818925714168 / 1006);

                double resVoltage = voltage - 1494 * current;
                double res = resVoltage / current;

                objWriter.Write(time.ToString());


                    time = (float)Math.Round(time + 0.01, 2);


                objWriter.WriteLine("," + voltage.ToString() + "," + current.ToString());

                t += 0.08f;
                x = (int)Math.Floor(t);

                if (x == 899)
                {
                    x = 0; 
                    t = 0; 
                    gfx.Clear(pictureBox1.BackColor);
                    ScopeInit();
                }

                y = -(int)((resVoltage / 5.0) * 300) + 150;

                if (x != 0)
                {
                    gfx.DrawLine(pen, new Point(tempx, tempy), new Point(x, y));
                }

                tempx = x;
                tempy = y;
            }
        }
        

        private void button1_Click(object sender, EventArgs e)
        {
            if (!buttonStart)
            {
                desiredAmplitude = (float)numericUpDown1.Value;

                gfx.Clear(pictureBox1.BackColor);
                ScopeInit();
                pictureBox1.Image = bmpScreen;

                qBuffer.Clear();

                buttonStart = true;
                button1.Text = "Stop";

                button3.Enabled = true;

                x = 0;
                t = 0;
                if (serialPort1.IsOpen)
                    serialPort1.Close();

                serialPort1.PortName = cmbPortName.Text;
                serialPort1.ReadBufferSize = 1000000;
                cmbPortName.Enabled = false;
                
                Byte cmdType = desiredType;
                Byte cmdSign = ((desiredSign == -1) ? (Byte)0 : (Byte)15);
                Byte cmdAmplitude = (Byte)((desiredAmplitude / 2.5f) * 255);
                UInt32 cmdPeriod = (UInt32)((desiredPeriod / 100.0) * 4294967295.0);

                button2.Enabled = false;
                if (!Directory.Exists("OUTPUT"))
                {
                    Directory.CreateDirectory("OUTPUT");
                }

                processThread = new Thread(processSerialData);
                processThread.Start();
                threadStarted = true;

                if (!serialPort1.IsOpen)
                   serialPort1.Open();

                // Send signal properties
                serialPort1.Write("Q" + cmdType.ToString("X1") + cmdSign.ToString("X1") + cmdAmplitude.ToString("X2") + cmdPeriod.ToString("X8"));

                // Start singnal genarating
                serialPort1.Write("G");

                int name = 0;

                while (File.Exists("OUTPUT\\PERIOD_" + name.ToString() + ".dat"))
                {
                    name++;
                }

                objWriter = new StreamWriter("OUTPUT\\PERIOD_" + name.ToString() + ".dat", false, Encoding.ASCII);
                //objWriter.WriteLine("t,v,i");

                overCurrent = false;
            }
            else
            {
                buttonStart = false;
                button1.Text = "Start";

                resistance = false;
                button3.Enabled = false;

                time = 0;
                dcEnd = 0;

                cmbPortName.Enabled = true;
                button2.Enabled = true;

                serialPort1.Write("P");
                objWriter.Close();

                if (threadStarted)
                {
                    processThread.Abort();
                    threadStarted = false;
                }
            }
        }

        private void ScopeInit()
        {
            Pen tpen = new Pen(Color.FromArgb(230, 230, 230));
            Font tfont = new Font("Segoe UI", 8);

            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;

        //    int ky = ScreenHeight / 20;
        //    for (int i = 0; i * ky <= ScreenHeight; i++)
        //    {
        //        drawFormat.Alignment = StringAlignment.Far;
        //        drawFormat.FormatFlags = StringFormatFlags.NoWrap;
        //        gfx.DrawLine(tpen, new Point(0, i * ky - 1), new Point(ScreenWidth, i * ky - 1));
        //       //gfx.DrawString((-i + 5).ToString(), tfont, new SolidBrush(Color.Yellow), new Point(200 ,i * ky), drawFormat);
        //    }

        //    int kx = ScreenWidth / 20;
        //    for (int i = 0; i * kx <= ScreenWidth; i++)
        //    {
        //        gfx.DrawLine(tpen, new Point(i * kx - 1, 0), new Point(i * kx - 1, ScreenHeight));
        //        //gfx.DrawString((i - 5).ToString(), tfont, new SolidBrush(Color.Yellow), new Point(i * kx, 200));
        //    }
        //    //gfx.DrawString("Current (A)", tfont, new SolidBrush(Color.Black), new Point(270, 10), drawFormat);
        //    //gfx.DrawString("Voltage (V)", tfont, new SolidBrush(Color.Black), new Point(325, 210));
            gfx.DrawLine(new Pen(Color.FromArgb(100, 100, 100)), new Point(0, 149), new Point(900, 149));
        //    gfx.DrawLine(new Pen(Color.FromArgb(100, 100, 100)), new Point(199, 0), new Point(199, 400));

            int ty = -(int)((desiredAmplitude / 5.0) * 300) + 150;
            gfx.DrawLine(new Pen(Color.FromArgb(200, 200, 200)), new Point(0, ty), new Point(900, ty));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            scanSerialPorts();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            pictureBox1.Image = bmpScreen;
            if (resistance)
                label1.Text = (time).ToString();
            else
                label1.Text = (time).ToString();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen)
                serialPort1.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (buttonStart)
            {
                if (!resistance)
                {
                    resistance = true;
                    serialPort1.Write("R");
                }
                else
                {
                    //serialPort1.Write("P");
                }

                button3.Enabled = false;
            }
        }
    }
}
