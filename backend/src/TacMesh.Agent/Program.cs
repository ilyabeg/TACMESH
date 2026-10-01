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
        static void Main(string[] args)
        {
            //if (args.Length != 2)
            //{
            //    Console.WriteLine("ERROR: Agent Process arguments did not match. Terminating process...");
            //    return;
            //}

            int role = GetRole();
            Node node = new Node(new SimTransport(GetSocket()), "TEST-NODE");
            node.Test(role);
        }

        static int GetRole()
        {
            Console.WriteLine("Enter Role (0 - listener, 1 - sender): ");
            return int.Parse(Console.ReadLine());
        }

        static Socket GetSocket()
        {
            Socket s = SocketGenerator.GenerateUdpSocket();
            SocketGenerator.BindSocket(s, IPAddress.Loopback);
            return s;
        }
    }
}
