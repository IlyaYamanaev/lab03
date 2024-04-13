using System;
using System.Diagnostics.Tracing;
using System.Net.Sockets;
using System.Resources;
using System.Text;

namespace file_server_emulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                TcpClient client = new TcpClient("127.0.0.1", 8888);
                NetworkStream stream = client.GetStream();

                Console.Write("\nEnter action (1 - get a file, 2 - create a file, 3 - delete a file, 4 - show all file):  > ");
                var action = Console.ReadLine();
                while (action != "1" && action != "2" && action != "3" && action != "exit")
                {
                    Console.Write("Wrong command. Repeat:  > ");
                    action = Console.ReadLine();
                }
               
                var filename = "";
                var content = "";

                if (action != "4" && action != "exit")
                {
                    Console.Write("Enter filename: > ");
                    filename = Console.ReadLine();
                }
                if (action == "2")
                {
                    Console.Write("Enter file content: \n> ");
                    content = Console.ReadLine();
                }
                

                var request = "";
                if (action == "1")
                {
                    request += $"GET {filename}";
                }
                if (action == "2")
                {
                    request += $"PUT {filename} {content}";
                }
                if (action == "3")
                {
                    request += $"DELETE {filename}";
                }
                if (action == "4")
                {
                    request += "SHOW";
                }
                if (action == "exit")
                {
                    request = "EXIT";
                }

                byte[] data = Encoding.UTF8.GetBytes(request);
                stream.Write(data, 0, data.Length);
                Console.WriteLine("The request was sent.");
                if (action == "exit")
                {
                    Console.WriteLine("The server was shut down.");
                    break;
                }

                byte[] biteResponseFromServer = new byte[512];
                int bytes = stream.Read(biteResponseFromServer, 0, biteResponseFromServer.Length);
                string buffResp = Encoding.UTF8.GetString(biteResponseFromServer, 0, bytes);
                string[] buffArrResp = buffResp.Split('~');
                string respFromServer = buffArrResp[0];
                string contentOfRespFromServer = "";
                try
                {
                    contentOfRespFromServer = buffArrResp[1];
                }
                catch (Exception) {}

                if (respFromServer == "")
                {
                    Console.WriteLine("No response from the server!");
                }
                //
                if (respFromServer == "200")
                {
                    Console.WriteLine("The response says that the file was created!");
                }
                if (respFromServer == "400")
                {
                    Console.WriteLine("ERROR 400\n" +
                        "The response says that creating the file was forbidden!");
                }
                //
                if (respFromServer == "202")
                {
                    Console.WriteLine("The content of this file is:\n" + contentOfRespFromServer );
                }
                if (respFromServer == "402")
                {
                    Console.WriteLine("ERROR 402\n" +
                        "The response says that the file was not found!");
                }
                //
                if (respFromServer == "203")
                {
                    Console.WriteLine("The response says that the file was successfully deleted!");
                }
                if (respFromServer == "403")
                {
                    Console.WriteLine("ERROR 403\n" +
                        "The response says that the file was not found!");
                }
                //
                if (respFromServer == "204")
                {
                    Console.WriteLine("All files on server:\n" + contentOfRespFromServer );
                }
                //
                if (respFromServer == "404")
                {
                    Console.WriteLine("ERROR 404\n" +
                        "Something went wrong!");
                }

                stream.Close();
                client.Close();
            }
        }
    }
}

/*
 400 - файл не удалось создать потому что такой уже был 
 402 - не получилось открыть файл, такого не было 
 404 - что не так при создании
 
 
 
 200 - файл успешно создан 
 202 - файл успешно прочитан
 204 - список файлов усепшно отправлен 
 
 
 
 
 
 */