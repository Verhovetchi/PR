using SimpleTCP;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
     public partial class Client : Form
     {
          SimpleTcpClient tcpClient;
          public Client()
          {
               InitializeComponent();
          }

          private void btnConnect_Click(object sender, EventArgs e)
          {
               btnConnect.Enabled = false;
               tcpClient = new SimpleTcpClient().Connect(txtHost.Text, Convert.ToInt32(txtPort.Text));
               btnSend.Enabled = true;
          }

          private void btnSend_Click(object sender, EventArgs e)
          {
               string myMessage;
               myMessage = tcpClient.WriteLineAndGetReply(txtMessage.Text, TimeSpan.FromSeconds(3)).MessageString;
               myMessage = myMessage.Substring(0, myMessage.Length - 1);
               txtStatus.Text += myMessage + Environment.NewLine;
          }
     }
}
