using System;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection.Emit;
using System.Text;

namespace file_server_client
{
    internal class Program
    {
        static void Main(string[] args)
        {

            TcpListener server = new TcpListener(IPAddress.Any, 8888);
            server.Start();
            Console.WriteLine("The server is running.");


            while (true)
            {
                TcpClient client = server.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                Console.WriteLine("client connected to " + IPAddress.Parse(((IPEndPoint)client.Client.RemoteEndPoint).Address.ToString()) + "on port number " + ((IPEndPoint)client.Client.RemoteEndPoint).Port.ToString());
                byte[] data = new byte[256];
                int bytes = stream.Read(data, 0, data.Length);
                string request = Encoding.UTF8.GetString(data, 0, bytes);
                //Console.WriteLine(request);
                string[] arrForUnderstandingRequest = request.Split(' ');

                string command = arrForUnderstandingRequest[0];
                string filename = "C:\\Users\\depor\\Desktop\\proga\\cSharp\\file_server_emulator\\fileServer\\";
                string content = "";
               
                try
                {
                    filename += arrForUnderstandingRequest[1];
                }
                catch (Exception) { }

                try
                {
                    content = arrForUnderstandingRequest[2];
                }
                catch (Exception) { }


                string respToClient = "";
                string contentOfResponse = "";

                //PUT
                if (command == "PUT")
                {
                    if (!File.Exists(filename))
                    {
                        try
                        {
                            using (StreamWriter writer = new StreamWriter(filename))
                            {
                                writer.Write(content);
                            }
                            Console.WriteLine("the file has been created.\n");
                            respToClient = "200";
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("something went wrong. ERROR 404\n");
                            respToClient = "404";
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR 400\n");
                        respToClient = "400";
                    }
                }
                //SHOW
                if (command == "SHOW")
                {
                    if (Directory.Exists(filename))
                    {
                        string[] files = Directory.GetFiles(filename);

                        foreach (string file in files)
                        {
                            contentOfResponse += "\t-> " + Path.GetFileName(file) + "\n";
                        }
                        Console.WriteLine("file list is sent\n");
                        respToClient = "204";
                    }
                    else
                    {
                        Console.WriteLine("ERROR 404\n");

                        respToClient = "404";
                    }
                }
                //GET
                if (command == "GET")
                {
                    if (File.Exists(filename))
                    {
                        try
                        {
                            using (var r = new StreamReader(filename))
                            {
                                contentOfResponse = r.ReadToEnd();
                            }
                            Console.WriteLine("file is read\n");
                            respToClient = "202";
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("ERROR 404\n");
                            respToClient = "404";
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR 402\n");
                        respToClient = "402";
                    }
                }
                //DELETE
                if (command == "DELETE")
                {
                    if (File.Exists(filename))
                    {
                        try
                        {
                            File.Delete(filename);
                            Console.WriteLine("file is delete\n");
                            respToClient = "203";
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("ERROR 404\n");
                            respToClient = "404";
                        }
                    }
                    else
                    {
                        Console.WriteLine("ERROR 403\n");
                        respToClient = "403";
                    }                    
                }
                //EXIT
                if (command == "EXIT")
                {
                    server.Stop();                    
                }

                respToClient += "~" + contentOfResponse;
                byte[] response = Encoding.UTF8.GetBytes(respToClient);
                stream.Write(response, 0, response.Length);

                client.Close();
            }            
                server.Stop();            
        }
    }
}