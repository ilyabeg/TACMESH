using System.Net;
using System.Net.Sockets;
using TacMesh.Core.custom_events;
using TacMesh.Core.events;
using TacMesh.Core.interfaces;
using TacMesh.Core.packet_related;
using TacMesh.Core.utils;

namespace TacMesh.Core.communication.simulation_communication
{
    public class SimReceiver : IReceiver
    {
        public event EventHandler<CrashEventArgs> Crashed;
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

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
                        int len = _source.ReceiveFrom(buffer, ref remoteEP);

                        // ReceiveFrom return the number of bytes received
                        if (len > 0)
                        {
                            // actual size buffer
                            byte[] receivedBytes = new byte[len];
                            Array.Copy(buffer, receivedBytes, len); // copy only received bytes

                            // push the message to the Engine
                            IPEndPoint remoteIPEP = (IPEndPoint)remoteEP;
                            MessageReceived?.Invoke(this, new MessageReceivedEventArgs(receivedBytes, remoteIPEP));
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"[Sim Receiver] Error caught while receiving message: '{e}'");
                    Crashed?.Invoke(this, new CrashEventArgs(e));
                }
            });           
        }
    }
}
