using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Serial_ESCPOS
{
    public partial class Form1 : Form
    {
        Socket c = null;
        String str_ip = null;
        int port = 9100;

        Thread readThread;
        static bool _continue;

        delegate void SetTextCallback(string text);

        public Form1()
        {
            InitializeComponent();
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void buttonOpenPort_Click(object sender, EventArgs e)
        {
            // IP地址检查
            if ((numericUpDown1.Value != 10)
                && (numericUpDown1.Value != 172) && (numericUpDown1.Value != 192))
            {
                MessageBox.Show("私有IP地址允许的网段范围是:\n"
                                + "10.0.0.0--10.255.255.255\n"
                                + "172.16.0.0--172.31.255.255\n"
                                + "192.168.0.0--192.168.255.255\n");
            }
            else if ((numericUpDown1.Value == 172)
                 && ((numericUpDown2.Value < 16) || (numericUpDown2.Value > 31)))
            {
                MessageBox.Show("私有IP地址允许的网段范围是:\n"
                                + "172.16.0.0--172.31.255.255\n");
            }
            else if ((numericUpDown1.Value == 192) && (numericUpDown2.Value != 168))
            {
                MessageBox.Show("私有IP地址允许的网段范围是:\n"
                                + "192.168.0.0--192.168.255.255\n");
            }
            else
            {
                // Connect this IP address on TCP Port9100
                str_ip = numericUpDown1.Value + "." + numericUpDown2.Value + "."
                       + numericUpDown3.Value + "." + numericUpDown4.Value;
                IPAddress ip = IPAddress.Parse(str_ip);

                try
                {
                    //把ip和端口转化为IPEndPoint实例
                    IPEndPoint ip_endpoint = new IPEndPoint(ip, port);

                    //创建一个Socket
                    c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


                    //连接到服务器
                    c.Connect(ip_endpoint);
                    //应对同步Connect超时过长的办法，猜测应该是先用异步方式建立以个连接然后，
                    //确认连接是否可用，然后报错或者关闭后，重新建立一个同步连接                    

                    c.SendTimeout = 1000;

                    //初始化打印机，并打印
                    ipWrite("\x1b\x40打开TCP/IP连接!\n-------------------------------\n\n");

                    //操作按钮启用
                    buttonClosePort.Enabled = true;
                    buttonOpenPort.Enabled = false;
                    button1.Enabled = true;
                    button2.Enabled = true;
                    button3.Enabled = true;
                    button4.Enabled = true;
                    button5.Enabled = true;
                    //button6.Enabled = true;
                    //button7.Enabled = true;
                    button8.Enabled = true;
                    button9.Enabled = true;

                    //读线程
                    _continue = true;
                    readThread = new Thread(Read);

                    //读线程启动
                    readThread.Start();
                }
                catch (ArgumentNullException e1)
                {
                    //MessageBox.Show(String.Format("参数意外:{0}", e1));
                    MessageBox.Show("Socket参数设置错误!");
                }
                catch (SocketException e2)
                {
                    //MessageBox.Show(String.Format("Socket连接意外:{0}", e2));
                    MessageBox.Show("连接不到指定IP的打印机!");
                }
            }
        }

        private void ipWrite(String str_send)
        {
            //String str_send = "爱普生（中国）有限公司!\n";
            Byte[] byte_send = Encoding.GetEncoding("gb18030").GetBytes(str_send);
            ipWrite(byte_send, 0, byte_send.Length);
        }

        private void ipWrite(Byte[] byte_send, int start, int length)
        {
            try
            {
                //发送测试信息
                c.Send(byte_send, length, 0);
            }
            catch (SocketException e2)
            {
                MessageBox.Show(String.Format("Socket连接意外:{0}", e2));
            }
        }

        private void buttonClosePort_Click(object sender, EventArgs e)
        {
            ipClose();
        }

        private void ipClose()
        {
            // 结束演示，关闭IP连接
            ipWrite("\n-------------------------------\n关闭TCP/IP连接!\n");

            _continue = false;
            readThread.Abort();
            c.Close();

            buttonClosePort.Enabled = false;
            buttonOpenPort.Enabled = true;
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
            //button6.Enabled = false;
            //button7.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "读取(可复位)计数器: " + comboBox1.Text + "!\r\n";
            
            // GS g 0
            byte[] readCounter = new byte[] { 0x1D, 0x67, 0x32, 0x00, 0x14, 0x00 };

            if (comboBox1.Text == "进纸的行数")
            {
                readCounter[4] = (byte)'\x14';
            }
            else if (comboBox1.Text == "热敏头加热次数")
            {
                readCounter[4] = (byte)'\x15';
            }
            else if (comboBox1.Text == "切纸次数")
            {
                readCounter[4] = (byte)'\x32';
            }
            else if (comboBox1.Text == "上电时间")
            {
                readCounter[4] = (byte)'\x46';
            }

            ipWrite(readCounter, 0, readCounter.Length);
            ipWrite("3.2 读取(可复位)计数器: " + comboBox1.Text + "!\n\n");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "重置计数器: " + comboBox1.Text + "!\r\n";

            // GS g 0
            byte[] readCounter = new byte[] { 0x1D, 0x67, 0x30, 0x00, 0x14, 0x00 };

            if (comboBox1.Text == "进纸的行数")
            {
                readCounter[4] = (byte)'\x14';
            }
            else if (comboBox1.Text == "热敏头加热次数")
            {
                readCounter[4] = (byte)'\x15';
            }
            else if (comboBox1.Text == "切纸次数")
            {
                readCounter[4] = (byte)'\x32';
            }
            else if (comboBox1.Text == "上电时间")
            {
                readCounter[4] = (byte)'\x46';
            }

            ipWrite(readCounter, 0, readCounter.Length);
            ipWrite("3.3 重置计数器: " + comboBox1.Text + "!\n\n");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "读取(累计)计数器: " + comboBox1.Text + "!\r\n";

            // GS g 0
            byte[] readCounter = new byte[] { 0x1D, 0x67, 0x32, 0x00, 0x14, 0x00 };

            if (comboBox1.Text == "进纸的行数")
            {
                readCounter[4] = (byte)'\x94';
            }
            else if (comboBox1.Text == "热敏头加热次数")
            {
                readCounter[4] = (byte)'\x95';
            }
            else if (comboBox1.Text == "切纸次数")
            {
                readCounter[4] = (byte)'\xB2';
            }
            else if (comboBox1.Text == "上电时间")
            {
                readCounter[4] = (byte)'\xC6';
            }

            ipWrite(readCounter, 0, readCounter.Length);
            ipWrite("3.1 读取(累计)计数器: " + comboBox1.Text + "!\n\n");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "读取当前的打印机速度设定:\r\n";

            // Enter user setting mode
            byte[] setBegin = new byte[] { 0x1D, 0x28, 0x45, 0x03, 0x00, 0x01, 0x49, 0x4E };
            ipWrite(setBegin, 0, setBegin.Length);

            // Read printer speed
            byte[] setReadSpeed = new byte[] { 0x1D, 0x28, 0x45, 0x02, 0x00, 0x06, 0x06 };
            ipWrite(setReadSpeed, 0, setReadSpeed.Length);

            // Leave user setting mode
            byte[] setEnd = new byte[] { 0x1D, 0x28, 0x45, 0x04, 0x00, 0x02, 0x4F, 0x55, 0x54 };
            ipWrite(setEnd, 0, setEnd.Length);

            ipWrite("1.2 读取当前的打印机速度设定!\n\n");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "设置打印速度为：" + comboBox2.Text + "\r\n";

            // Enter user setting mode
            byte[] setBegin = new byte[] { 0x1D, 0x28, 0x45, 0x03, 0x00, 0x01, 0x49, 0x4E };
            ipWrite(setBegin, 0, setBegin.Length);

            // Set printer speed
            byte[] setSpeed = new byte[] { 0x1D, 0x28, 0x45, 0x04, 0x00, 0x05, 0x06, 0x09, 0x00};
            setSpeed[7] = (byte)(comboBox2.Text[6] - '0');          
            ipWrite(setSpeed, 0, setSpeed.Length);

            // Leave user setting mode
            byte[] setEnd = new byte[] { 0x1D, 0x28, 0x45, 0x04, 0x00, 0x02, 0x4F, 0x55, 0x54 };
            ipWrite(setEnd, 0, setEnd.Length);

            ipWrite("1.1 设置打印速度为：" + comboBox2.Text + "\n\n");

            //打印机复位会关闭连接
            MessageBox.Show("打印机复位会关闭连接");

            //readThread.Abort();
            //ipClose();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "仅当跳线1-7和1-8为ON时\r\n"
                            + "GS ( E 的波特率设定才起作用!!\r\n\r\n"
                            + "读取当前串口波特率（BPS）:\r\n";

            // Enter user setting mode
            byte[] setBegin = new byte[] { 0x1D, 0x28, 0x45, 0x03, 0x00, 0x01, 0x49, 0x4E };
            ipWrite(setBegin, 0, setBegin.Length);

            // Read printer baud
            byte[] setReadBaud = new byte[] { 0x1D, 0x28, 0x45, 0x02, 0x00, 0x0C, 0x01 };
            ipWrite(setReadBaud, 0, setReadBaud.Length);

            // Leave user setting mode
            byte[] setEnd = new byte[] { 0x1D, 0x28, 0x45, 0x04, 0x00, 0x02, 0x4F, 0x55, 0x54 };
            ipWrite(setEnd, 0, setEnd.Length);

            ipWrite("4.1 读取当前打印机串口波特率（BPS）!\n\n");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "仅当跳线1-7和1-8为ON时\r\n"
                            + "GS ( E 的波特率设定才起作用!!\r\n\r\n"
                            + "设置串口波特率（BPS）:\r\n";

            // Enter user setting mode
            byte[] setBegin = new byte[] { 0x1D, 0x28, 0x45, 0x03, 0x00, 0x01, 0x49, 0x4E };
            ipWrite(setBegin, 0, setBegin.Length);

            // 设置打印机串口波特率
            byte[] setBaud = new byte[] { 0x1D, 0x28, 0x45, 0x00, 0x00, 0x0B, 0x01 };
            setBaud[3] = (byte)(comboBox3.Text.Length + 2);
            ipWrite(setBaud, 0, setBaud.Length);
            ipWrite(comboBox3.Text); 

            // Leave user setting mode
            byte[] setEnd = new byte[] { 0x1D, 0x28, 0x45, 0x04, 0x00, 0x02, 0x4F, 0x55, 0x54 };
            ipWrite(setEnd, 0, setEnd.Length);

            ipWrite("4.2 设置新串口波特率（BPS）!\n\n");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "设置打印速度为：" + comboBox4.Text + "\r\n";

            // Set printer speed
            byte[] setSpeed = new byte[] { 0x1D, 0x28, 0x4B, 0x02, 0x00, 0x32, 0x00 };
            setSpeed[6] = (byte)(comboBox4.Text[6]);
            ipWrite(setSpeed, 0, setSpeed.Length);

            ipWrite("2.1 设置打印速度为：" + comboBox4.Text + "\n\n");            
        }

        private void button9_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "2.2 测试设置后的速度快慢!\r\n";

            ipWrite("测试设置后的速度快慢!\n测试设置后的速度快慢!\n"
                            + "测试设置后的速度快慢!\n测试设置后的速度快慢!\n"
                            + "测试设置后的速度快慢!\n测试设置后的速度快慢!\n\n");
        }
                     
        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.textBox1.Text += text;
            }
        }

        private void Read()
        {
            while (_continue)
            {
                Byte[] byte_recv = new Byte[64];
                int byte_num;

                //从打印机端接受返回信息
                if (c.Connected == true)
                {
                    byte_num = c.Receive(byte_recv, byte_recv.Length, 0);
                    try
                    {
                        if (byte_num > 0)
                        {
                            this.SetText("\n" + ByteArrayToHexString(byte_recv, byte_num) + "\n");
                        }
                    }
                    catch (TimeoutException) { }
                }
            }
        }

        /// <summary> Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)</summary>
        /// <param name="data"> The array of bytes to be translated into a string of hex digits. </param>
        /// <returns> Returns a well formatted string of hex digits with spacing. </returns>
        private static string ByteArrayToHexString(byte[] data, int length)
        {
            StringBuilder sb = new StringBuilder(length * 8);

            //PadLeft,PadRight分别是左对齐和右对齐字符串长度，不足部分用指定字符填充
            for (int i = 0; i < length; i++)
            {
                if (data[i] != 0)
                {
                    sb.Append("<" + Convert.ToChar(data[i]) + ">");
                    sb.Append(Convert.ToString(data[i], 16).PadLeft(2, '0').PadLeft(3, ':'));
                    sb.Append("\r\n");
                }
                else
                {
                    sb.Append("< >:00\r\n");
                }
            }
            //组成结果如此, "<A>:38 <0>:30"

            return sb.ToString().ToUpper();
        }
   }
}
