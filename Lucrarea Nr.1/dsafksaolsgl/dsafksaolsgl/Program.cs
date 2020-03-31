using System;
using System.Text;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Drawing.Imaging;

public class GetSocket
{
     public static void Main(string[] args)
     {
          string url = "http://unite.md/", result = "", headers;
          int port = 80;
          Uri uri = new Uri(url);
          string host = uri.Authority;
          string path = uri.Scheme + "://" + uri.Authority + uri.AbsolutePath;

          Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
          socket.Connect(host, port);
          if (socket.Connected)
          {
               headers =
               "GET " + path + " HTTP/1.1\r\n" +
               "Host: " + host + "\r\n" +
               "User-Agent: Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/54.0.2840.71 Safari/537.36\r\n" +
               "Connection: Close\r\n" +
               "Accept: */*\r\n" +
               "Accept-Language: ru-RU,ru;q=0.9,en;q=0.8\r\n\r\n";

               Byte[] bytesSent = Encoding.UTF8.GetBytes(headers);
               Byte[] bytesReceived = new Byte[1024];

               socket.Send(bytesSent, bytesSent.Length, 0);
               int bytes = 0;
          Label1:
               bytes = socket.Receive(bytesReceived, bytesReceived.Length, 0);
               result += Encoding.UTF8.GetString(bytesReceived, 0, bytes);
               if (bytes > 0)
               {
                    goto Label1;
               }
               

               //Console.WriteLine(result);
          }

          var str = Regex.Split(result, "\n");
          Regex regex = new Regex(@"/images/[^ ]*(.jpg|.png|.gif)+");
          Match match;
          int i = 0, j = 0;
          string urnImg;
          foreach (var item in str)
          {
               urnImg = "http://unite.md";
               match = regex.Match(str[i]);
               i++;
               if (match.Success)
               {
                    j++;
                    urnImg = urnImg + match.Value;
                    string name = j.ToString();
                    name = name + ".png";
                    
                    SaveImage(name, ImageFormat.Png, urnImg);
               }
          }

          Console.WriteLine("Download finish");

          socket.Close();
          socket.Dispose();


          string a = Console.ReadLine();
     }

     public static void SaveImage(string filename, ImageFormat format, string str)
     {
          WebClient client = new WebClient();
          Stream stream = client.OpenRead(str);
          Bitmap bitmap; 
          bitmap = new Bitmap(stream);

          if (bitmap != null)
          {
               bitmap.Save(filename, format);
          }

          stream.Flush();
          stream.Close();
          client.Dispose();
     }
}
