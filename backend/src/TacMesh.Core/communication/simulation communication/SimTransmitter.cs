using System.Net;
using System.Net.Sockets;
using TacMesh.Core.custom_events;
using TacMesh.Core.interfaces;

namespace TacMesh.Core.communication
{
    public class SimTransmitter : ITransmitter
    {
        public event EventHandler<CrashEventArgs> CrashedEventHandler;

        private readonly Socket _source; // source socket

        // Constructor
        public SimTransmitter(Socket source)
        {
            _source = source;
        }

        /// <summary>
        /// Transmits messages to a given remote socket
        /// </summary>
        /// <param name="data"></param>
        /// <param name="destination"></param>
        public void Transmit(byte[] data, IPEndPoint destination)
        {
            try
            {
                // send the bytes to the provided end point
                _source.SendTo(data, destination);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error caught while transmitting message: '{e.Message}'");
                CrashedEventHandler?.Invoke(this, new CrashEventArgs(e));
            }
        }
    }
}
