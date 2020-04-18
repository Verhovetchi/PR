using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace HTTP_Test
{
     class program
     {
          static void Main()
          {
               string result = GetResponse();
               
               FindTeams(result);

               HttpClientRequest(HttpMethod.Get, "Get");
               HttpClientRequest(HttpMethod.Post, "Post");
               HttpClientRequest(HttpMethod.Options, "Options");
               HttpClientRequest(HttpMethod.Head, "Head");

               Console.ReadKey();
          }

          static void FindTeams(string result)
          {
               List<string> teams = new List<string>();

               var str = Regex.Split(result, "\n");
               Regex regex = new Regex(@"joomsport/team/21-/[^ ]*");
               Match match;
               int i = 0;
               foreach (var item in str)
               {
                    match = regex.Match(str[i]);
                    i++;
                    if (match.Success)
                         teams.Add(match.Value);
               }

               teams.Sort();

               List<string> team = new List<string>();
               
               string[] Item;
               for(int j = 0; j < teams.Count; j++)
               {
                    Item = teams[j].Split('<', '>');
                    team.Add(Item[1]);
           
               }
               Console.WriteLine("Teams:");
               var noDupes = team.Distinct().ToList();
               noDupes = noDupes.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();

               for (int j = 0; j < noDupes.Count; j++)
               {
                    Console.WriteLine(noDupes[j]);
               }
               
          }

          public static string GetResponse()
          {
               HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create("https://barcaman.ru/");
               request.Method = "GET";
               String test;
               using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
               {
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    test = reader.ReadToEnd();
                    reader.Close();
                    dataStream.Close();
               }

               return test;
          }

          static async void HTTP_GET()
          {
               var TARGETURL = "https://barcaman.ru/";

               HttpClientHandler handler = new HttpClientHandler()
               {
                    Proxy = new WebProxy("http://51.38.71.101:8080"),
                    UseProxy = true,
               };

               Console.WriteLine("GET: + " + TARGETURL);

               // ... Use HttpClient.            
               HttpClient client = new HttpClient(handler);

               try
               {
                    HttpResponseMessage response = await client.GetAsync(TARGETURL);

                    HttpContent content = response.Content;

                    // ... Check Status Code                                
                    Console.WriteLine("Response StatusCode: " + (int)response.StatusCode);

                    // ... Read the string.
                    string result = await content.ReadAsStringAsync();

                    // ... Display the result.
                    //if (result != null && result.Length >= 50)
                    //{
                    //     Console.WriteLine(result.Substring(0, 350) + "...");
                    //}

                    Console.WriteLine(result);
               }
               catch(Exception e)
               {
                    Console.WriteLine(e.Message);
               }
               
          }

          static WebProxy getWebProxy()
          {
               return new WebProxy("http://51.38.71.101:8080");
          }

          static void HttpClientRequest(HttpMethod httpMethod, string method)
          {
               Console.WriteLine();
               WebProxy proxy = getWebProxy();
               var _handler = new HttpClientHandler { Proxy = proxy };
               _handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true;

               HttpClient httpClient = new HttpClient(_handler);
              
               Uri BaseUri = new Uri("https://barcaman.ru/");
               
               var result = httpClient.SendAsync(new HttpRequestMessage(httpMethod, BaseUri));
               result.Wait();
               
               Console.WriteLine(method);
               Console.WriteLine(result.Result);
               Console.WriteLine($"{method}");
               Console.WriteLine();
          }

     }
}