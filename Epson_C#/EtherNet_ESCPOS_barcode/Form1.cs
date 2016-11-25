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
                    button2.Enabled = true;
                    button3.Enabled = true;
                    button4.Enabled = true;

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
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Feed and cut paper, GS V
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

            //------------------------------------------------------
            ipWrite("测试一维条码，格式<A>!\n\n");
            
            // 1.UPC-A, 选择HRI字符显示位置, Default
            ipWrite("\nUPC-A! no HRI\n\n");
            byte[] printBarcode = new byte[] { 0x1d, 0x6b, 0x00 };
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode = "098765432198\x0\xA";   //data = 12
            ipWrite(contentBarcode);
           
            // 2.UPC-E, HRI显示于条码上部，GS H n=1
            ipWrite("\nUPC-E! Shows HRI above\n\n");
            byte[] setHRI = new byte[] { 0x1D, 0x48, 0x01 };
            ipWrite(setHRI, 0, setHRI.Length);

            printBarcode[2] = (byte)'\x01';
            ipWrite(printBarcode, 0, printBarcode.Length);
            
            ipWrite(contentBarcode);  //UPC-E, data must be start from '0'

            // 3.JAN-13/EAN-13, HRI显示于条码下部，GS H n=2
            ipWrite("\nJAN-13/EAN-13! Shows HRI below\n\n");
            setHRI[2] = (byte)'\x02';
            ipWrite(setHRI, 0, setHRI.Length);

            printBarcode[2] = (byte)'\x02';
            ipWrite(printBarcode, 0, printBarcode.Length);

            ipWrite(contentBarcode);  //JAN-13/EAN-13, 不满13，自动加1位.

            // 4.JAN-8/EAN-8, HRI显示于条码上部和下部，GS H n=3
            ipWrite("\nJAN-8/EAN-8! Shows HRI above & below\n\n");
            setHRI[2] = (byte)'\x03';
            ipWrite(setHRI, 0, setHRI.Length);

            printBarcode[2] = (byte)'\x03';
            ipWrite(printBarcode, 0, printBarcode.Length);
            
            String contentBarcode2 = "87654321\x0\xA";   //data = 8
            ipWrite(contentBarcode2);
                        
            // 5.CODE39, HRI字体选择，FontA
            ipWrite("\nCODE39! HRI font is FontA\n\n");
            byte[] setHRIfont = new byte[] { 0x1D, 0x66, 0x00 };
            ipWrite(setHRIfont, 0, setHRIfont.Length);

            printBarcode[2] = (byte)'\x04';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode3 = "1A/. $%+-*\x0\xA";   //CODE38长度不限，适合算术内容
            ipWrite(contentBarcode3);

            // 6.ITF, HRI字体选择，FontB
            ipWrite("\nITF! HRI font is FontB\n\n");
            setHRIfont[2] = (byte)'\x01';
            ipWrite(setHRIfont, 0, setHRIfont.Length);

            printBarcode[2] = (byte)'\x05';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode4 = "1122334455667788\x0\xA";   //ITF, data must be even.
            ipWrite(contentBarcode4);

            // 7.CODEBAR, 恢复各项默认,HRI不显示，HRI font is FontA
            ipWrite("\nCODEBAR! Default: no HRI, HRI font is FontA\n\n");
            setHRI[2] = (byte)'\x00';
            ipWrite(setHRI, 0, setHRI.Length);

            setHRIfont[2] = (byte)'\x00';
            ipWrite(setHRIfont, 0, setHRIfont.Length);

            printBarcode[2] = (byte)'\x06';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode5 = "A12$./:+-d\x0\xA";   //CODEBAR
            ipWrite(contentBarcode5);

            // Feed and cut paper
            ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Feed and cut paper, GS V
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

            //------------------------------------------------------
            ipWrite("测试一维条码，格式<B>!\n\n");
            byte[] setHRI = new byte[] { 0x1D, 0x48, 0x02 };
            ipWrite(setHRI, 0, setHRI.Length);

            // 1.UPC-A, 条码宽度，即条码的最小单位宽度, default n=3
            ipWrite("\nUPC-A! Default barcode width\n\n");
            byte[] printBarcode = new byte[] { 0x1d, 0x6b, 0x41, 0x0C }; //n=12
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode = "098765432198\xA";   //data = 12
            ipWrite(contentBarcode);

            // 2.UPC-E, 条码宽度，即条码的最小单位宽度, GS w n=6
            ipWrite("\nUPC-E! Width n=6\n\n");
            byte[] setWidth = new byte[] { 0x1D, 0x77, 0x06 };  
            ipWrite(setWidth, 0, setWidth.Length);

            printBarcode[2] = (byte)'\x42';
            ipWrite(printBarcode, 0, printBarcode.Length);

            ipWrite(contentBarcode);  //UPC-E, data must be start from '0'

            // 3.JAN-13/EAN-13, 条码宽度，即条码的最小单位宽度, GS w n=2
            ipWrite("\nJAN-13/EAN-13! Width n=2\n\n");
            setWidth[2] = (byte)'\x02';
            ipWrite(setWidth, 0, setWidth.Length);

            printBarcode[2] = (byte)'\x43';
            ipWrite(printBarcode, 0, printBarcode.Length);

            ipWrite(contentBarcode);  //JAN-13/EAN-13, 不满13，自动加1位.

            // 4.JAN-8/EAN-8, 条码宽度，恢复默认, GS w n=3
            ipWrite("\nJAN-8/EAN-8! Width (default) n=3\n\n");
            setWidth[2] = (byte)'\x03';
            ipWrite(setWidth, 0, setWidth.Length);

            printBarcode[2] = (byte)'\x44';
            printBarcode[3] = (byte)'\x08';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode2 = "87654321\x0\xA";   //data = 8
            ipWrite(contentBarcode2);

            // 5.CODE39, 条码高度，T81每毫米为8个点，GS h n=32
            ipWrite("\nCODE39! Height is 4mm, n=32\n\n");
            byte[] setHeight = new byte[] { 0x1D, 0x68, 0x20 };
            ipWrite(setHeight, 0, setHeight.Length);

            printBarcode[2] = (byte)'\x45';
            printBarcode[3] = (byte)'\x0A';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode3 = "1A/. $%+-*\xA";   //CODE38长度不限，适合算术内容
            ipWrite(contentBarcode3);

            // 6.ITF, 修改条码高度，GS h n=240
            ipWrite("\nITF! Height is 3cm, n=240\n\n");
            setHeight[2] = (byte)'\xF0';
            ipWrite(setHeight, 0, setHeight.Length);

            printBarcode[2] = (byte)'\x46';
            printBarcode[3] = (byte)'\x10';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode4 = "1122334455667788\xA";   //ITF, data must be even.
            ipWrite(contentBarcode4);

            // 7.ITF, 修改条码高度，GS h n=162, default
            ipWrite("\nCODEBAR! Height is 2cm, (default)n=162\n\n");
            setHeight[2] = (byte)'\xA2';
            ipWrite(setHeight, 0, setHeight.Length);

            printBarcode[2] = (byte)'\x47';
            printBarcode[3] = (byte)'\x0A';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode5 = "A12$./:+-d\xA";   //CODEBAR
            ipWrite(contentBarcode5);

            // 8.CODE93,
            ipWrite("\nCODE93!\n\n");
            printBarcode[2] = (byte)'\x48';
            printBarcode[3] = (byte)'\x0A';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode6 = "aB!@#$%^&*\x0A";   //CODE93, n=1~127
            ipWrite(contentBarcode6);

            // 9.CODE128,
            ipWrite("\nCODE128! Table C: 1 char = 01~99 number\n\n");
            printBarcode[2] = (byte)'\x49';
            printBarcode[3] = (byte)'\x0E';
            ipWrite(printBarcode, 0, printBarcode.Length);

            String contentBarcode7 = "{A12345{C\x63\x62\x61\x60\x59\x0A";   //CODE128, {A,{B,{C
            ipWrite(contentBarcode7);

            // Feed and cut paper
            ipWrite(cutPaper, 0, cutPaper.Length); 
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Feed and cut paper, GS V
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

            //------------------------------------------------------
            ipWrite("测试二维条码，PDF417!\n\n");

            // 1.PDF417 default setting printing
            ipWrite("\nPDF417, default setting!\n\n");

            // Store the data in symbol storage area
            byte[] storePDF417 = new byte[] { 0x1D, 0x28, 0x6B, 0xB7, 0x00, 0x30, 0x50, 0x30 };
            ipWrite(storePDF417, 0, storePDF417.Length);

            String contentBarcode = "123456789012345678901234567890"
                                    + "abcedfghijklmnopqrstuvwxyzabcd"
                                    + "123456789012345678901234567890"
                                    + "abcedfghijklmnopqrstuvwxyzabcd"
                                    + "123456789012345678901234567890"
                                    + "abcedfghijklmnopqrstuvwxyzabcd"; //data = 180
            ipWrite(contentBarcode);

            // Print out data of PDF417 
            byte[] printPDF417 = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x51, 0x30 };
            ipWrite(printPDF417, 0, printPDF417.Length);

            // 2.PDF417 column set to be 4
            ipWrite("\nPDF417, set columns to be 4!\n\n");
            byte[] columnPDF417 = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x41, 0x04 };
            ipWrite(columnPDF417, 0, columnPDF417.Length);

            // Print out data of PDF417 
            ipWrite(printPDF417, 0, printPDF417.Length);

            // 3.PDF417 rows set to be 64
            ipWrite("\nPDF417, set rows to be 64!\n\n");
            columnPDF417[7] = (byte)'\x00';                         // columns to default
            ipWrite(columnPDF417, 0, columnPDF417.Length);

            // data =180, rows至少14行（28), rows > 28 才有意义, n=64
            byte[] rowPDF417 = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x42, 0x20 };
            ipWrite(rowPDF417, 0, rowPDF417.Length);

            // Print out data of PDF417 
            ipWrite(printPDF417, 0, printPDF417.Length);

            // 4.PDF417 set width of module
            ipWrite("\nPDF417, set width of module to be 5!\n\n");
            rowPDF417[7] = (byte)'\x00';                            // Rows to be auto
            ipWrite(rowPDF417, 0, rowPDF417.Length);

            // data =180, width of module, n=2~8, default n=3
            byte[] widthPDF417 = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x43, 0x05 };
            ipWrite(widthPDF417, 0, widthPDF417.Length);

            // Print out data of PDF417 
            ipWrite(printPDF417, 0, printPDF417.Length);

            // 5.PDF417 set height of row
            ipWrite("\nPDF417, set height of row to be 5!\n\n");
            widthPDF417[7] = (byte)'\x03';                            // width to be 3
            ipWrite(widthPDF417, 0, widthPDF417.Length);

            // data =180, height of row, n=2~8, default n=3
            byte[] heightPDF417 = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x44, 0x05 };
            ipWrite(heightPDF417, 0, heightPDF417.Length);

            // Print out data of PDF417 
            ipWrite(printPDF417, 0, printPDF417.Length);

            // 6.PDF417 error crrection level
            ipWrite("\nPDF417, error correction to be level 0!\n\n");
            heightPDF417[7] = (byte)'\x03';                            // height to be 3
            ipWrite(heightPDF417, 0, heightPDF417.Length);

            // data =180, error correction level to be level 0, word 2
            byte[] correctPDF417 = new byte[] { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x30, 0x45, 0x30, 0x30 };
            ipWrite(correctPDF417, 0, correctPDF417.Length);

            // Print out data of PDF417 
            ipWrite(printPDF417, 0, printPDF417.Length);

            // 7.PDF417 option, standard or truncated
            ipWrite("\nPDF417, option to be truncated!\n\n");
            correctPDF417[7] = (byte)'\x31';                            // error level to be 1
            correctPDF417[8] = (byte)'\x01';
            ipWrite(correctPDF417, 0, correctPDF417.Length);

            // data =180, error correction level to be level 0, word 2
            byte[] optionPDF417 = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x30, 0x46, 0x01 };
            ipWrite(optionPDF417, 0, optionPDF417.Length);

            // Print out data of PDF417 
            ipWrite(printPDF417, 0, printPDF417.Length);
            
            // Feed and cut paper
            ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            // Feed and cut paper, GS V
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

            //------------------------------------------------------
            ipWrite("测试二维条码，QR Code!\n\n");

            // 1.PDF417 default setting printing
            ipWrite("\nQR Code, default setting!\n\n");

            // Store the data in symbol storage area
            byte[] storeQRCode = new byte[] { 0x1D, 0x28, 0x6B, 0xB7, 0x00, 0x31, 0x50, 0x30 };
            ipWrite(storeQRCode, 0, storeQRCode.Length);

            String contentBarcode = "123456789012345678901234567890"
                                    + "abcedfghijklmnopqrstuvwxyzabcd"
                                    + "123456789012345678901234567890"
                                    + "abcedfghijklmnopqrstuvwxyzabcd"
                                    + "123456789012345678901234567890"
                                    + "abcedfghijklmnopqrstuvwxyzabcd"; //data = 180
            ipWrite(contentBarcode);

            // Print out data of QR Code
            byte[] printQRCode = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x51, 0x30 };
            ipWrite(printQRCode, 0, printQRCode.Length);

            // 2.QR Code select model 1, n=49, (default n=50)
            ipWrite("\nQR Code, set model 1! (default model 2)\n\n");
            byte[] modQRCode = new byte[] { 0x1D, 0x28, 0x6B, 0x04, 0x00, 0x31, 0x41, 0x31, 0x00 };
            ipWrite(modQRCode, 0, modQRCode.Length);

            // Print out data of PDF417 
            ipWrite(printQRCode, 0, printQRCode.Length);

            // 3.QR Code set size of module to 7 (n=1~16)
            ipWrite("\nQR Code, set size to 7! (default 3)\n\n");
            modQRCode[7] = (byte)'\x32';
            ipWrite(modQRCode, 0, modQRCode.Length);

            // size n=7, (default n=3)
            byte[] sizeQRCode = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x43, 0x07 };
            ipWrite(sizeQRCode, 0, sizeQRCode.Length);

            // Print out data of PDF417 
            ipWrite(printQRCode, 0, printQRCode.Length);

            // 4.QR Code error correction level to Q, (default L)
            ipWrite("\nQR Code, set error correction level to Q!\n\n");
            sizeQRCode[7] = (byte)'\x03';
            ipWrite(sizeQRCode, 0, sizeQRCode.Length);

            // Set error correction level to Q, (L, M, Q, H), default=L
            byte[] correctQRCode = new byte[] { 0x1D, 0x28, 0x6B, 0x03, 0x00, 0x31, 0x45, 0x32 };
            ipWrite(correctQRCode, 0, correctQRCode.Length);

            // Print out data of PDF417 
            ipWrite(printQRCode, 0, printQRCode.Length);

            // Return to default
            correctQRCode[7] = (byte)'\x30';
            ipWrite(correctQRCode, 0, correctQRCode.Length);
            
            // Feed and cut paper
            ipWrite(cutPaper, 0, cutPaper.Length);
        }  
    }
}
