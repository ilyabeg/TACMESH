using System.Net;
using System.Net.Sockets;
using TacMesh.Core;
using TacMesh.Core.builders;
using TacMesh.Core.communication.simulation_communication;
using TacMesh.Core.socket_related;

namespace TacMesh.Agent
{
    public class Program
    {
        // arguments length should be exactly 2: NodeID, config file path
        const int arguments_length = 2;

        static void Main(string[] args)
        {
            if (args.Length != arguments_length)
            {
                Console.WriteLine("ERROR: Agent Process arguments did not match. Terminating process...");
                return;
            }

            Node node = new Node(new SimTransport(GetSocket()), args[0]);
        }

        static Socket GetSocket()
        {
            // create and bind socket to a random free port assigned by the OS and the localhoast ip
            Socket s = SocketGenerator.GenerateUdpSocket();
            SocketGenerator.BindSocket(s, IPAddress.Loopback);
            return s;
        }
    }
}
