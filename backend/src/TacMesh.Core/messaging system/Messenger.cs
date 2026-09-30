using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TacMesh.Core;

/// <summary>
/// The Messenger Class is the class responsible for handling end-user's messages.
/// Core Features:
/// - Can send Packets of data to provided end-users.
/// - Can receive Packets from end-users.
/// - Can Broadcast Messages to nearby peers.
/// </summary>
public class Messenger
{
    // magic numbers
    private static readonly int _bufferSize = 1500; // mtu
    private static readonly IPAddress _anyIP = IPAddress.Any;
    private static readonly int _anyPort = 0;

    /// <summary>
    /// Infinite loop that listenes for 'Broadcasts' that were sent to the multicast group on the discovery port.
    /// </summary>
    /// <param name="socket">The Discovry Socket which receives the information from mcast group broadcasts</param>
    /// <param name="assignedPort">The actual User Socket port number</param>
    /// <param name="pushNotification">Callback to push the received information to the Agent out of the loop</param>
    /// <param name="shutdown">Callback used to shutdown the whole Node Process if the listening loop crashed</param>
    public static void ListenForBroadcast(Socket socket, int assignedPort, Action<byte[], IPEndPoint> pushNotification, Action shutdown)
    {
        try
        {
            while (true)
            {
                byte[] buffer = new byte[_bufferSize]; // buffer to hold the remote messages
                EndPoint remoteEP = new IPEndPoint(_anyIP, _anyPort); // remote endpoint holder

                // ReceiveFrom return the number of bytes received
                if (socket.ReceiveFrom(buffer, ref remoteEP) > 0)
                {
                    IPEndPoint remoteIPEP = (IPEndPoint)remoteEP;

                    string message = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine($"[BROADCAST]: {message} (from PORT {assignedPort})");

                    // TEMPORARY 'dumb' parsing to get port number of remote end
                    int parsedPort = int.Parse(message.Split('#')[1]);
                    IPEndPoint parsedEP = new IPEndPoint(IPAddress.Loopback, parsedPort);

                    // ignore self messages
                    if (parsedEP.Port == assignedPort) continue;

                    // push the message to the Agent
                    pushNotification.Invoke(buffer, parsedEP);                    
                }                
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error caught while listening for broadcasts: '{e.Message}'");
            shutdown.Invoke(); // shutdown node and all background threads gracefully
        }
    }

    /// <summary>
    /// Infinite loop that listenes for unicats that were sent to the User port.
    /// </summary>
    /// <param name="socket">The User Socket which receives the information from unicast messages</param>
    /// <param name="pushNotification">Callback to push the received information to the Agent out of the loop</param>
    /// <param name="shutdown">Callback used to shutdown the whole Node Process if the listening loop crashed</param>
    public static void ListenForUnicast(Socket socket, Action<byte[], IPEndPoint> pushNotification, Action shutdown)
    {
        try
        {
            while (true)
            {
                byte[] buffer = new byte[_bufferSize]; // buffer to hold the remote messages
                EndPoint remoteEP = new IPEndPoint(_anyIP, _anyPort); // remote endpoint holder

                // ReceiveFrom return the number of bytes received
                if (socket.ReceiveFrom(buffer, ref remoteEP) > 0)
                {
                    IPEndPoint remoteIPEP = (IPEndPoint)remoteEP;

                    string message = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine($"[UNICAST]: {message} (from PORT {remoteIPEP.Port})");

                    // TEMPORARY 'dumb' parsing to get port number of remote end
                    int parsedPort = int.Parse(message.Split('#')[1]);
                    IPEndPoint parsedEP = new IPEndPoint(IPAddress.Loopback, parsedPort);

                    // push the message to the Agent and stop
                    pushNotification.Invoke(buffer, parsedEP);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error caught while listening for unicasts: '{e.Message}'");
            shutdown.Invoke(); // shutdown node and all background threads gracefully
        }
    }

    public static void SendTo(Socket srcSocket, IPEndPoint remoteEP, byte[] message)
    {
        try
        {
            // send the bytes to the provided end point
            srcSocket.SendTo(message, remoteEP);
        } 
        catch (Exception e)
        {
            Console.WriteLine($"Error caught while sending message: '{e.Message}'");
        }
    }
}
