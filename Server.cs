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
                BinaryReader reader = new BinaryReader(networkStream, Encoding.ASCII, true);
                Operands operands = new Operands();

                var buffer = new byte[256];

                while (reader.PeekChar() == -1)
                {
                    byte startOfFile = reader.ReadByte();
                    short messageSize = reader.ReadInt16();
                    byte[] messageBytes = reader.ReadBytes((int)messageSize);
                    string message = Encoding.ASCII.GetString(messageBytes);
                    byte endOfFile = reader.ReadByte();

                    string response = AssignOperands(message, operands);
                    byte[] responseBytes = Encoding.ASCII.GetBytes(response);

                    writer.Write((byte)1);
                    writer.Write((short)Encoding.ASCII.GetByteCount(response));
                    writer.Write(responseBytes);
                    writer.Write(byte.MaxValue);
                }

                Array.Clear(buffer, 0, buffer.Length);
            }
        }
        private static string AssignOperands(string operand, Operands operands)
        {
            int result = 0;
            string[] split = null;
            if (operand.StartsWith("SETOPA:"))
            {
                split = operand.Split(':');
                operands.OperandA = Int16.Parse(split[1]);
            }
            else if (operand.StartsWith("SETOPB:"))
            {
                split = operand.Split(':');
                operands.OperandB = Int16.Parse(split[1]);
            }
            else if (operand == "ADD")
            {
                result = operands.OperandA + operands.OperandB;
            }
            else if (operand == "SUB")
            {
                result = operands.OperandA - operands.OperandB;
            }
            else if (operand == "MULT")
            {
                result = operands.OperandA * operands.OperandB;
            }
            return result.ToString();
        }
    }
}