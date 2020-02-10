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
        // public static void Main()
        // {
        //     Int32 port = 9900;
        //     IPAddress iPAddress = IPAddress.Parse("127.0.0.1");

        //     Server server = new Server();

        //     server.StartUp(iPAddress, port);
        // }

        object _lock = new Object();
        List<Task> _connections = new List<Task>();

        private async Task StartListener()
        {
            var tcpListener = TcpListener.Create(9900);
            tcpListener.Start();
            while (true)
            {
                var tcpClient = await tcpListener.AcceptTcpClientAsync();
                Console.WriteLine("[Server] Client has connected");
                var task = StartHandleConnectionAsync(tcpClient);
                if (task.IsFaulted)
                    await task;
            }
        }

        private async Task StartHandleConnectionAsync(TcpClient tcpClient)
        {
            var connectionTask = HandleConnectionAsync(tcpClient);

            lock (_lock)
                _connections.Add(connectionTask);
            try
            {
                await connectionTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                lock (_lock)
                    _connections.Remove(connectionTask);
            }
        }

        private static Task HandleConnectionAsync(TcpClient tcpClient)
        {
            return Task.Run(async () =>
            {
                using (var networkStream = tcpClient.GetStream())
                {
                    var buffer = new byte[4096];
                    Console.WriteLine("[Server] Reading from client");
                    var byteCount = await networkStream.ReadAsync(buffer, 0, buffer.Length);
                    var request = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    Console.WriteLine("[Server] Client wrote {0}", request);
                    var serverResponseBytes = Encoding.UTF8.GetBytes("Hello from server");
                    await networkStream.WriteAsync(serverResponseBytes, 0, serverResponseBytes.Length);
                    Console.WriteLine("[Server] Response has been written");
                }
            });
        }

        static async Task Main(string[] args)
        {
            Int32 port = 9900;
            Console.WriteLine("Hit Ctrl-C to exit.");
            Server tcpServer = new Server();
            await tcpServer.StartServer(port);
        }
    }
}
