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
    private static readonly int _bufferSize = 1500;
    private static readonly IPAddress _anyIP = IPAddress.Any;
    private static readonly int _anyPort = 0;

    public static void ListenForBroadcast(Socket socket, Action<byte[], IPEndPoint> pushNotification)
    {
        try
        {
            while (true)
            {
                byte[] buffer = new byte[_bufferSize];
                EndPoint remoteEP = new IPEndPoint(_anyIP, _anyPort);

                int count = socket.ReceiveFrom(buffer, ref remoteEP);

                if (count > 0)
                {
                    IPEndPoint remoteIPEP = (IPEndPoint)remoteEP;

                    string message = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine($"Received message: {message} (BROADCAST)");

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
            Console.WriteLine($"Error caught while listening for broadcasts: {e.Message}");
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
            Console.WriteLine($"Error caught while sending: {e.Message}");
        }
    }

    public static void ListenForUnicast(Socket socket)
    {
        try
        {
            while (true)
            {
                byte[] buffer = new byte[_bufferSize];
                EndPoint remoteEP = new IPEndPoint(_anyIP, _anyPort);

                int count = socket.ReceiveFrom(buffer, ref remoteEP);

                if (count > 0)
                {
                    IPEndPoint remoteIPEP = (IPEndPoint)remoteEP;

                    string message = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine($"Received message: {message} (UNICAST from {remoteIPEP.Address}:{remoteIPEP.Port})");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error caught while listening for unicasts: {e.Message}");
        }
    }
}
