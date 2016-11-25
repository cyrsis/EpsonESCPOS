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
//using System.Threading;   不含读操作

namespace Serial_ESCPOS
{
    public partial class Form1 : Form
    {
        Socket c = null;
        String str_ip = null;
        int port = 9100;

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

                    // 操作按钮启用
                    buttonClosePort.Enabled = true;
                    buttonOpenPort.Enabled = false;
                    button1.Enabled = true;
                    button6.Enabled = true;
                    button7.Enabled = true;
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
            // 结束演示，关闭IP连接
            ipWrite("\n-------------------------------\n关闭TCP/IP连接!\n");
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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ipWrite("进入页模式!\n\n");
            
            // ESC L
            byte[] enterPagemode = new byte[] { 0x1b, 0x4c };
            ipWrite(enterPagemode, 0, enterPagemode.Length);

            button1.Enabled = false;
            button2.Enabled = true;
            button3.Enabled = true;
            button4.Enabled = true;
            button5.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // ESC W, 
            byte[] defPagemode = new byte[] { 0x1B, 0x57, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x02};
            ipWrite(defPagemode, 0, defPagemode.Length);

            // ESC T n=0
            byte[] directPagemode = new byte[] { 0x1B, 0x54, 0x00 };
            ipWrite(directPagemode, 0, directPagemode.Length);
            ipWrite("我是页模式里的壹行字-----------\n");

            // ESC T n=1
            directPagemode[2] = (byte)'\x01';
            ipWrite(directPagemode, 0, directPagemode.Length);
            ipWrite("我是页模式里的两行字\n");
            ipWrite("我是页模式里的两行字-----------\n");

            // ESC T n=2
            directPagemode[2] = (byte)'\x02';
            ipWrite(directPagemode, 0, directPagemode.Length);
            ipWrite("我是页模式里的叁行字\n");
            ipWrite("我是页模式里的叁行字\n");
            ipWrite("我是页模式里的叁行字-----------\n");

            // ESC T n=2
            directPagemode[2] = (byte)'\x03';
            ipWrite(directPagemode, 0, directPagemode.Length);
            ipWrite("我是页模式里的肆行字\n");
            ipWrite("我是页模式里的肆行字\n");
            ipWrite("我是页模式里的肆行字\n");
            ipWrite("我是页模式里的肆行字-----------\n");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // ESC W, 
            byte[] defPagemode = new byte[] { 0x1B, 0x57, 0x50, 0x00, 0x50, 0x00, 0x00, 0x01, 0x00, 0x02 };
            ipWrite(defPagemode, 0, defPagemode.Length);

            // ESC T n=0
            byte[] directPagemode = new byte[] { 0x1B, 0x54, 0x03 };
            ipWrite(directPagemode, 0, directPagemode.Length);
         
            //空出条码高度的空间！重要！
            byte[] movedownPagemode = new byte[] { 0x1D, 0x5C, 0x80, 0x00 };
            ipWrite(movedownPagemode, 0, movedownPagemode.Length);

            // 1.UPC-A
            byte[] setHRI = new byte[] { 0x1D, 0x48, 0x02 };
            ipWrite(setHRI, 0, setHRI.Length);

            byte[] printBarcode = new byte[] { 0x1d, 0x6b, 0x00 };
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode = "098765432198\x0\xA";   //data = 12
            ipWrite(contentBarcode);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            byte[] printPagemode = new byte[] { 0x1B, 0x0C };
            ipWrite(printPagemode, 0, printPagemode.Length);

            // Feed and cut paper, GS V
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };
            ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // CAN, ESC S
            byte[] cancelPagemode = new byte[] { 0x18, 0x1B, 0x53 };
            ipWrite(cancelPagemode, 0, cancelPagemode.Length);

            ipWrite("放弃页模式数据，并返回标准模式!\n");            

            button1.Enabled = true;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            button5.Enabled = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // GS P x y
            byte[] movUnits = new byte[] { 0x1D, 0x50, 0x00, 0x00 };
            ipWrite(movUnits, 0, movUnits.Length);

            ipWrite("将水平和垂直移动单位改回默认值! 203 & 203\n");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // GS P x y
            byte[] movUnits = new byte[] { 0x1D, 0x50, 0x00, 0x00 };

            movUnits[2] = (byte)numericUpDownH.Value;
            movUnits[3] = (byte)numericUpDownV.Value;
            ipWrite(movUnits, 0, movUnits.Length);

            ipWrite("设置水平移动单位 =" + numericUpDownH.Value 
                            + "垂直移动单位 =" + numericUpDownV.Value + "\n");

        }
    }
}
