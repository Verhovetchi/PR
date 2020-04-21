using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SimpleTCP;

namespace TCPIP_Client_Server
{
     public partial class Server : Form
     {
          SimpleTcpServer server;
          public Server()
          {
               InitializeComponent();
          }

          private void Server_Load(object sender, EventArgs e)
          {
               server = new SimpleTcpServer();
               server.Delimiter = 0x13;
               server.StringEncoder = Encoding.UTF8;
               server.DataReceived += Server_DataReceived;
          }

          private void Server_DataReceived(object sender, SimpleTCP.Message e)
          {
               txtStatus.Invoke((MethodInvoker)delegate ()
               {
                    string myMessage = e.MessageString.Substring(0, e.MessageString.Length - 1);
                    txtStatus.Text += myMessage + Environment.NewLine;
                    e.ReplyLine(string.Format($"{DateTime.Now.ToString("H:mm")}:" + myMessage));
               });
          }

          private void btnStart_Click(object sender, EventArgs e)
          {
               txtStatus.Text += "Server Starting ..." + Environment.NewLine;
               IPAddress ip = IPAddress.Parse(txtHost.Text);
               server.Start(ip, Convert.ToInt32(txtPort.Text));
          }

          private void btnStop_Click(object sender, EventArgs e)
          {
               if (server.IsStarted)
                    server.Stop();
               txtStatus.Text += "Server Stopped";
          }
     }
}
