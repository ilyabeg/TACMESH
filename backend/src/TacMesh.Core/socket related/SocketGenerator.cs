using System.Net;
using System.Net.Sockets;

namespace TacMesh.Core.socket_related
{
    /// <summary>
    /// Socket generating class to provide fast and readeable socket generation and binding methods.    
    /// </summary>
    public class SocketGenerator
    {
        // constants
        public const int RandomPort = 0; // let the OS decide the port


        // --- Socket Generators ---

        /// <summary>
        /// Generates a new Socket
        /// </summary>
        /// <returns>new Socket instance bound to ip: Any, and Port: random free</returns>
        public static Socket GenerateUdpSocket()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            return socket;
        }


        // --- Socket Binding overloads ---

        /// <summary>
        /// Tries to bind the specified Socket to any IP and a free Port.
        /// </summary>
        /// <param name="socket">the socket to bind</param>
        public static void BindSocket(Socket socket)
        {
            socket.Bind(new IPEndPoint(IPAddress.Any, RandomPort));
        }

        /// <summary>
        /// Tries to bind the specified Socket to any IP and a specified Port.
        /// </summary>
        /// <param name="socket">the socket to bind</param>
        /// <param name="portNumber">the port number to bind to</param>
        public static void BindSocket(Socket socket, int portNumber)
        {
            socket.Bind(new IPEndPoint(IPAddress.Any, portNumber));
        }

        /// <summary>
        /// Tries to bind the specified Socket to a specified IP and any Port.
        /// </summary>
        /// <param name="socket">the socket to bind</param>
        /// <param name="ipAddress">the IP address to bind to</param>
        public static void BindSocket(Socket socket, IPAddress ipAddress)
        {
            socket.Bind(new IPEndPoint(ipAddress, RandomPort));
        }

        /// <summary>
        /// Tries to bind the specified Socket to the specified IP and Port.
        /// </summary>
        /// <param name="socket">the socket to bind</param>
        /// <param name="ipAddress">the IP address to bind to</param>
        /// <param name="portNumber">the port number to bind to</param>
        public static void BindSocket(Socket socket, IPAddress ipAddress, int portNumber)
        {
            socket.Bind(new IPEndPoint(ipAddress, portNumber));
        }
    }
}
