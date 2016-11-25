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
                    button6.Enabled = true;
                    button7.Enabled = true;
                    button8.Enabled = true;
                    button9.Enabled = true;
                    button10.Enabled = true;
                    button11.Enabled = true;
                    button12.Enabled = true;
                    button13.Enabled = true;
                    button14.Enabled = true;
                    button15.Enabled = true;

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
            button6.Enabled = false;
            button7.Enabled = false;
            button8.Enabled = false;
            button9.Enabled = false;
            button10.Enabled = false;
            button11.Enabled = false;
            button12.Enabled = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "";

            // 2.GS I n, Read function
            // n = 0x45, font
            // n = 0x44, serial number
            // n = 0x43, TM printer model
            // n = 0x42, Vendor
            // n = 0x41, ESC/POS version

            byte[] readGSI = new byte[] { 0x1D, 0x49, 0x01 };
            //ipWrite(readGSI, 0, readGSI.Length);

            readGSI[2] = (byte)'\x41';
            ipWrite(readGSI, 0, readGSI.Length);

            ipWrite("1.1 读取了打印机固件信息!\n\n");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "";

            // 2.GS I n, Read function
            // n = 0x45, font
            // n = 0x44, serial number
            // n = 0x43, TM printer model
            // n = 0x42, Vendor
            // n = 0x41, ESC/POS version

            byte[] readGSI = new byte[] { 0x1D, 0x49, 0x01 };
            //ipWrite(readGSI, 0, readGSI.Length);

            readGSI[2] = (byte)'\x42';
            ipWrite(readGSI, 0, readGSI.Length);

            ipWrite("1.2 读取制造商信息!\n\n");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "";

            // 2.GS I n, Read function
            // n = 0x45, font
            // n = 0x44, serial number
            // n = 0x43, TM printer model
            // n = 0x42, Vendor
            // n = 0x41, ESC/POS version

            byte[] readGSI = new byte[] { 0x1D, 0x49, 0x01 };
            //ipWrite(readGSI, 0, readGSI.Length);

            readGSI[2] = (byte)'\x43';
            ipWrite(readGSI, 0, readGSI.Length);

            ipWrite("1.3 读取产品型号!\n\n");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "";

            // 2.GS I n, Read function
            // n = 0x45, font
            // n = 0x44, serial number
            // n = 0x43, TM printer model
            // n = 0x42, Vendor
            // n = 0x41, ESC/POS version

            byte[] readGSI = new byte[] { 0x1D, 0x49, 0x01 };
            //ipWrite(readGSI, 0, readGSI.Length);

            readGSI[2] = (byte)'\x44';
            ipWrite(readGSI, 0, readGSI.Length);

            ipWrite("1.4 读取产品序列号!\n\n");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "";

            // 2.GS I n, Read function
            // n = 0x45, font
            // n = 0x44, serial number
            // n = 0x43, TM printer model
            // n = 0x42, Vendor
            // n = 0x41, ESC/POS version

            byte[] readGSI = new byte[] { 0x1D, 0x49, 0x01 };
            //ipWrite(readGSI, 0, readGSI.Length);

            readGSI[2] = (byte)'\x45';
            ipWrite(readGSI, 0, readGSI.Length);

            ipWrite("1.5 读取板载字库!\n\n");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "如果纸尽与纸将尽传感器都监测有纸，返回值为0!\r\n";

            // 2.GS r n=1,2; Read status

            byte[] readGSr = new byte[] { 0x1D, 0x72, 0x01 };
            ipWrite(readGSr, 0, readGSr.Length);

            ipWrite("3.1 如果纸尽与纸将尽传感器都监测有纸，返回值为0!\n\n");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "请确保连接了钱箱，否则返回值无意义!\r\n";

            // 2.GS r n=1,2; Read status

            byte[] readGSr = new byte[] { 0x1D, 0x72, 0x02 };
            ipWrite(readGSr, 0, readGSr.Length);

            ipWrite("3.2 请确保连接了钱箱，否则返回值无意义!\n\n");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "看一下钱箱弹出了没有?!\r\n";

            // ESC p m t1 t2

            byte[] cashDrawer = new byte[] { 0x1B, 0x70, 0x00, 0x30, 0xC0 };
            ipWrite(cashDrawer, 0, cashDrawer.Length);

            ipWrite("6.1 看一下钱箱弹出了没有?!\n\n");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "启用所有可监测状态的监测!\r\n";

            // GS a n = 1+2+4+8
            byte[] ASBOn = new byte[] { 0x1D, 0x61, 0xFF };
            ipWrite(ASBOn, 0, ASBOn.Length);

            ipWrite("4.1 启用所有可监测状态的监测!\n\n");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "关闭所有状态的监测!\r\n";

            // GS a n = 1+2+4+8
            byte[] ASBOff = new byte[] { 0x1D, 0x61, 0x00 };
            ipWrite(ASBOff, 0, ASBOff.Length);

            ipWrite("4.2 关闭所有状态的监测!\n\n");
        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "实时打印机状态获取!\r\n";

            // DLE EOT n=1~4
            byte[] dleEot = new byte[] { 0x10, 0x04, 0x01 };
            ipWrite(dleEot, 0, dleEot.Length);

            ipWrite("2.1 实时打印机状态获取!\n\n");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "实时脱机原因!\r\n";

            // DLE EOT n=1~4
            byte[] dleEot = new byte[] { 0x10, 0x04, 0x02 };
            ipWrite(dleEot, 0, dleEot.Length);

            ipWrite("2.2 实时脱机原因!\n\n");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "实时错误原因!\r\n";

            // DLE EOT n=1~4
            byte[] dleEot = new byte[] { 0x10, 0x04, 0x03 };
            ipWrite(dleEot, 0, dleEot.Length);

            ipWrite("2.3 实时错误原因!\n\n");
        }

        private void button14_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "实时传感器状态!\r\n";

            // DLE EOT n=1~4
            byte[] dleEot = new byte[] { 0x10, 0x04, 0x04 };
            ipWrite(dleEot, 0, dleEot.Length);

            ipWrite("2.4 实时传感器状态!\n\n");
        }

        private void button15_Click(object sender, EventArgs e)
        {
            if (textBox2.Text.Length != 4)
            {
                MessageBox.Show("嘿，处理过程的ID号必须是4位数字或字母！");
            }
            else
            {
                this.textBox1.Text = "在打印任务的结尾加个处理ID号\r\n"
                                    + "通过读取返回的这个ID号，\r\n"
                                    + "从而可以间接确保打印任务的完成!\r\n\r\n"
                                    + "也可通俗地理解为爱普生的防止丢单设计.\r\n";

                ipWrite("5.1 我是一个复杂的打印任务\n在我的结尾加个处理过程ID号\n这样可间接监测打印的完成！\n很多人喜欢将这通俗的称为防丢单设计");

                // GS ( H, 在打印任务的结尾加个处理过程ID号
                byte[] processID = new byte[] { 0x1D, 0x28, 0x48, 0x06, 0x00, 0x30, 0x30 };
                ipWrite(processID, 0, processID.Length);
                ipWrite(textBox2.Text);
            }
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

        /// <summary> Converts an array of bytes into a formatted string of hex digits (ex: E4 CA B2)</summary>
        /// <param name="data"> The array of bytes to be translated into a string of hex digits. </param>
        /// <returns> Returns a well formatted string of hex digits with spacing. </returns>
        private static string ByteArrayToHexString(byte[] data, int length)
        {
            StringBuilder sb = new StringBuilder(length * 8);

            //PadLeft,PadRight分别是左对齐和右对齐字符串长度，不足部分用指定字符填充
            for(int i = 0; i < length; i++)
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
