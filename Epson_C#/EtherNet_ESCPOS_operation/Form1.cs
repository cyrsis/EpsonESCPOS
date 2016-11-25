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
            this.textBox1.Text = "实时操作打印机，从错误中恢复!\r\n";
            
            // DLE ENQ n = 1~2
            byte[] dleEnq = new byte[] { 0x10, 0x05, 0x01 };
            ipWrite(dleEnq, 0, dleEnq.Length);

            ipWrite("1.1 实时操作打印机，从错误中恢复!\n\n");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "实时操作打印机，清空缓冲，再从错误中恢复!\r\n";

            // DLE ENQ n = 1~2
            byte[] dleEnq = new byte[] { 0x10, 0x05, 0x02 };
            ipWrite(dleEnq, 0, dleEnq.Length);

            ipWrite("1.2 实时操作打印机，清空缓冲，再从错误中恢复!\n\n");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "实时硬件操作，产生钱箱弹出脉冲!\r\n";

            // DLE DC4 fn m t
            byte[] dleDc4 = new byte[] { 0x10, 0x14, 0x01, 0x00, 0x01 };
            ipWrite(dleDc4, 0, dleDc4.Length);

            ipWrite("3.1 实时硬件操作，产生钱箱弹出脉冲!\n\n");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "实时硬件操作，执行关机例行操作!\r\n";

            // DLE DC4 fn m t
            byte[] dleDc4 = new byte[] { 0x10, 0x14, 0x02, 0x01, 0x08 };
            ipWrite(dleDc4, 0, dleDc4.Length);

            ipWrite("3.2 实时硬件操作，执行关机例行操作!\n\n");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "实时硬件操作，清空打印机缓冲!\r\n";

            // DLE DC4 fn m t
            byte[] dleDc4 = new byte[] { 0x10, 0x14, 0x08, 0x01, 0x03, 0x14, 0x01, 0x06, 0x02, 0x08 };
            ipWrite(dleDc4, 0, dleDc4.Length);

            ipWrite("3.3 实时硬件操作，清空打印机缓冲!\n\n");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "选择连接在打印机上的外设，通常是客显!\r\n";

            // ESC = n =1`3
            byte[] selectDM = new byte[] { 0x1B, 0x3D, 0x02 };
            ipWrite(selectDM, 0, selectDM.Length);

            ipWrite("4.1 选择连接在打印机上的外设，通常是客显!\n\n");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "选择打印机\r\n";

            // ESC = n =1`3
            byte[] selectPR = new byte[] { 0x1B, 0x3D, 0x03 };
            ipWrite(selectPR, 0, selectPR.Length);

            ipWrite("4.2 选择打印机\n\n");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "禁止打印机面板按键\r\n";

            // ESC c 5
            byte[] selectPR = new byte[] { 0x1B, 0x63, 0x35, 0x01 };
            ipWrite(selectPR, 0, selectPR.Length);

            ipWrite("2.1 禁止打印机面板按键\n\n");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "启用打印机面板按键\r\n";

            // ESC c 5
            byte[] selectPR = new byte[] { 0x1B, 0x63, 0x35, 0x00 };
            ipWrite(selectPR, 0, selectPR.Length);

            ipWrite("2.2 启用打印机面板按键\n\n");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "打印自检页一!\r\n";

            // GS ( A
            byte[] selfTest = new byte[] { 0x1D, 0x28, 0x41, 0x02, 0x00, 0x00, 0x02 };
            ipWrite(selfTest, 0, selfTest.Length);

            ipWrite("5.1 打印自检页一!\n\n");

            _continue = false;
            readThread.Abort();
            Thread.Sleep(3000);
            ipClose();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "打印字库自检!\r\n";

            // GS ( A
            byte[] selfTest = new byte[] { 0x1D, 0x28, 0x41, 0x02, 0x00, 0x00, 0x03 };
            ipWrite(selfTest, 0, selfTest.Length);

            ipWrite("5.2 打印字库自检!\n\n");

            _continue = false;
            readThread.Abort();
            Thread.Sleep(5000);
            ipClose();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            this.textBox1.Text = "禁止实时硬件操作命令!\r\n";

            // GS ( D
            byte[] disableDleDc4 = new byte[] { 0x1B, 0x28, 0x44, 0x05, 0x00, 0x14, 0x01, 0x00, 0x02, 0x00 };
            ipWrite(disableDleDc4, 0, disableDleDc4.Length);

            ipWrite("6 禁止实时硬件操作命令!\n\n");
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
                //打印自检后，需要重新连接！！
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
