using System.Net;
using System.Net.Sockets;
using TacMesh.Core.custom_events;
using TacMesh.Core.interfaces;

namespace TacMesh.Core.communication
{
    public class SimTransmitter : ITransmitter
    {
        public event EventHandler<CrashEventArgs> Crashed;

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
        /// <returns>The number of bytes sent or -1 if failed to send.</returns>
        public int Transmit(byte[] data, IPEndPoint destination)
        {
            try
            {
                // send the bytes to the provided end point and return the number of bytes sent
                int count = _source.SendTo(data, destination);
                return count;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error caught while transmitting message: '{e.Message}'");
                Crashed?.Invoke(this, new CrashEventArgs(e));
                return -1;
            }
        }
    }
}
