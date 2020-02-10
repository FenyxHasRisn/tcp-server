using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace tcp_server
{
    public class Program
    {
        public static void Main()
        {
            TcpListener server = null;
            try
            {
                Int32 port = 9900;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                Operands operands = new Operands();

                // TcpListener server = new TcpListener(port);
                server = new TcpListener(localAddr, port);

                // Start listening for client requests.
                server.Start();

                // Buffer for reading data
                Byte[] bytes = new Byte[256];
                String data = null;

                // Enter the listening loop.
                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    // Perform a blocking call to accept requests.
                    // You could also user server.AcceptSocket() here.
                    TcpClient client = server.AcceptTcpClient();
                    Console.WriteLine("Connected!");

                    data = null;

                    // Get a stream object for reading and writing
                    NetworkStream stream = client.GetStream();

                    BinaryReader reader = new BinaryReader(stream, Encoding.ASCII, true);
                    BinaryWriter writer = new BinaryWriter(stream, Encoding.ASCII, true);

                    int i;

                    // Loop to receive all the data sent by the client.
                    while ((i = reader.Read(bytes, 0, bytes.Length)) >= 1)
                    {
                        // Translate data bytes to a ASCII string.
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                        Console.WriteLine("Received: {0}", data.Substring(0, data.Length - 1));

                        data = AssignOperands(data.Substring(0, data.Length - 1), operands);

                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);

                        // Send back a response.
                        writer.Write((byte)1);
                        writer.Write((short)Encoding.ASCII.GetByteCount(data));
                        writer.Write(msg);
                        writer.Write(byte.MaxValue);
                        stream.Flush();
                        Console.WriteLine("Sent: {0}", data);
                    }

                    // Shutdown and end connection
                    client.Close();
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
            finally
            {
                // Stop listening for new clients.
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }

        private static string AssignOperands(string operand, Operands operands)
        {
            int result = 0;
            if (operand.StartsWith("SETOPA:"))
            {
                operands.OperandA = Int16.Parse(operand.Substring(operand.Length - 1));
            }
            else if (operand.StartsWith("SETOPB:"))
            {
                operands.OperandB = Int16.Parse(operand.Substring(operand.Length - 1));
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
