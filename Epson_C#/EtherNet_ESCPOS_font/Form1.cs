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
//using System.Threading;   不含讀操作

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
            // IP地址檢查
            if ((numericUpDown1.Value != 10)
                && (numericUpDown1.Value != 172) && (numericUpDown1.Value != 192))
            {
                MessageBox.Show("私有IP地址允許的網段范圍是:\n"
                                + "10.0.0.0--10.255.255.255\n"
                                + "172.16.0.0--172.31.255.255\n"
                                + "192.168.0.0--192.168.255.255\n");
            }
            else if ((numericUpDown1.Value == 172)
                 && ((numericUpDown2.Value < 16) || (numericUpDown2.Value > 31)))
            {
                MessageBox.Show("私有IP地址允許的網段范圍是:\n"
                                + "172.16.0.0--172.31.255.255\n");
            }
            else if ((numericUpDown1.Value == 192) && (numericUpDown2.Value != 168))
            {
                MessageBox.Show("私有IP地址允許的網段范圍是:\n"
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
                    //把ip和端口轉化為IPEndPoint實例
                    IPEndPoint ip_endpoint = new IPEndPoint(ip, port);

                    //創建一個Socket
                    c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


                    //連接到服務器
                    c.Connect(ip_endpoint);
                    //應對同步Connect超時過長的辦法，猜測應該是先用異步方式建立以個連接然后，
                    //確認連接是否可用，然后報錯或者關閉后，重新建立一個同步連接                    

                    c.SendTimeout = 1000;

                    //初始化打印機，並打印
                    ipWrite("\x1b\x40打開TCP/IP連接!\n-------------------------------\n\n");

                    // 操作按鈕啟用
                    buttonClosePort.Enabled = true;
                    buttonOpenPort.Enabled = false;
                    button1.Enabled = true;
                    button2.Enabled = true;
                    button3.Enabled = true;
                    button4.Enabled = true;
                    button5.Enabled = true;
                    button6.Enabled = true;
                    button7.Enabled = true;
                }
                catch (ArgumentNullException e1)
                {
                    //MessageBox.Show(String.Format("參數意外:{0}", e1));
                    MessageBox.Show("Socket參數設置錯誤!");
                }
                catch (SocketException e2)
                {
                    //MessageBox.Show(String.Format("Socket連接意外:{0}", e2));
                    MessageBox.Show("連接不到指定IP的打印機!");
                }
            }
        }

        private void ipWrite(String str_send)
        {
            //String str_send = "愛普生（中國）有限公司!\n";
            Byte[] byte_send = Encoding.GetEncoding("gb18030").GetBytes(str_send);
            ipWrite(byte_send, 0, byte_send.Length);
        }

        private void ipWrite(Byte[] byte_send, int start, int length)
        {
            try
            {
                //發送測試信息
                c.Send(byte_send, length, 0);
            }
            catch (SocketException e2)
            {
                MessageBox.Show(String.Format("Socket連接意外:{0}", e2));
            }
        }

        //private String ipRead()
        //{
        //    //String str_recv = "";                    
        //    //Byte[] byte_recv = new Byte[512];
        //    //int byte_num;

        //    ////從服務器端接受返回信息
        //    //byte_num = c.Receive(byte_recv, byte_recv.Length, 0);
        //    //str_recv += Encoding.ASCII.GetString(byte_recv, 0, byte_num);

        //    ////顯示服務器返回信息
        //    //ipWrite("收到數據:\n" + str_recv);

        //    //return str_recv;
        //}

        private void buttonClosePort_Click(object sender, EventArgs e)
        {
            // 結束演示，關閉IP連接
            ipWrite("\n-------------------------------\n關閉TCP/IP連接!\n");
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
            /*------ String data to be written ------*/
            String msg1 = "EPSON (CHINA) CORP.\x0A";
            String msg2 = "愛普生(中國)有限公司\x0A";
            int i = 0;

            // Set Font size, GS !
            byte[] fontSize = new byte[] { 0x1D, 0x21, 0x00 };

            // Feed 4 lines, ESC d  
            byte[] feed4Lines = new byte[] { 0x1b, 0x64, 0x04 };

            // Feed and cut paper, GS V
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

            //------------------------------------------------------
            // 1.Normal size
            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            // 2.Double width
            ipWrite("Double width\n");

            fontSize[2] = (byte)'\x10';
            ipWrite(fontSize, 0, fontSize.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontSize[2] = (byte)'\x00';                         //Set back to normal
            ipWrite(fontSize, 0, fontSize.Length);

            // 3.Double height
            ipWrite("Double height\n");

            fontSize[2] = (byte)'\x01';
            ipWrite(fontSize, 0, fontSize.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontSize[2] = (byte)'\x00';                         //Set back to normal
            ipWrite(fontSize, 0, fontSize.Length);

            // 4.Set font to be 3x3
            ipWrite("3x3 size\n");

            fontSize[2] = (byte)'\x22';
            ipWrite(fontSize, 0, fontSize.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontSize[2] = (byte)'\x00';                         //Set back to normal
            ipWrite(fontSize, 0, fontSize.Length);

            // 5.Set font to be biggest, 8x8
            ipWrite("8x8 size, the biggest!\n");
            ipWrite("And demo turn smoothing mode ON\n");

            fontSize[2] = (byte)'\x77';
            ipWrite(fontSize, 0, fontSize.Length);

            ipWrite(msg1);
            ipWrite(msg2);

            fontSize[1] = (byte)'\x62';
            fontSize[2] = (byte)'\x01';
            ipWrite(fontSize, 0, fontSize.Length);

            ipWrite(msg1);
            ipWrite(msg2);

            fontSize[2] = (byte)'\x00';
            ipWrite(fontSize, 0, fontSize.Length);

            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontSize[1] = (byte)'\x21';
            fontSize[2] = (byte)'\x00';                         //Set back to normal
            ipWrite(fontSize, 0, fontSize.Length);

            // Feed and cut paper
            ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            /*------ String data to be written ------*/
            String msg1 = "EPSON (CHINA) CORP.\x0A";
            String msg2 = "愛普生(中國)有限公司\x0A";
            int i = 0;

            // Set Font style, ESC !
            byte[] fontStyle = new byte[] { 0x1b, 0x21, 0x00 };

            // Feed 4 lines, ESC d  
            byte[] feed4Lines = new byte[] { 0x1b, 0x64, 0x04 };

            // Feed and cut paper, GS V
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };

            //------------------------------------------------------
            // 1.Default font sytle
            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            // 2.Select fontB,  or use "ESC M"
            ipWrite("ANK fontB\n");
            fontStyle[2] = (byte)'\x01';
            ipWrite(fontStyle, 0, fontStyle.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontStyle[2] = (byte)'\x00';
            ipWrite(fontStyle, 0, fontStyle.Length);

            // 3.Set emphasized mode, or use "ESC E"
            ipWrite("Emphasized\n");

            fontStyle[2] = (byte)'\x08';
            ipWrite(fontStyle, 0, fontStyle.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontStyle[2] = (byte)'\x00';
            ipWrite(fontStyle, 0, fontStyle.Length);

            // 4.Set underline mode, or use "ESC -"
            ipWrite("Underline\n");

            fontStyle[2] = (byte)'\x80';
            ipWrite(fontStyle, 0, fontStyle.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontStyle[2] = (byte)'\x00';
            ipWrite(fontStyle, 0, fontStyle.Length);

            // 5.Set underline 2-dot width
            ipWrite("Underline 2-dot\n");

            fontStyle[1] = (byte)'\x2d';
            fontStyle[2] = (byte)'\x02';
            ipWrite(fontStyle, 0, fontStyle.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontStyle[2] = (byte)'\x00';
            ipWrite(fontStyle, 0, fontStyle.Length);

            // 6.Turn 90 degree clockwise rotation mode on/off
            ipWrite("Turn 90 degree clockwise ON\n");

            fontStyle[1] = (byte)'\x56';
            fontStyle[2] = (byte)'\x01';
            ipWrite(fontStyle, 0, fontStyle.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontStyle[2] = (byte)'\x00';
            ipWrite(fontStyle, 0, fontStyle.Length);

            // 7.Turn upside-down
            ipWrite("Turn on upside-down\n");

            fontStyle[1] = (byte)'\x7b';
            fontStyle[2] = (byte)'\x01';
            ipWrite(fontStyle, 0, fontStyle.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontStyle[2] = (byte)'\x00';
            ipWrite(fontStyle, 0, fontStyle.Length);

            // 8.Turn white/black reverse print mode on/off, GS B
            ipWrite("Turn white/black reverse mode ON\n");

            fontStyle[0] = (byte)'\x1d';
            fontStyle[1] = (byte)'\x42';
            fontStyle[2] = (byte)'\x01';
            ipWrite(fontStyle, 0, fontStyle.Length);

            for (i = 0; i < 3; i++)
            {
                ipWrite(msg1);
                ipWrite(msg2);
            }
            ipWrite(feed4Lines, 0, feed4Lines.Length);

            fontStyle[2] = (byte)'\x00';
            ipWrite(fontStyle, 0, fontStyle.Length);

            // Feed and cut paper
            ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            /*------ String data to be written ------*/
            String msg1 = "EPSON (CHINA) CORP.\x0A";
            String msg2 = "愛普生(中國)有限公司\x0A";

            // 1.Set chars alignment, ESC a n=0~2
            byte[] fontAlign = new byte[] { 0x1b, 0x61, 0x00 };

            // 默認是左對齊
            ipWrite("1.左對齊\n");
            ipWrite(msg1);
            ipWrite(msg2);

            // 居中
            fontAlign[2] = (byte)'\x01';
            ipWrite(fontAlign, 0, fontAlign.Length);
            ipWrite("居中\n");
            ipWrite(msg1);
            ipWrite(msg2);

            // 靠右
            fontAlign[2] = (byte)'\x02';
            ipWrite(fontAlign, 0, fontAlign.Length);
            ipWrite("右對齊\n");
            ipWrite(msg1);
            ipWrite(msg2);

            fontAlign[2] = (byte)'\x00';
            ipWrite(fontAlign, 0, fontAlign.Length);

            // 2.Print and feed paper, ESC J
            ipWrite("2.打印並進紙若干距離");
            byte[] printFeedPaper = new byte[] { 0x1b, 0x4A, 0xF0 };
            ipWrite(printFeedPaper, 0, printFeedPaper.Length);

            // 3.Set absolute print position
            ipWrite("3.設置絕對打印位置\n\n");

            byte[] abPosition = new byte[] { 0x1b, 0x24, 0x60, 0x00 };
            ipWrite(abPosition, 0, abPosition.Length);

            ipWrite("你看我縮進打印位置了吧!\n");

            abPosition[2] = (byte)'\x0';
            ipWrite(abPosition, 0, abPosition.Length);

            ipWrite("又恢復了!\n\n");

            // 4.設置字符間距
            ipWrite("4.字符間距的設置隻對西文字符起作用\n\n");

            byte[] charSpace = new byte[] { 0x1b, 0x20, 0x10 };
            ipWrite(charSpace, 0, charSpace.Length);
            ipWrite("asdfghjklqwer\n");
            ipWrite("你看對漢字不起作用的!\n\n");

            charSpace[2] = (byte)'\x00';
            ipWrite(charSpace, 0, charSpace.Length);
            ipWrite("Turn to normal char space!\n\n");

            // 5.Tab位置自定義
            ipWrite("5.Tab位置自定義\n\n");
            ipWrite("Start\tTab8\tTab16\tTab24\tTab32\tTab40\n");

            byte[] tabDefine = new byte[] { 0x1b, 0x44, 0x0A, 0x14, 0x1E, 0x28, 0x00 };
            ipWrite(tabDefine, 0, tabDefine.Length);

            ipWrite("Start\tTab10\tTab20\tTab30\tTab40\n\n");

            // 6.相對位置移動打印, ESC \
            ipWrite("6.相對位置移動打印\n\n");
            ipWrite("\t起初在這裡打印");

            byte[] relMove = new byte[] { 0x1B, 0x5C, 0x40, 0x80 };
            ipWrite(relMove, 0, relMove.Length);
            ipWrite("然后在這!\n\n");

            // 7.設置左邊留白，GS L
            ipWrite("7.設置左邊空白\n\n");

            byte[] leftBlank = new byte[] { 0x1D, 0x4C, 0x50, 0x00 };
            ipWrite(leftBlank, 0, leftBlank.Length);
            ipWrite("看到左邊留出的空白了嗎？看到左邊留出的空白了嗎？看到左邊留出的空白了嗎？看到左邊留出的空白了嗎？\n");

            byte[] printWidth = new byte[] { 0x1D, 0x57, 0x00, 0x01 };
            ipWrite(printWidth, 0, printWidth.Length);
            ipWrite("縮小允許打印的寬度，看到效果了嗎？縮小允許打印的寬度，看到效果了嗎？\n\n");

            byte[] initPrinter = new byte[] { 0x1B, 0x40 };
            ipWrite(initPrinter, 0, initPrinter.Length);

            // Feed and cut paper
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };
            ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            /*------ String data to be written ------*/
            String msg = "愛普生(香港)有限公司\x0A";

            // 1.行間距離 ESC 3 n=0~255
            byte[] lineSpace = new byte[] { 0x1b, 0x33, 0x00 };

            // 默認行間距
            ipWrite("1.默認行間距\n");
            ipWrite(msg);
            ipWrite(msg);

            // 最小行間距，n=0
            ipWrite(lineSpace, 0, lineSpace.Length);
            ipWrite("\n行間距為0\n");
            ipWrite(msg);
            ipWrite(msg);

            // 行間距離為48
            lineSpace[2] = (byte)'\x30';
            ipWrite(lineSpace, 0, lineSpace.Length);

            ipWrite("\n行間距離為48\n");
            ipWrite(msg);
            ipWrite(msg);

            // 行間距離為96
            lineSpace[2] = (byte)'\x60';
            ipWrite(lineSpace, 0, lineSpace.Length);

            ipWrite("\n行間距離為96\n");
            ipWrite(msg);
            ipWrite(msg);

            // 行間距離為192
            lineSpace[2] = (byte)'\xC0';
            ipWrite(lineSpace, 0, lineSpace.Length);

            ipWrite("\n行間距離為192\n");
            ipWrite(msg);
            ipWrite(msg);

            // 2.恢復默認行間距
            byte[] lineSpaceDefault = new byte[] { 0x1b, 0x32 };
            ipWrite(lineSpaceDefault, 0, lineSpaceDefault.Length);

            ipWrite("恢復默認行間距\n");
            ipWrite(msg);
            ipWrite(msg);

            // Feed and cut paper
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };
            ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ipWrite("自定義字符並打印\n");
            ipWrite("特別注意，自定義字符和位圖定義不能同時使用！\n");

            byte[] defineUserChar = new byte[] { 0x1B, 0x26, 0x03, 0x30, 0x30, 0x0C, 
                                0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 
                                0x01, 0x02, 0x03, 0x01, 0x02, 0x03, 
                                0x01, 0x02, 0x03, 0x01, 0x02, 0x03, 
                                0x01, 0x02, 0x03, 0x01, 0x02, 0x03, 
                                0x01, 0x02, 0x03, 0x01, 0x02, 0x03, 
                                0x01, 0x02, 0x03, 0x01, 0x02, 0x03 };
            ipWrite(defineUserChar, 0, defineUserChar.Length);

            byte[] selectUserChar = new byte[] { 0x1B, 0x25, 0x01 };    //用戶定義區
            ipWrite(selectUserChar, 0, selectUserChar.Length);

            byte[] printUserChar = new byte[] { 0x0A, 0x30, 0x0A };
            ipWrite(printUserChar, 0, printUserChar.Length);

            selectUserChar[2] = (byte)'\x0';                            //退出用戶定義區
            ipWrite(selectUserChar, 0, selectUserChar.Length);

            ipWrite(printUserChar, 0, printUserChar.Length);

            byte[] cancelUserChar = new byte[] { 0x1B, 0x3F, 0x30 };    //刪除用戶定義的某字符
            ipWrite(cancelUserChar, 0, cancelUserChar.Length);

            //驗証自定義的字符是否刪除了定義
            //ipWrite(selectUserChar, 0, selectUserChar.Length);
            //ipWrite(printUserChar, 0, printUserChar.Length);
            //selectUserChar[2] = (byte)'\x0';                            //退出用戶定義區
            //ipWrite(selectUserChar, 0, selectUserChar.Length);

            // Feed and cut paper
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };
            ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            ipWrite("選擇國際字符集\n");
            ipWrite("在ASCII 0～127范圍內，各國對某些字符有習慣定義\n");

            byte[] interCharSet = new byte[] { 0x1B, 0x52, 0x00 };
            byte[] interChars = new byte[] { 0x23, 0x24, 0x40, 0x5B, 0x5C, 0x5D, 0x5E,
                                             0x60, 0x7B, 0x7C, 0x7D, 0x7E, 0x0A };

            String[] interCharList = new String[] {"U.S.A.", "France", "Germany", 
                                                   "U.K.", "Denmark I", "Sweden",
                                                   "Italy", "Spain I", "Japan",
                                                   "Norway", "Denmark II", "Spain II",
                                                   "Latin America", "Korea", "Slovenia / Croatia",
                                                   "China"};
            for (int i = 0; i < 16; i++)
            {
                interCharSet[2] = (byte)i;
                ipWrite(interCharSet, 0, interCharSet.Length);
                ipWrite(String.Format("\nn = {0}, {1}\n", i, interCharList[i]));
                ipWrite(interChars, 0, interChars.Length);
            }

            // Feed and cut paper
            byte[] cutPaper = new byte[] { 0x1D, 0x56, 0x42, 0x00 };
            ipWrite(cutPaper, 0, cutPaper.Length);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ipWrite("選擇國際字符表\n");
            ipWrite("在單字節字符的128～255的范圍內，各國對某些字符有習慣定義。\n");

            byte[] fsDot = new byte[] { 0x1C, 0x2E };
            ipWrite(fsDot, 0, fsDot.Length);

            byte[] interTableSet = new byte[] { 0x1B, 0x74, 0x00 };
            byte[] interTables = new byte[] { 0x00 };

            String[] interTableList = new String[] {"Page 0 [PC437 (USA: Standard Europe)]",
                                                    "Page 1 [Katakana]",
                                                    "Page 2 [PC850: Multilingual]",
                                                    "Page 3 [PC860 (Portuguese)]",
                                                    "Page 4 [PC863 (Canadian-French)",
                                                    "Page 5 [PC865 (Nordic)]",
                                                    "Page 16 [WPC1252]",
                                                    "Page 17 [PC866 (Cyrillic #2)]",
                                                    "Page 18 [PC852 (Latin 2)]",
                                                    "Page 19 [PC858 (Euro)]" };
            int[] interTableNumber = new int[] { 0, 1, 2, 3, 4, 5, 16, 17, 18, 19 };

            for (int i = 0; i < interTableList.Length; i++)
            {
                ipWrite(String.Format("\n\n\n n = {0}, {1}\n", interTableNumber[i], interTableList[i]));

                interTableSet[2] = (byte)interTableNumber[i];
                ipWrite(interTableSet, 0, interTableSet.Length);

                for (int j = 128; j < 255; j++)
                {
                    interTables[0] = (byte)j;
                    ipWrite(interTables, 0, interTables.Length);
                }
            }

            // Feed and cut paper
            byte[] cutPaper = new byte[] { 0x0A, 0x1D, 0x56, 0x42, 0x00 };
            ipWrite(cutPaper, 0, cutPaper.Length);
        }
    }
}
