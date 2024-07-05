namespace WatsonApp.BasicWebServer.Console;

using System;
using WatsonApp.BasicWebServer.WebServer;

public class Program
{
    static void Main(string[] args)
    {
        Server.Start();
        Console.ReadLine();
    }
}