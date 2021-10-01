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
        private static int sampleCount = 10000;
        private int[] currentArray = new int[sampleCount];
        private int[] voltageArray = new int[sampleCount];
        private Queue<int> adc0Q = new Queue<int>();
        private Queue<int> adc1Q = new Queue<int>();
        private bool buttonStart = false;
        private bool threadStarted = false;
        private bool operationDone = false;
        private int count = 0;
        private int periodCount = 0;
        private int offset = 43769;

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

                gfx.DrawString("Period: " + periodCount.ToString(), new Font("Tahoma", 10, FontStyle.Bold), new SolidBrush(Color.White), new Point(10, 10));

                if (periodCount == 1)
                    offset = (int)voltageArray.Average();

                for (int i = 0; i < sampleCount; i++)
                {
                    double voltage = -((double)(voltageArray[i] - offset) / 65535) * 3.2 * 1.53333;
                    double current = -(((double)(currentArray[i] - offset) / 65535) * 3.2 * 1.53333 / 1003);

                    objWriter.WriteLine(voltage.ToString() + "," + current.ToString());

                    x = -(int)(((double)(voltageArray[i] - offset) / 65535) * 400) + 200;
                    y = (int)(((double)(currentArray[i] - offset) / 65535) * 400) + 200;

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
                serialPort1.ReceivedBytesThreshold = sampleCount * 9 + 1;
                serialPort1.ReadBufferSize = sampleCount * 9 + 10;
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
                    serialPort1.Close();
                buffer.Clear();

            }
        }

        private void ScopeInit()
        {
            Pen tpen = new Pen(Color.FromArgb(60, 60, 60));
            Font tfont = new Font("Segoe UI", 8);

            StringFormat drawFormat = new StringFormat();
            drawFormat.FormatFlags = StringFormatFlags.DirectionVertical;

            int ky = ScreenHeight / 20;
            for (int i = 0; i * ky <= ScreenHeight; i++)
            {
                drawFormat.Alignment = StringAlignment.Far;
                drawFormat.FormatFlags = StringFormatFlags.NoWrap;
                gfx.DrawLine(tpen, new Point(0, i * ky), new Point(ScreenWidth,i * ky));
               //gfx.DrawString((-i + 5).ToString(), tfont, new SolidBrush(Color.Yellow), new Point(200 ,i * ky), drawFormat);
            }

            int kx = ScreenWidth / 20;
            for (int i = 0; i * kx <= ScreenWidth; i++)
            {
                gfx.DrawLine(tpen, new Point(i * kx, 0), new Point(i * kx, ScreenHeight));
                //gfx.DrawString((i - 5).ToString(), tfont, new SolidBrush(Color.Yellow), new Point(i * kx, 200));
            }
            gfx.DrawString("Current (A)", tfont, new SolidBrush(Color.White), new Point(190, 10), drawFormat);
            gfx.DrawString("Voltage (V)", tfont, new SolidBrush(Color.White), new Point(325, 210));
            gfx.DrawLine(new Pen(Color.FromArgb(255, 255, 255)), new Point(0, 200), new Point(400, 200));
            gfx.DrawLine(new Pen(Color.FromArgb(255, 255, 255)), new Point(200, 0), new Point(200, 400));
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
    }
}
