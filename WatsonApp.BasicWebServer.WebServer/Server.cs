using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace WatsonApp.BasicWebServer.WebServer;

public static class Server
{
    private static HttpListener listener;
    private static int maxSimultaneousConnections = 20;
    private static Semaphore semaphore = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);

    public static void Start()
    {
        List<IPAddress> localHostIPs = GetLocalHostIPs();
        HttpListener listener = InitializeListener(localHostIPs);
        Start(listener);
    }

    private static List<IPAddress> GetLocalHostIPs()
    {
        IPHostEntry host;
        host = Dns.GetHostEntry(Dns.GetHostName());
        List<IPAddress> addresses = host.AddressList
            .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
            .ToList();

        return addresses;
    }

    private static HttpListener InitializeListener(List<IPAddress> addresses)
    {
        HttpListener listener = new HttpListener();
        listener.Prefixes.Add("http://+:80/WatsonApp.BasicWebServer/");

        addresses.ForEach(ip =>
        {
            string address = $"http://{ip}/";
            Console.WriteLine($"Listening on IP {address}");
            listener.Prefixes.Add(address);
        });

        return listener;
    }

    private static void Start(HttpListener listener)
    {
        listener.Start();
        Task.Run(() => RunServer(listener));
    }

    private static void RunServer(HttpListener listener)
    {
        while (true)
        {
            semaphore.WaitOne();
            StartConnectionListener(listener);
        }
    }

    private static async void StartConnectionListener(HttpListener listener)
    {
        HttpListenerContext context = await listener.GetContextAsync();
        semaphore.Release();

        string response = "Hello Browser!";
        byte[] encoded = Encoding.UTF8.GetBytes(response);
        context.Response.ContentLength64 = encoded.Length;
        context.Response.OutputStream.Write(encoded, 0, encoded.Length);
        context.Response.OutputStream.Close();
    }
}