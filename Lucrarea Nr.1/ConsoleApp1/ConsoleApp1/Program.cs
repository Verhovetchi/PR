using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        public static string ClearExpression(string aux)
        {
            string input = null;
            input += aux;


            string[] tags = { "src", "narrow", "lazy" };
            for (int i = 0; i < tags.Length; i++)
            {
                string pattern = @"val=""(?<url>.*?)""".Replace("val", tags[i]);

                Match m = Regex.Match(input, pattern);
                while (m.Success)
                {
                    string res = m.Value.Substring(tags[i].Length + 1);
                    m = m.NextMatch();
                    if (res.Length > 2)
                    {
                        return res;
                    }
                }
            }
            return null;
        }



        public static string SocketResult()
        {
            string result = null;

            IPHostEntry Host = Dns.GetHostEntry("www.unite.md");
            IPAddress Addr = Host.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(Addr, 80);

            Socket socket = new Socket(Addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endpoint);
            Console.WriteLine(socket.RemoteEndPoint.ToString());
            string get = "GET / HTTP/1.1\r\n";
            string host = "HOST: www.unite.md\r\n";
            string accept = "ACCEPT: *\r\n\r\n";
            byte[] sendbyte = Encoding.ASCII.GetBytes(get + host + accept);
            socket.Send(sendbyte);
            byte[] response = new byte[socket.ReceiveBufferSize];

            while (socket.Receive(response) != 0)
            {
                result = Encoding.ASCII.GetString(response);
            }

            socket.Close();
            return result;
        }


        static int iImage = 1;
        public static List<string> FindByRegularExpression()
        {
            List<string> imageAddresses = new List<string>();
            Regex regex = new Regex("<img.*?src=\"(.*?)\"[^>]+>", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            using (StringReader reader = new StringReader(SocketResult()))
            {
                string line = string.Empty;
                do
                {
                    line = reader.ReadLine();
                    if (line != null)
                    {
                        Match match = regex.Match(line);
                        if (match.Value.Length > 0)
                        {
                            string s1 = match.ToString();
                            string s2 = ClearExpression(s1);
                            imageAddresses.Add(s2);
                        }
                    }

                } while (line != null);
            }

            return imageAddresses;
        }


        public static void CallSocket(string temp1)
        {
            IPHostEntry Host = Dns.GetHostEntry("unite.md");
            IPAddress Addr = Host.AddressList[0];
            IPEndPoint endpoint = new IPEndPoint(Addr, 80);

            Socket socket = new Socket(Addr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endpoint);
            string ResponseToConvert = null;
            int SocketResponseSizeInBytes = 0;
            byte[] SocketBytesResponse;
            string temp = temp1.Substring(1, temp1.Length - 2);

            var GetReq = $"GET {temp} HTTP/1.1\r\nHost: unite.md\r\nConnection: " +
                    $"keep-alive\r\nAccept: text/html\r\n\r\n";


            socket.Send(Encoding.UTF8.GetBytes(GetReq));
            SocketBytesResponse = new byte[socket.ReceiveBufferSize];
            SocketResponseSizeInBytes = socket.Receive(SocketBytesResponse);
            TryToWrite();
            socket.Close();


            void TryToWrite()
            {
                for (int i = 0; i < SocketResponseSizeInBytes; i++)
                {
                    ResponseToConvert += $"{Convert.ToChar(SocketBytesResponse[i]).ToString()}";
                }

                var index = ResponseToConvert.IndexOf("\r\n\r\n");
                ResponseToConvert = ResponseToConvert.Trim();
                string path = $@"C:\Users\Daniel\Desktop\Folder nou\{iImage}.jpg";
                iImage++;
                var writer = new BinaryWriter(File.Open(path, FileMode.OpenOrCreate));
                for (int i = index + 4; i < ResponseToConvert.Length; i++)
                {
                    writer.Write((byte)ResponseToConvert[i]);
                }
                writer.Close();
            }
        }

        public static void TryDownloadPhoto()
        {
            List<string> PhotoPaths = FindByRegularExpression();
            Parallel.For(0, PhotoPaths.Count, i => CallSocket(PhotoPaths[i]));
            //Parallel.For(0, new ParallelOptions { MaxDegreeOfParallelism = 4}, i => CallSocket(PhotoPaths[i]));
        }



        static void Main(string[] args)
        {

            TryDownloadPhoto();

            Console.ReadLine();
        }

    }
}



