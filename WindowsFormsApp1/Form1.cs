using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        Random random = new Random();
        Thread threadWatch = null; //负责监听客户端的线程
        Socket socketWatch = null; //负责监听客户端的套接字
        //创建一个负责和客户端通信的套接字 
        List<Socket> socConnections = new List<Socket>();
        List<Thread> dictThread = new List<Thread>();
        public Form1()
        {
            Control.CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

            comboBox1.SelectedIndex = 0;
            radioButton2.Checked = true;
            radioButton5.Checked = true;
            radioButton10.Checked = true;
        }

        private void btnServerConn_Click(object sender, EventArgs e)
        {            
            //定义一个套接字用于监听客户端发来的信息  包含3个参数(IP4寻址协议,流式连接,TCP协议)
            socketWatch = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //服务端发送信息 需要1个IP地址和端口号
            IPAddress ipaddress = IPAddress.Parse(Properties.Settings.Default.Ip); //获取文本框输入的IP地址
            //将IP地址和端口号绑定到网络节点endpoint上 
            IPEndPoint endpoint = new IPEndPoint(ipaddress, Properties.Settings.Default.Port); //获取文本框上输入的端口号
            //监听绑定的网络节点
            socketWatch.Bind(endpoint);
            //将套接字的监听队列长度限制为20
            socketWatch.Listen(20);
            //创建一个监听线程 
            threadWatch = new Thread(WatchConnecting);
            //将窗体线程设置为与后台同步
            threadWatch.IsBackground = true;
            //启动线程
            threadWatch.Start();
            //启动线程后 txtMsg文本框显示相应提示
            textBox2.AppendText("开始监听客户端传来的信息!" + "\r\n");
            button1.Enabled = false;
        }

        /// <summary>
        /// 监听客户端发来的请求
        /// </summary>
        private void WatchConnecting()
        {
            while (true)  //持续不断监听客户端发来的请求
            {
                Socket socConnection = socketWatch.Accept();
                textBox2.AppendText("客户端连接成功" + "\r\n");
                //创建一个通信线程 
                ParameterizedThreadStart pts = new ParameterizedThreadStart(ServerRecMsg);
                Thread thr = new Thread(pts);
                thr.IsBackground = true;
                socConnections.Add(socConnection);
                //启动线程
                thr.Start(socConnection);
                dictThread.Add(thr);

            }
        }

        private void ServerRecMsg(object obj)
        {
            //throw new NotImplementedException();
        }

        //发送信息到客户端
        private void btnSendMsg_Click(object sender, EventArgs e)
        {
            int i = 0;
            string ConMessage = string.Empty;
            string LpnMessage = string.Empty;
            if (radioButton3.Checked)
            {
                ConMessage = "[C|20180910003010|01|0||N|45G1]";
            }
            if (radioButton4.Checked)
            {
                ConMessage = "[C|20020919114100|01|2|||N|N|22G1|22G1]";
            }
            if (radioButton5.Checked)
            {
                ConMessage = "[C|20180910003010|01|0|TEXU7337250|Y|45G1]";
            }
            if (radioButton6.Checked)
            {
                ConMessage = "[C|20020919114100|01|2|MGLU2872320|Y|MGLU2782249|Y|22G1|22G1]";
            }
            if (radioButton7.Checked)
            {
                ConMessage = "[C|20180910003010|01|0|TEXU7337251|N|45G1]";
            }
            if (radioButton8.Checked)
            {
                ConMessage = "[C|20020919114100|01|2|MGLU2872310|N|MGLU4782249|N|22G1|22G1]";
            }
            if (radioButton9.Checked)
            {
                ConMessage = "[C|20020919114100|01|2|MGLU2872320|Y|MGLU2782219|N|22G1|22G1]";
            }
            if(radioButton10.Checked)
            {
                LpnMessage = "[U|20020919114100|01|AB123874|1]";
            }
            if(radioButton11.Checked)
            {
                LpnMessage = "[U|20020919114100|01||1]";
            }

            if (comboBox1.SelectedIndex==comboBox1.Items.Count-1)
            {
               while(true)
                {
                    textBox2.AppendText("Message:" + "\r\n" + i + "\r\n");                  
                    ServerSendMsg(ConMessage);
                    ServerSendMsg(LpnMessage);
                    if (radioButton1.Checked)
                    {
                        Delay(random.Next(int.Parse(textBox3.Text) * 1000, int.Parse(textBox4.Text) * 1000));
                    }
                    if(radioButton2.Checked)
                    {
                        Delay(int.Parse(textBox5.Text) * 1000);
                    }
                    i++;
                }
            }
            else
            {
                while (i < int.Parse(comboBox1.SelectedItem.ToString()))
                {
                    textBox2.AppendText("Message:" + "\r\n" + i + "\r\n");
                    ServerSendMsg(ConMessage);
                    ServerSendMsg(LpnMessage);
                    if (radioButton1.Checked)
                    {
                        Delay(random.Next(int.Parse(textBox3.Text) * 1000, int.Parse(textBox4.Text) * 1000));
                    }
                    if (radioButton2.Checked)
                    {
                        Delay(int.Parse(textBox5.Text) * 1000);
                    }
                    i++;
                }
            }    
        }


        [DllImport("kernel32.dll")]
        static extern uint GetTickCount();
        static void Delay(int ms)
        {
            uint start = GetTickCount();
            while (GetTickCount() - start < ms)
            {
                Application.DoEvents();
            }
        }

        /// <summary>
        /// 发送信息到客户端的方法
        /// </summary>
        /// <param name="sendMsg">发送的字符串信息</param>
        private void ServerSendMsg(string sendMsg)
        {
            //将输入的字符串转换成 机器可以识别的字节数组
            //byte[] arrSendMsg = Encoding.UTF8.GetBytes(sendMsg);
            byte[] arrSendMsg = Encoding.GetEncoding("GB2312").GetBytes(sendMsg);
            //向客户端发送字节数组信息
            foreach (Socket socConnection in socConnections)
            {
                //socConnection.BeginSend(arrSendMsg,0,arrSendMsg.Length, SocketFlags.None, null, null);
                socConnection.Send(arrSendMsg);
            }

            //将发送的字符串信息附加到文本框txtMsg上
            textBox2.AppendText("So-flash:"  + "\r\n" + sendMsg + "\r\n");
            //}

        }
    }
}
