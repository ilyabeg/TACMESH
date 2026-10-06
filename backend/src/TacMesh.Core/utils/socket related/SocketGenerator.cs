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

        // WINDOWS CONSTANT NUMBER THAT DISABLES THE SOCKET'S RESET UPON RECEIVING
        // AN ICMP PORT UNREACHABLE 
        private const int SIO_UDP_CONNRESET = -1744830452;


        // --- Socket Generators ---

        /// <summary>
        /// Generates a new Socket
        /// </summary>
        /// <returns>a new Socket instance bound to ip: Any, and Port: random free</returns>
        public static Socket GenerateUdpSocket()
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            // disable the automatic closing of the socket on ICMP Port Unreachable
            socket.IOControl(SIO_UDP_CONNRESET, new byte[] { 0 }, null);

            return socket;
        }

        /// <summary>
        /// Generates a new multicast socket
        /// </summary>
        /// <returns>a new Socket instance that can receive mcast messages if sent to the fixed mcast port</returns>
        public static Socket GenerateMcastListenerSocket(IPAddress mcastIP, int mcastPort)
        {
            // let the socket to be bound to an address already in use
            Socket s = GenerateUdpSocket();
            s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // add socket to the mcast group to receive mcast packets
            MulticastOption multicastOption = new MulticastOption(mcastIP);
            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, multicastOption);

            // bind socket to the fixed mcast port
            BindSocket(s, IPAddress.Any, mcastPort);
            return s;
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
