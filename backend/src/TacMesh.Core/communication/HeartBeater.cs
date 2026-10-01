using System.Net;
using TacMesh.Core.builders;
using TacMesh.Core.database_related;
using TacMesh.Core.interfaces;
using TacMesh.Core.serializers;

namespace TacMesh.Core.communication
{
    public class HeartBeater
    {
        // heart beater dependencies
        private Dictionary<string, IPEndPoint> _destinations;
        PacketHeaderSerializer _serializer;
        private ITransport _transporter;
        private string _srcID;       

        // inject dependencies through constructor
        public HeartBeater(string sourceID, Dictionary<string, IPEndPoint> destinations, PacketHeaderSerializer serializer, ITransport transporter)
        {
            _srcID = sourceID;
            _destinations = destinations;
            _serializer = serializer;
            _transporter = transporter;
        }

        /// <summary>
        /// Transmits heartbeat messages to all destinations in the _destinations dictionary each 5 seconds
        /// using a background thread
        /// </summary>
        public void TransmitHeartbeat()
        {
            // TEMPORARY COUNTER TO BREAK OUT OF THE HEARTBEAT LOOP AFTER 5 HEARTBEATS
            int TEMP_COUNTER = 0;

            Task.Run(async () =>
            {
                // wait 100ms before sending to ensure other sockets are open and actively listening
                await Task.Delay(100);

                // heartbeat loop
                while (true)
                {
                    // TEMPORARY TEST ONLY, SEND 5 HEARTBEATS AND BREAK OUT OF THE LOOP
                    if (TEMP_COUNTER == 5) break;

                    foreach (string destinationID in _destinations.Keys)
                    {
                        // NOTE: DATA PACKET NOT IMPLEMENTED YET SO I USE ONLY
                        // THE PACKET HEADER OBJECT FOR NOW INSTEAD.

                        // serialize each header for each destination
                        PacketHeader header = CreateHeader(destinationID);
                        byte[] headerBytes = _serializer.Serialize(header);

                        // treansmit header and log
                        _transporter.Transmit(headerBytes, _destinations[destinationID]);
                        Logger.LogHeartbeat(LoggingMode.Sent, _srcID, destinationID, DateTime.Now);
                    }
                    await Task.Delay(5000);

                    TEMP_COUNTER++;
                }

                // TEMPORARILY CHECK THE LOGS HERE. WILL CHANGE IN FUTURE
                Logger.PrintLogs();
            });
        }

        private PacketHeader CreateHeader(string destinationID)
        {
            PacketHeaderBuilder builder = new PacketHeaderBuilder();

            // TEMPORARILY USE MAGIC NUMBERS HERE, WILL BE CHANGED IN THE FUTURE.
            // FOR NOW I USE MAGIC NUMBERS UNTILL I IMPLEMENT A CONFIGURATION FILE.

            builder.SetProtocolVersion(1)
                .SetMessageType(PacketType.Heartbeat)
                .SetMessageID(Guid.NewGuid())
                .SetSourceID(_srcID)
                .SetDestinationID(destinationID)
                .SetHopCount()
                .SetLogicalClock(0)
                .SetTimeToLive(5)
                .SetPriority(1)
                .SetSenderCounter(0);

            return builder.BuildHeader();
        }

        /// <summary>
        /// Updates old destinations if new provided
        /// </summary>
        /// <param name="newDestinations"></param>
        public void UpdateDestinations(Dictionary<string, IPEndPoint> newDestinations) => _destinations = newDestinations;
    }
}
