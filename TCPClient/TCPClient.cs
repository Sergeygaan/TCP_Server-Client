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

namespace TCPClient
{
    public partial class TCPClient : Form
    {
        public TCPClient()
        {
            InitializeComponent();
        }

        BackgroundWorker backgroundWorker1;

        Socket senderSocket;

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

        void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                SendMessageFromSocket(11000);
            }
            catch (Exception ex)
            {
                TextList(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }
        int index = 0;

        void SendMessageFromSocket(int port)
        {
         
            // Устанавливаем удаленную точку для сокета
            IPHostEntry ipHost = Dns.GetHostEntry("192.168.0.102");
            IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, port);

            senderSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Соединяем сокет с удаленной точкой
            senderSocket.Connect(ipEndPoint);


            TextList("Введите сообщение: ");

            TCPClientConnect(senderSocket);
        }

        private void TCPClientConnect(Socket sender)
        {
            // Буфер для входящих данных
            byte[] bytes = new byte[1024];

            Thread.Sleep(100);

            // Отправляем данные через сокет
           // int bytesSent = sender.Send(msg);

            // Получаем ответ от сервера
            int bytesRec = sender.Receive(bytes);

            TextList("\nОтвет от сервера:" + Encoding.UTF8.GetString(bytes, 0, bytesRec));

            // Используем рекурсию для неоднократного вызова SendMessageFromSocket()
          
                TCPClientConnect(sender);

            // Освобождаем сокет
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string message = textBox1.Text;

            byte[] msg = Encoding.UTF8.GetBytes(message);

            // Отправляем данные через сокет
            int bytesSent = senderSocket.Send(msg);
        }
    }
}
