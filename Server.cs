using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace tcp_server
{
    public class Server
    {
        public static ManualResetEvent resetEvent = new ManualResetEvent(false);

        public Server()
        {

        }
        public static void StartUp(IPAddress iPAddress, int port)
        {
            try
            {
                Socket socket = new Socket(iPAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint endPoint = new IPEndPoint(iPAddress, port);

                socket.Bind(endPoint);
                socket.Listen(100);

                while (true)
                {
                    resetEvent.Reset();

                    socket.BeginAccept(
                        new AsyncCallback(AssignOperands), socket
                    );
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
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

    public class Operands
    {
        public int OperandA { get; set; }

        public int OperandB { get; set; }

        public int Result { get; set; }
    }
}