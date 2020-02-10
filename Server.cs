using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;

namespace tcp_server
{
    public class Server
    {
        object _lock = new Object();

        List<Task> _connections = new List<Task>();

        public async Task StartServer(int portNumber)
        {
            var tcpListener = TcpListener.Create(portNumber);
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

        private async Task HandleConnectionAsync(TcpClient tcpClient)
        {
            await Task.Yield();

            using (var networkStream = tcpClient.GetStream())
            {

                BinaryWriter writer = new BinaryWriter(networkStream, Encoding.ASCII, true);
                Operands operands = new Operands();

                var buffer = new byte[256];
                int byteCount;
                string request;

                Console.WriteLine("[Server] Reading from client");
                while ((byteCount = networkStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    request = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    Console.WriteLine("[Server] Client wrote {0}", request);

                    request = AssignOperands(request.Substring(0, request.Length - 1), operands);

                    byte[] msg = System.Text.Encoding.ASCII.GetBytes(request);

                    writer.Write((byte)1);
                    writer.Write((short)Encoding.ASCII.GetByteCount(request));
                    writer.Write(msg);
                    writer.Write(byte.MaxValue);
                    Console.WriteLine("[Server] Response was: {0}", request);
                }
            }
        }
        private static string AssignOperands(string operand, Operands operands)
        {
            int result = 0;
            if (operand.Contains("SETOPA:"))
            {
                operands.OperandA = Int16.Parse(operand.Substring(operand.Length - 1));
            }
            else if (operand.Contains("SETOPB:"))
            {
                operands.OperandB = Int16.Parse(operand.Substring(operand.Length - 1));
            }
            else if (operand.Contains("ADD"))
            {
                result = operands.OperandA + operands.OperandB;
            }
            else if (operand.Contains("SUB"))
            {
                result = operands.OperandA - operands.OperandB;
            }
            else if (operand.Contains("MULT"))
            {
                result = operands.OperandA * operands.OperandB;
            }
            return result.ToString();
        }
    }
}