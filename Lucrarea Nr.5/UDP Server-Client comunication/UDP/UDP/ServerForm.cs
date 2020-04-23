using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace UDP
{
    public partial class ServerForm : Form
    {
          UdpClient server;
          IPEndPoint endPoint;

          public ServerForm()
          {
               InitializeComponent();
          }

          private void btnStartServer_Click(object sender, EventArgs e)
          {
               server = new UdpClient(int.Parse(txtServerPort.Text));
               endPoint = new IPEndPoint(IPAddress.Any, 0);

               Thread thread = new Thread(new ThreadStart(ServerStart));
               thread.Start();
               btnStartServer.Enabled = false;
               btnStop.Enabled = true;

               WriteLog("Server Started Successfully...");
          }

          private void ServerStart()
          {
               while (true)
               {
                    byte[] buffer = server.Receive(ref endPoint);

                    string[] msg = Encoding.Unicode.GetString(buffer).Split('.');
                    int clientPort = int.Parse(msg[0]);
                    string clientHostName = msg[1];
                    string request = msg[2];

                    WriteLog(string.Format($"{request}"));

                    string response = string.Format("Date time at Server: {0}.", DateTime.Now.ToLocalTime());
                   
                    buffer = Encoding.Unicode.GetBytes(response);
                    server.Send(buffer, buffer.Length, clientHostName, clientPort);
               }
          }

          private void WriteLog(string msg)
          {
               MethodInvoker invoker = new MethodInvoker(delegate
               {
                    txtLog.Text += string.Format("{0}{1}", msg, Environment.NewLine);
               });

               this.BeginInvoke(invoker);
          }

          private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
          {
               server.Close();
               Application.Exit();
          }

          private void btnStop_Click(object sender, EventArgs e)
          {
               WriteLog("Server stoped");
               btnStop.Enabled = false; 
          }
    }
}
