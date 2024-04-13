using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.CodeDom.Compiler;

void SendStrToClient(string line,ref NetworkStream stream)
{
    byte[] byteResponseToCLient = Encoding.UTF8.GetBytes(line);
    stream.WriteAsync(byteResponseToCLient, 0, byteResponseToCLient.Length);
}

var tcpListener = new TcpListener(IPAddress.Any, 8888);
var serverStopped = false;
Dictionary<int, string> serverDB = new Dictionary<int, string>();
tcpListener.Start();    // запускаем сервер
Console.WriteLine("The server is running.");
var filedb = new StreamReader("C:\\Users\\depor\\Desktop\\proga\\cSharp\\FileServer_v2\\serverDB.txt");
while (!filedb.EndOfStream)
{
    var line = filedb.ReadLine().Split(' ');
    serverDB.Add(int.Parse(line[0]), line[1]);
}
filedb.Close();


while (!serverStopped)
{
    //if (serverStopped == true) break;
    var tcpClient = await tcpListener.AcceptTcpClientAsync();
    Task.Run(async () => await ProcessClientAsync(tcpClient));
}

tcpListener.Stop();


async Task ProcessClientAsync(TcpClient tcpClient)
{
    Console.WriteLine($"The client {tcpClient.Client.RemoteEndPoint} connected.");

    var stream = tcpClient.GetStream();
    byte[] byteRequestFromClient = new byte[256];
    int bytes = stream.Read(byteRequestFromClient, 0, byteRequestFromClient.Length);
    var requestFromClient = Encoding.UTF8.GetString(byteRequestFromClient, 0, bytes);
    var arrForUnderstandingRequest = requestFromClient.Split(' ');
    string command = arrForUnderstandingRequest[0];
    string filename = "C:\\Users\\depor\\Desktop\\proga\\cSharp\\FileServer_v2\\serverData\\";

    if (command == "EXIT")
    {
        var sw = new StreamWriter("C:\\Users\\depor\\Desktop\\proga\\cSharp\\FileServer_v2\\serverDB.txt");
        foreach (var item in serverDB)
        {
            sw.WriteLine($"{item.Key} {item.Value}");
        }
        Console.WriteLine("dfslkghsferh;jwflrhuinecgwmnpgiprmojnehr;liefmj");
        sw.Close(); 
        serverStopped = true;  
        return;
    }
    switch (command)
    {
        case "GET":
            if (arrForUnderstandingRequest[1] == "1")
            {
                filename += serverDB[int.Parse(arrForUnderstandingRequest[2])];
            }
            else filename += arrForUnderstandingRequest[2];

            if (File.Exists(filename))
            {
                SendStrToClient("201", ref stream);
                byte[] fileBytes = File.ReadAllBytes(filename);
                byte[] f1leSizeBytes = BitConverter.GetBytes(fileBytes.Length);
                stream.Write(f1leSizeBytes, 0, f1leSizeBytes.Length);
                stream.Write(fileBytes, 0, fileBytes.Length);
            }
            else SendStrToClient("401", ref stream);
            break;

        case "PUT":
            filename += arrForUnderstandingRequest[1];
            int rnd = 111;
            while (serverDB.ContainsKey(rnd))
            {
                Random random = new Random();
                rnd = random.Next(100, 1000);
            }
            serverDB.Add(rnd, arrForUnderstandingRequest[1]);
            SendStrToClient($"202~{rnd}", ref stream);

            byte[] fileSizeBytes = new byte[4];
            stream.Read(fileSizeBytes, 0, fileSizeBytes.Length);
            int fileSize = BitConverter.ToInt32(fileSizeBytes, 0);
            byte[] imageData = new byte[fileSize];
            int bytesRead = stream.Read(imageData, 0, fileSize);
            File.WriteAllBytes(filename, imageData);

            break;

        case "DELETE":
            if (arrForUnderstandingRequest[1] == "1")
            {
                filename += serverDB[int.Parse(arrForUnderstandingRequest[2])];
            }
            else filename += arrForUnderstandingRequest[2];
            if (File.Exists(filename))
            {
                try
                {
                    File.Delete(filename);
                    Console.WriteLine("file is delete\n");
                    SendStrToClient("203", ref stream);
                }
                catch (Exception)
                {
                    Console.WriteLine("ERROR 404\n");
                    SendStrToClient("403", ref stream);
                }
            }
            else
            {
                Console.WriteLine("ERROR 403\n");
                SendStrToClient("403", ref stream);
            }
            break;

        case "SHOW":
            var resp = "";
            foreach (var item in serverDB)
            {
                resp += $"{item.Key}\t{item.Value}\n";
            }
            SendStrToClient($"204~{resp}", ref stream);
            break;        
        default:
            break;
    }
    Console.WriteLine($"Response to client was send\n");
    tcpClient.Close();
}