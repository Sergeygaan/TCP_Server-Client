using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TCPServer
{
    public partial class TCPServer : Form
    {
        public TCPServer()
        {
            InitializeComponent();
        }

        Socket sListener;

        List<TCPClientReader> sockets = new List<TCPClientReader>();

        BackgroundWorker backgroundWorker2;

        void backgroundWorker_Accept(object sender, DoWorkEventArgs e)
        {
            while(true)
            {
                Socket handler = sListener.Accept();

                bool flag = false;

                foreach (var currentSocket in sockets)
                {
                    if (handler.RemoteEndPoint == currentSocket.handlerSocket.RemoteEndPoint)
                    {
                        flag = true;

                        break;
                    }
                }

                if (!flag)
                {
                    MessageDelegate message = TextList; // делегат указывает на метод Add
                    WriteClientDelegate writeClientDelegate = WriteClient;
                    sockets.Add(new TCPClientReader(handler, message, WriteClient));
                }

            }

        }

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Устанавливаем для сокета локальную конечную точку
            IPHostEntry ipHost = Dns.GetHostEntry("192.168.0.102");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 11000);

            // Создаем сокет Tcp/Ip
            sListener = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                sListener.Bind(ipEndPoint);
                sListener.Listen(10);


                TextList("Ожидаем соединение через порт " + ipEndPoint);

                backgroundWorker2 = new BackgroundWorker();

                backgroundWorker2.DoWork += backgroundWorker_Accept;
                backgroundWorker2.RunWorkerAsync();

                // Начинаем слушать соединения
                while (true)
                {
                    Thread.Sleep(60);

                    //foreach (var currentSocketRead in sockets.ToArray())
                    //{
                    //    string data = null;

                    //    // Мы дождались клиента, пытающегося с нами соединиться

                    //    byte[] bytes = new byte[1024];
                    //    int bytesRec = currentSocketRead.Receive(bytes);

                    //    data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                    //    // Показываем данные на консоли
                    //    TextList("Полученный текст: " + data);

                    //    foreach (var currentSocketWrite in sockets.ToArray())
                    //    {
                    //        // Отправляем ответ клиенту\
                    //        string reply = data;
                    //        byte[] msg = Encoding.UTF8.GetBytes(reply);
                    //        currentSocketWrite.Send(msg);
                    //    }

                    //    if (data.IndexOf("<TheEnd>") > -1)
                    //    {
                    //        TextList("Сервер завершил соединение с клиентом.");

                    //        break;
                    //    }
                    //}
                    //handler.Shutdown(SocketShutdown.Both);
                    //handler.Close();
                }
            }
            catch (Exception ex)
            {
                TextList(ex.ToString());
            }
            finally
            {

            }
        }

        BackgroundWorker backgroundWorker1;

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1 = new BackgroundWorker();

            backgroundWorker1.DoWork += backgroundWorker_DoWork;
            backgroundWorker1.RunWorkerAsync();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            backgroundWorker1.Dispose();
            backgroundWorker1 = null;

        }

        public delegate void MessageDelegate(string text); // 1. Объявляем делегат

        private void TextList(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => listBox1.Items.Add(text)));
            }
            else
            {
                listBox1.Items.Add(text);
            }
        }

        public delegate void WriteClientDelegate(string text); // 1. Объявляем делегат

        public void WriteClient(string data)
        {
            foreach (var currentSocketWrite in sockets.ToArray())
            {
                // Отправляем ответ клиенту\
                string reply = data;
                byte[] msg = Encoding.UTF8.GetBytes(reply);
                currentSocketWrite.handlerSocket.Send(msg);
            }
        }
    }

    public class TCPClientReader
    {

        public Socket handlerSocket;

        BackgroundWorker backgroundWorker1;
        TCPServer.MessageDelegate _message;
        TCPServer.WriteClientDelegate _writeClientDelegat;

        public TCPClientReader(Socket handler, TCPServer.MessageDelegate message, TCPServer.WriteClientDelegate writeClientDelegate)
        {
            handlerSocket = handler;
            _message = message;
            _writeClientDelegat = writeClientDelegate;

            backgroundWorker1 = new BackgroundWorker();

            backgroundWorker1.DoWork += backgroundWorker_DoWork;
            backgroundWorker1.RunWorkerAsync();
        }

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            // Начинаем слушать соединения
            while (true)
            {
                Thread.Sleep(60);

                string data = null;

                // Мы дождались клиента, пытающегося с нами соединиться

                byte[] bytes = new byte[1024];
                int bytesRec = handlerSocket.Receive(bytes);

                data += Encoding.UTF8.GetString(bytes, 0, bytesRec);

                // Показываем данные на консоли
                _message("Полученный текст: " + data);

                _writeClientDelegat(data);

                if (data.IndexOf("<TheEnd>") > -1)
                {
                    _message("Сервер завершил соединение с клиентом.");

                    break;
                }
                
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }

        }
    }

}
