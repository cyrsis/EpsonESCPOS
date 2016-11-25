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
        Bitmap bmp = null;
        
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
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            try
            {
                //openFileDialog1.InitialDirectory = "c:\\";
                openFileDialog1.Filter = "bmp files (*.bmp)|*.bmp|All files (*.*)|*.*";
                openFileDialog1.FilterIndex = 2;
                openFileDialog1.RestoreDirectory = true;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    //img = Image.FromFile(openFileDialog1.FileName);
                    bmp = new Bitmap(openFileDialog1.FileName, true);
                    pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                    pictureBox1.Image = bmp;
                }

                button2.Enabled = true;
                button3.Enabled = true;
                button4.Enabled = true;

                button9.Enabled = true;
                button10.Enabled = true;
                button11.Enabled = true;
                button12.Enabled = true;
                button13.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("打开BMP位图文件失败：" + ex.Message);
            }


        }

        private void button2_Click(object sender, EventArgs e)
        {
            int escType = 1;
            byte[] data = new byte[] { 0x1B, 0x33, 0x00 };
            ipWrite(data, 0, data.Length);
            data[0] = (byte)'\x00';
            data[1] = (byte)'\x00';
            data[2] = (byte)'\x00';    // Clear to Zero.

            Color pixelColor;

            // ESC * m nL nH
            byte[] escBmp = new byte[] { 0x1B, 0x2A, 0x00, 0x00, 0x00 };
           
            if (comboBox2.Text == "原始大小")
            {
                escBmp[2] = (byte)'\x21';
                escType = 3;
                ipWrite("\n1.原始大小!\n");
            }
            else if (comboBox2.Text == "倍宽")
            {
                escBmp[2] = (byte)'\x20';
                escType = 3;
                ipWrite("\n2.倍宽!\n");
            }
            else if (comboBox2.Text == "叁倍高")
            {
                escBmp[2] = (byte)'\x01';
                escType = 1;
                ipWrite("\n3.叁倍高!\n");
            }
            else if (comboBox2.Text == "叁倍高，倍宽")
            {
                escBmp[2] = (byte)'\x00';
                escType = 1;
                ipWrite("\n4.叁倍高，倍宽!\n");
            }

            if (escType == 1)
            {
                //nL, nH
                escBmp[3] = (byte)(bmp.Width % 256);                
                escBmp[4] = (byte)(bmp.Width / 256);
                            
                // data
                for (int i = 0; i < ((bmp.Height + 7) / 8); i++)
                {
                    ipWrite(escBmp, 0, escBmp.Length);

                    for (int j = 0; j < bmp.Width; j++)
                    {
                        for (int k = 0; k < 8; k++)
                        {
                            if (((i * 8) + k) < bmp.Height)  // if within the BMP size
                            {
                                pixelColor = bmp.GetPixel(j, (i * 8) + k);
                                if (pixelColor.R == 0)
                                {
                                    data[0] += (byte)(128 >> k);
                                }
                            }
                        }

                        ipWrite(data, 0, 1);
                        data[0] = (byte)'\x00'; // Clear to Zero.
                    }

                    ipWrite("\n");
                } // data
             }
             else if (escType == 3)
             {
                 //nL, nH
                 escBmp[3] = (byte)(bmp.Width % 256);
                 escBmp[4] = (byte)(bmp.Width / 256);


                 // data
                 for (int i = 0; i < (bmp.Height / 24) + 1; i++)
                 {
                     ipWrite(escBmp, 0, escBmp.Length);

                     for (int j = 0; j < bmp.Width; j++)
                     {
                         for (int k = 0; k < 24; k++)
                         {
                             if (((i * 24) + k) < bmp.Height)   // if within the BMP size
                             {
                                 pixelColor = bmp.GetPixel(j, (i * 24) + k);
                                 if (pixelColor.R == 0)
                                 {
                                     data[k / 8] += (byte)(128 >> (k % 8));
                                 }
                             }
                         }

                         ipWrite(data, 0, 3);
                         data[0] = (byte)'\x00';
                         data[1] = (byte)'\x00';
                         data[2] = (byte)'\x00';    // Clear to Zero.
                     }

                     ipWrite("\n");
                 } // data
             }

            ipWrite("\n1.1 完成<ESC *>方式的BMP输出!\n\n");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            byte[] data = new byte[] { 0x00 };
            Color pixelColor;

            // GS *, define the download bit image
            byte[] gsBmp = new byte[] { 0x1D, 0x2A, 0x00, 0x00 };
            
            gsBmp[2] = (byte)((bmp.Width + 7) / 8);
            gsBmp[3] = (byte)((bmp.Height + 7) / 8);
            ipWrite(gsBmp, 0, gsBmp.Length);

            for (byte x = 0; x < (gsBmp[2] * 8); x++)             // Width
            {
                for (byte y = 0; y < gsBmp[3]; y++)    // Height
                {
                    if (x < bmp.Width)
                    {
                        for (int k = 0; k < 8; k++)
                        {
                            if ( ((y * 8) + k) < bmp.Height)  // if within the BMP size
                            {
                                pixelColor = bmp.GetPixel(x, (y * 8) + k);
                                if (pixelColor.R == 0)
                                {
                                    data[0] += (byte)(128 >> k);
                                }
                            }
                        }
                    }
                    ipWrite(data, 0, 1);
                    data[0] = (byte)'\x00'; // Clear to Zero.
                }
            }

            ipWrite("\n2.1 完成<GS *>方式的位图下载!\n");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // GS /, print the download bit image
            byte[] gsBmp = new byte[] { 0x1D, 0x2F, 0x00 };

            if (comboBox3.Text == "原始大小")
            {
                gsBmp[2] = (byte)'\x30';
                ipWrite("\n1.原始大小!\n");
            }
            else if (comboBox3.Text == "倍宽")
            {
                gsBmp[2] = (byte)'\x31';
                ipWrite("\n2.倍宽!\n");
            }
            else if (comboBox3.Text == "倍高")
            {
                gsBmp[2] = (byte)'\x32';
                ipWrite("\n3.倍高!\n");
            }
            else if (comboBox3.Text == "倍宽倍高，四倍大小")
            {
                gsBmp[2] = (byte)'\x33';
                ipWrite("\n4.倍宽倍高，四倍大小!\n");
            }

            ipWrite(gsBmp, 0, gsBmp.Length);

            ipWrite("\n2.2 完成<GS />方式的下载位图打印!\n\n");
        }

        private void button5_Click(object sender, EventArgs e)
        {
            // <Function 48>
            // GS (/8 L, Transmit the NV graphics memory capacity.
            byte[] gsNV = new byte[] { 0x1D, 0x28, 0x4C, 0x02, 0x00, 0x30, 0x00 };
            ipWrite(gsNV, 0, gsNV.Length);

            textBox1.Text = "查询NV图形存储空间大小!\r\nTM-T81为256K=256 x 1024= 262144\r\n";
            ipWrite("\n3.4 <GS ( L>/<GS 8 L> 查询NV图形存储空间大小!\n\n");
        }

        private void button6_Click(object sender, EventArgs e)
        {
            // <Function 51>
            // GS (/8 L, Transmit the remaining capacity of the NV graphics memory
            byte[] gsNV = new byte[] { 0x1D, 0x28, 0x4C, 0x02, 0x00, 0x30, 0x03 };
            ipWrite(gsNV, 0, gsNV.Length);

            textBox1.Text = "查询剩余NV图形存储空间大小!\r\n";
            ipWrite("\n3.5 <GS ( L>/<GS 8 L> 查询剩余NV图形存储空间大小!\n\n");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            // <Function 64>
            // GS (/8 L, Transmit the key code list for defined NV graphics
            byte[] gsNV = new byte[] { 0x1D, 0x28, 0x4C, 0x04, 0x00, 0x30, 0x40, 0x4B, 0x43 };
            ipWrite(gsNV, 0, gsNV.Length);

            textBox1.Text = "读取NV存储中的所有键值!\r\n";
            ipWrite("\n3.6 <GS ( L>/<GS 8 L> 读取NV存储中的所有键值!\n\n");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            // <Function 65>
            // GS (/8 L, Delete all NV graphics data
            byte[] gsNV = new byte[] { 0x1D, 0x28, 0x4C, 0x05, 0x00, 0x30, 0x41, 0x43, 0x4C, 0x52 };
            ipWrite(gsNV, 0, gsNV.Length);

            textBox1.Text = "删除NV存储中的所有位图!\r\n";
            ipWrite("\n3.7 <GS ( L>/<GS 8 L> 删除NV存储中的所有位图!\n\n");
        }

        private void button9_Click(object sender, EventArgs e)
        {
            // <Function 67>
            ipWrite("\n\n3.1 <GS ( L>/<GS 8 L> 将选择位图与指定索引绑定，并存储到NV中!\n");
            ipWrite("\n\n索引范围：\n  32<= key1 <=126\n  32<= key1 <=126\n\n");
            textBox1.Text = "<GS ( L>/<GS 8 L> 将选择位图与指定索引绑定，并存储到NV中!\r\n";           

            byte[] data = new byte[] { 0x00 };
            Color pixelColor;

            // GS (/8 L, Define the NV graphics data (raster format)
            byte[] gsBmp = new byte[] { 0x1D, 0x28, 0x4C, 0x00, 0x00, 0x30, 0x43, 0x30, 
                                        0x20, 0x20, 0x01, 0x00, 0x00, 0x00, 0x00, 0x31 };
            int tempWB = (bmp.Width + 7) / 8;
            int tempSize = tempWB * bmp.Height + 11;

            // nL, nH, 后面总的字节个数
            gsBmp[3] = (byte)(tempSize % 256);
            gsBmp[4] = (byte)(tempSize / 256);

            // 位图的键值
            String[] bmpKey = comboBox4.Text.Split(new Char[] {','});
            gsBmp[8] = byte.Parse(bmpKey[0]);
            gsBmp[9] = byte.Parse(bmpKey[1]);                      

            // xL, xH; yL, yH
            gsBmp[11] = (byte)((tempWB * 8) % 256);
            gsBmp[12] = (byte)((tempWB * 8) / 256);
            gsBmp[13] = (byte)(bmp.Height % 256);
            gsBmp[14] = (byte)(bmp.Height / 256);

            ipWrite(gsBmp, 0, gsBmp.Length);

            for (byte y = 0; y < bmp.Height; y++)   // Height
            {
                for (int x = 0; x < tempWB; x++)    // Width
                {
                    for (int k = 0; k < 8; k++)
                    {
                        if (((x * 8) + k) < bmp.Width)  // if within the BMP width
                        {
                            pixelColor = bmp.GetPixel(((x * 8) + k), y);
                            if (pixelColor.R == 0)
                            {
                                data[0] += (byte)(128 >> k);
                            }
                        }
                    }

                    ipWrite(data, 0, 1);
                    data[0] = (byte)'\x00'; // Clear to Zero.
                }
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            // <Function 69>
            textBox1.Text = "<GS ( L>/<GS 8 L> 打印指定索引的位图!\r\n";
            ipWrite("\n3.2 <GS ( L>/<GS 8 L> 打印指定索引的位图!\n\n");

            // GS (/8 L, Print the specified NV graphics data
            byte[] gsBmp = new byte[] { 0x1D, 0x28, 0x4C, 0x06, 0x00, 
                                        0x30, 0x45, 0x00, 0x00, 0x00, 0x00 };
            
            // 位图的键值
            String[] bmpKey = comboBox4.Text.Split(',');
            gsBmp[7] = byte.Parse(bmpKey[0]);
            gsBmp[8] = byte.Parse(bmpKey[1]);

            // NV位图的放大方式
            if (comboBox5.Text == "原始大小")
            {
                gsBmp[9] = (byte)'\x01';
                gsBmp[10] = (byte)'\x01';
                ipWrite("\n1.原始大小!\n");
            }
            else if (comboBox5.Text == "倍宽")
            {
                gsBmp[9] = (byte)'\x02';
                gsBmp[10] = (byte)'\x01';
                ipWrite("\n2.倍宽!\n");
            }
            else if (comboBox5.Text == "倍高")
            {
                gsBmp[9] = (byte)'\x01';
                gsBmp[10] = (byte)'\x02';
                ipWrite("\n3.倍高!\n");
            }
            else if (comboBox5.Text == "四倍大小")
            {
                gsBmp[9] = (byte)'\x02';
                gsBmp[10] = (byte)'\x02';
                ipWrite("\n4.倍宽倍高，四倍大小!\n");
            }
            
            ipWrite(gsBmp, 0, gsBmp.Length);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            //遇到LF，也会输出"GS ( L n=112"刚定义的图片

            // GS (/8 L, Print the graphics data in the print buffer
            // <Function 50>

            ipWrite("\n2.4 <GS ( L>/<GS 8 L> 输出打印缓冲里的图形!\n");
            textBox1.Text = "输出打印缓冲里的位图!\r\n";
            
            byte[] gsNV = new byte[] { 0x1D, 0x28, 0x4C, 0x02, 0x00, 0x30, 0x02 };
            ipWrite(gsNV, 0, gsNV.Length);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            ipWrite("\n2.3 <GS ( L>/<GS 8 L> 存储图形到打印缓冲中...!\n");
            textBox1.Text = "先定义位图到打印缓冲中!\r\n";

            byte[] data = new byte[] { 0x00 };
            Color pixelColor;

            // GS ( L, Store the graphics data in the print buffer (raster format)
            // <Function 112>
            byte[] gsBmp = new byte[] { 0x1D, 0x28, 0x4C, 0x00, 0x00, 0x30, 0x70, 0x30, 
                                        0x01, 0x01, 0x31, 0x00, 0x00, 0x00, 0x00 };
            int tempWB = (bmp.Width + 7) / 8;
            int tempSize = tempWB * bmp.Height + 10;

            // nL, nH, 后面总的字节个数
            gsBmp[3] = (byte)(tempSize % 256);
            gsBmp[4] = (byte)(tempSize / 256);
            
            // 图片打印放大方式，是在定义的时候设置的。
            if (comboBox1.Text == "原始大小")
            {
                gsBmp[8] = (byte)'\x01';
                gsBmp[9] = (byte)'\x01';
                ipWrite("\n1.原始大小!\n");
            }
            else if (comboBox1.Text == "倍宽")
            {
                gsBmp[8] = (byte)'\x02';
                gsBmp[9] = (byte)'\x01';
                ipWrite("\n2.倍宽!\n");
            }
            else if (comboBox1.Text == "倍高")
            {
                gsBmp[8] = (byte)'\x01';
                gsBmp[9] = (byte)'\x02';
                ipWrite("\n3.倍高!\n");
            }
            else if (comboBox1.Text == "四倍大小")
            {
                gsBmp[8] = (byte)'\x02';
                gsBmp[9] = (byte)'\x02';
                ipWrite("\n4.倍宽倍高，四倍大小!\n");
            }
            
            // xL, xH; yL, yH
            gsBmp[11] = (byte)((tempWB * 8) % 256);
            gsBmp[12] = (byte)((tempWB * 8) / 256);
            gsBmp[13] = (byte)(bmp.Height % 256);
            gsBmp[14] = (byte)(bmp.Height / 256);

            ipWrite(gsBmp, 0, gsBmp.Length);

            for (byte y = 0; y < bmp.Height; y++)   // Height
            {
                for (int x = 0; x < tempWB; x++)    // Width
                {
                    for (int k = 0; k < 8; k++)
                    {
                        if (((x * 8) + k) < bmp.Width)  // if within the BMP width
                        {
                            pixelColor = bmp.GetPixel(((x * 8) + k), y);
                            if (pixelColor.R == 0)
                            {
                                data[0] += (byte)(128 >> k);
                            }
                        }
                    }

                    ipWrite(data, 0, 1);
                    data[0] = (byte)'\x00'; // Clear to Zero.
                }
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

        private void button11_Click(object sender, EventArgs e)
        {
            // <Function 66>
            // GS (/8 L, Delete the specified NV bitmap data
            byte[] gsBmp = new byte[] { 0x1D, 0x28, 0x4C, 0x04, 0x00, 0x30, 0x42, 0x20, 0x20 };

            // 位图的索引
            String[] bmpKey = comboBox4.Text.Split(',');
            gsBmp[7] = byte.Parse(bmpKey[0]);
            gsBmp[8] = byte.Parse(bmpKey[1]);

            ipWrite(gsBmp, 0, gsBmp.Length);

            textBox1.Text = "删除索引<" + comboBox4.Text + ">对应的位图!\r\n";
            ipWrite("\n3.3 <GS ( L>/<GS 8 L>删除索引<" + comboBox4.Text + ">对应的位图!\n\n");
        }
   }
}
