using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Czat
{
    public partial class Form1 : Form
    {
        private string nickname;
        private string tempNickname;

        private const string nicknameCheckMsg = "Trwa sprawdzanie, czy twój nick nie jest zajęty...";
        private const string nicknameBusyMsg = "Użytkownik o podanym nicku już istnieje! Podaj inny nick";

        private UdpClient client;
        //private IPEndPoint multiEP = new IPEndPoint(IPAddress.Parse("224.100.0.1"), 8200);
        private IPAddress multiGroup;
        private IPEndPoint multiEP;
        private StringBuilder messages;
        private bool isListenerAlive = true;
        private bool isSenderAlive = true;
        private byte[] messageToSend;
        //private delegate void Display(String mess);

        public Form1()
        {
            InitializeComponent();
            richTextBox1.ReadOnly = true;
            richTextBox1.BackColor = Color.White;
            textBox1.Select();
            client = new UdpClient();
            client.Client.ReceiveTimeout = 10000;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.ExclusiveAddressUse = false;
            client.Client.Bind(new IPEndPoint(IPAddress.Any, 8200));
            multiGroup = IPAddress.Parse("224.0.1.1");
            client.JoinMulticastGroup(multiGroup);
            multiEP = new IPEndPoint(IPAddress.Parse("224.0.1.1"), 8200);
            messages = new StringBuilder();
            nickname = "";
            ShowDialogBox(null);
            textBox1.KeyDown += textBox_KeyDown;
        }

        private void textBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                messageToSend = Encoding.ASCII.GetBytes("MSG " + nickname + ": " + textBox1.Text);
                textBox1.Clear();
            }
        }

        public void ShowDialogBox(string text)
        {
            DialogBox testDialog;
            if (text==null)
                testDialog = new DialogBox();
            else
                testDialog = new DialogBox(text);

            if (testDialog.ShowDialog(this) == DialogResult.OK)
            {
                tempNickname = testDialog.textBox1.Text;
                RunListeningThread();
                Connect();
                RunSendingThread();
            }
            else
            {
                isListenerAlive = false;
                Environment.Exit(1);
            }
            testDialog.Dispose();
        }

        private void Connect()
        {
            messageToSend = Encoding.ASCII.GetBytes("NICK " + tempNickname);
        }

        private void RunListeningThread()
        {
            Thread listeningThread = new Thread(Listen);
            Task.Factory.StartNew(() =>
            {
                Invoke(new Action(() => BlockTextBox()));
                Thread.Sleep(10000);
                listeningThread.Start();
            });
            
        }

        private void BlockTextBox()
        {
            textBox1.Enabled = false;
            textBox1.Text = nicknameCheckMsg;
        }

        private void UnlockTextBox()
        {
            textBox1.Enabled = true;
            textBox1.Text = "";
        }

        private void RunSendingThread()
        {
            Thread sendingThread = new Thread(Send);
            sendingThread.Start();
        }

        private void Listen()
        {
            long i = 0;
            String message;
            bool isNickTaken = false;
            bool isFirstTime = true;
            while (isListenerAlive)
            {
                IPEndPoint ep = null;
                while (client.Available > 0)
                {
                    message = Encoding.ASCII.GetString(client.Receive(ref ep));
                    //Console.WriteLine(message);
                    if (nickname.Equals("") && message.Equals("NICK " + tempNickname + " BUSY"))
                    {
                        //ShowDialogBox();
                        isNickTaken = true;
                        Invoke(new Action(() => ShowDialogBox(nicknameBusyMsg)));
                    }
                    else if (message.Substring(0, 5).Equals("NICK ") && message.Substring(5, message.Length - 5).Equals(nickname))
                    {
                        messageToSend = Encoding.ASCII.GetBytes("NICK " + nickname + " BUSY");
                        //client.Send(reply, reply.Length, multiEP);
                    }
                    else if(message.Substring(0, 4).Equals("MSG "))
                    {
                        //Display dis = new Display(AddMessage);
                        //Console.WriteLine(message);
                        messages.Append(DateTime.Now.ToString("[HH:mm:ss] ") + message.Substring(4) + "\n");
                        richTextBox1.Invoke(new Action(() => richTextBox1.Text = messages.ToString()));
                        //dis.Invoke(message);
                        //richTextBox1.Invoke
                        //Invoke(new Action(() => richTextBox1.Text = message));
                    }
                        //richTextBox1.Text = DateTime.Now.ToString("HH:mm:ss") + " " + message;
                        //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " " + message);
                }
                if (!isNickTaken)
                {
                    nickname = tempNickname;
                }
                else
                {
                    Thread.Sleep(10000);
                    continue;
                }
                if (isFirstTime && !nickname.Equals(""))
                {
                    isFirstTime = false;
                    Invoke(new Action(() => UnlockTextBox()));
                }
                isNickTaken = false;
                //Console.WriteLine(nickname);
                //tempNickname = "";
                Thread.Sleep(250);
            }
        }

        private void Send()
        {
            while (isSenderAlive)
            {
                if (messageToSend != null)
                {
                    client.Send(messageToSend, messageToSend.Length, multiEP);
                    messageToSend = null;
                }
                Thread.Sleep(100);
            }
        }
    }
}
