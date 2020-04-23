using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace UDP
{
    public partial class ClientForm : Form
    {
          UdpClient client;
          IPEndPoint endPoint;

          public ClientForm()
          {
               InitializeComponent();
          }

          private void btnConnect_Click(object sender, EventArgs e)
          {
               int clientPort = int.Parse(txtClientPort.Text);
               client = new UdpClient(clientPort);

               btnConnect.Enabled = txtClientPort.Enabled = txtHostName.Enabled = txtServerPort.Enabled = false;
               btnSend.Enabled = true;
          }

          private void btnSend_Click(object sender, EventArgs e)
          {
               int serverPort = int.Parse(txtServerPort.Text);
               int clientPort = int.Parse(txtClientPort.Text);
               string hostName = txtHostName.Text;

               string msg = string.Format("{0}.{1}.{2}", clientPort, hostName, txtMsg.Text.Trim());
               byte[] buffer = Encoding.Unicode.GetBytes(msg);

               client.Send(buffer, buffer.Length, hostName, serverPort);

               endPoint = new IPEndPoint(IPAddress.Any, 0);
               buffer = client.Receive(ref endPoint);
               msg = Encoding.Unicode.GetString(buffer);

               WriteLog(msg);
          }

          private void WriteLog(string msg)
          {
               MethodInvoker invoker = new MethodInvoker(delegate
               {
                    txtLog.Text += string.Format("{0}{1}", msg, Environment.NewLine);
                    txtMsg.Text = string.Empty;
               });

               this.BeginInvoke(invoker);
          }
     }
}