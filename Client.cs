using System.Collections.Generic;
using System;
using System.Net.Sockets;
using System.Text;
using System.IO;


// буфер для входящих данных

bool clientStopped = false;

while (!clientStopped)
{
    using TcpClient client = new TcpClient();
    await client.ConnectAsync("127.0.0.1", 8888);
    var stream = client.GetStream();
    Console.Write("\nEnter action (1 - get a file, 2 - send a file, 3 - delete a file, 4 - show all file): > ");
    var action = Console.ReadLine();
    while (action != "1" && action != "2" && action != "3" && action != "4" && action != "exit")
    {
        Console.Write("Wrong command. Repeat: > ");
        action = Console.ReadLine();
    }

    var file_IDorNAME = "";
    var filenameInClientData = "";
    var newFilenameForServer = "";
    var byIDorNAME = 0;



    if (action == "1" || action == "3")
    {
        Console.Write("Do you want to get the file by ID(1) or by name(2): > ");
        byIDorNAME = int.Parse(Console.ReadLine());  // 1 - ID; 2 - name
        while (byIDorNAME != 1 && byIDorNAME != 2)
        {
            Console.Write("Input \"1\" or \"2\": > ");
            byIDorNAME = int.Parse(Console.ReadLine());
        }
        string bufflineforconcole = "file ID";
        if (byIDorNAME == 2) bufflineforconcole = "file name";
        Console.Write($"Enter {bufflineforconcole}: > ");
        file_IDorNAME = Console.ReadLine();
    }

    if (action == "2")
    {
        Console.Write("Enter name of the file: > ");
        filenameInClientData = "C:\\Users\\depor\\Desktop\\" + Console.ReadLine();

        Console.Write("Enter name of the file to be saved on server: > ");
        newFilenameForServer = Console.ReadLine();
        if (newFilenameForServer == "") newFilenameForServer = filenameInClientData;
    }


    var requestToServer = "";
    switch (action)
    {
        case "1":
            requestToServer = $"GET {byIDorNAME} {file_IDorNAME}";
            break;
        case "2":
            requestToServer = $"PUT {newFilenameForServer}";
            break;
        case "3":
            requestToServer = $"DEL {file_IDorNAME} {newFilenameForServer}";
            break;
        case "4":
            requestToServer = "SHOW ";
            break;
        case "exit":
            requestToServer = "EXIT ";
            break;
        default:
            break;
    }

    byte[] byteRequest = Encoding.UTF8.GetBytes(requestToServer);
    await stream.WriteAsync(byteRequest, 0, byteRequest.Length);
    Console.WriteLine("The request was sent.");

    if (action == "exit")
    {
        Console.WriteLine("Serever stopped");
        break;
    }

    byte[] byterespFromServer = new byte[254];
    int bytes = stream.Read(byterespFromServer, 0, byterespFromServer.Length);
    var arrRespFromServer = Encoding.UTF8.GetString(byterespFromServer, 0, bytes).Split('~');
    var codeResp = int.Parse(arrRespFromServer[0]);

    switch (codeResp)
    {
        case 201:
            byte[] fileSizeBytes = new byte[4];
            stream.Read(fileSizeBytes, 0, fileSizeBytes.Length);
            int fileSize = BitConverter.ToInt32(fileSizeBytes, 0);

            byte[] imageData = new byte[fileSize];
            int bytesRead = stream.Read(imageData, 0, fileSize);
            Console.Write("The file was downloaded! Specify a name for it: > ");
            string filePath = "C:\\Users\\depor\\Desktop\\" + Console.ReadLine();
            File.WriteAllBytes(filePath, imageData);
            Console.WriteLine("File saved on the hard drive!");
            break;
        case 401:
            Console.WriteLine("The response says that this file is not found!");
            break;
        case 202:
            Console.Write("Response says that file is saved!");

            byte[] fileBytes = File.ReadAllBytes(filenameInClientData);
            byte[] f1leSizeBytes = BitConverter.GetBytes(fileBytes.Length);
            stream.Write(f1leSizeBytes, 0, f1leSizeBytes.Length);
            stream.Write(fileBytes, 0, fileBytes.Length);

            Console.WriteLine($"ID = {arrRespFromServer[1]}");

            break;
        case 402:
            Console.Write("Response says that file is NOT saved!");
            break;
        case 203:
            Console.Write("The response says that this file was deleted successfully!");
            break;
        case 403:
            Console.WriteLine("The response says that this file is not found!");
            break;
        case 204:
            Console.WriteLine(arrRespFromServer[1]);
            break;
        case 404:
            Console.WriteLine("ERROR");
            break;
        default:
            break;
    }
}