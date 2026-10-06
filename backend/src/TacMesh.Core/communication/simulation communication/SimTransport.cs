using System.Net;
using System.Net.Sockets;
using TacMesh.Core.custom_events;
using TacMesh.Core.events;
using TacMesh.Core.interfaces;
using TacMesh.Core.interfaces.communication_interfaces;

namespace TacMesh.Core.communication.simulation_communication
{
    public class SimTransport : ITransport
    {
        // private RX, TX, and source socket+id
        private readonly SimReceiver _receiver;
        private readonly SimTransmitter _transmitter;
        private readonly Socket _srcSocket;

        // event handlers
        public event EventHandler<CrashEventArgs> CrashedEventHandler;
        public event EventHandler<MessageReceivedEventArgs> MessageReceivedEventHandler;

        // injected radio model
        private readonly IRadioModel _radioModel;

        public SimTransport(Socket source, IRadioModel radioModel)
        {
            _srcSocket = source;
            _radioModel = radioModel;
            _receiver = new SimReceiver(source);
            _transmitter = new SimTransmitter(source);

            _receiver.CrashedEventHandler += (sender, e) => CrashedEventHandler?.Invoke(sender, e);
            _receiver.MessageReceivedEventHandler += (sender, e) => MessageReceivedEventHandler?.Invoke(sender, e);
            _transmitter.CrashedEventHandler += (sender, e) => CrashedEventHandler?.Invoke(sender, e);
        }

        /// <summary>
        /// Transmits messages to a given remote endpoint using the transmitter
        /// </summary>
        /// <param name="data"></param>
        /// <param name="destination"></param>
        public void Transmit(byte[] data, IPEndPoint destinationAddress)
        {
            Task.Run(async () => 
            {
                try
                {
                    // pull local enpoint from source socket
                    IPEndPoint sourceAddress = (IPEndPoint)_srcSocket.LocalEndPoint!;

                    // check via radio model if packet can be sent
                    // if denied, packet gets abandoned.
                    if (await _radioModel.TestConnection(sourceAddress, destinationAddress))
                        _transmitter.Transmit(data, destinationAddress);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });
        }

        /// <summary>
        /// Receives messages from any remote endpoint using the receiver
        /// </summary>
        public void StartReceiving() => _receiver.StartReceiving();

        /// <summary>
        /// Returns the port number that the source socket is bound to
        /// </summary>
        /// <returns></returns>
        public int GetAssignedPort() => ((IPEndPoint)_srcSocket.LocalEndPoint).Port;

        /// <summary>
        /// Run Close() on the source socket to shutdown both transmitter and receiver.
        /// Close() internally calls Dispose() on the socket, so no need to call Dispose().
        /// </summary>
        public void ShutDown() => _srcSocket.Close();
    }
}
