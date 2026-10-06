using System.Net;
using System.Net.Sockets;
using TacMesh.Core.custom_events;
using TacMesh.Core.events;
using TacMesh.Core.interfaces;
using TacMesh.Core.packet_related;
using TacMesh.Core.socket_related;

namespace TacMesh.Core.communication.simulation_communication
{
    public class SimReceiver : IReceiver
    {
        public event EventHandler<CrashEventArgs> CrashedEventHandler;
        public event EventHandler<MessageReceivedEventArgs> MessageReceivedEventHandler;

        private readonly Socket _source; // source socket

        // Constructor
        public SimReceiver(Socket source)
        {
            _source = source;
        }

        /// <summary>
        /// Receives messages from aby remote socket
        /// </summary>
        /// <param name="source">source socket</param>
        public void StartReceiving()
        {
            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        byte[] buffer = new byte[DataPacket.MaxPacketSize]; // buffer to hold the remote messages
                        EndPoint remoteEP = new IPEndPoint(IPAddress.Any, SocketGenerator.RandomPort); // remote endpoint holder

                        // ReceiveFrom return the number of bytes received
                        if (_source.ReceiveFrom(buffer, ref remoteEP) > 0)
                        {
                            // push the message to the Agent
                            IPEndPoint remoteIPEP = (IPEndPoint)remoteEP;
                            MessageReceivedEventHandler?.Invoke(this, new MessageReceivedEventArgs(buffer, remoteIPEP));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error caught while receiving message: '{e.Message}'");
                    CrashedEventHandler?.Invoke(this, new CrashEventArgs(e));
                }
            });           
        }
    }
}
