using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace tcp_server
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            Int32 port = 9900;
            Console.WriteLine("Hit Ctrl-C to exit.");
            Server tcpServer = new Server();
            await tcpServer.StartServer(port);
        }
    }
}
