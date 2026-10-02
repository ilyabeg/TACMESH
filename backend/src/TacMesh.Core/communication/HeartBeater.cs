using System.Net;
using TacMesh.Core.builders;
using TacMesh.Core.configurations;
using TacMesh.Core.database_related;
using TacMesh.Core.interfaces;
using TacMesh.Core.serializers;

namespace TacMesh.Core.communication
{
    public class HeartBeater
    {        
        private const int _initialization_delay = 100;//ms

        // heart beater dependencies
        private HeartbeatLogger _logger;
        private Dictionary<string, IPEndPoint> _destinations;
        private PacketHeaderSerializer _serializer;
        private ITransport _transporter;
        private string _srcID;       


        // inject dependencies through constructor
        public HeartBeater(string sourceID, HeartbeatLogger logger, Dictionary<string, IPEndPoint> destinations, PacketHeaderSerializer serializer, ITransport transporter)
        {
            _srcID = sourceID;
            _logger = logger;
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
            Task.Run(async () =>
            {
                // wait a small delay before sending to ensure other sockets are open and actively listening
                await Task.Delay(_initialization_delay);

                // heartbeat loop
                while (true)
                {
                    foreach (string destinationID in _destinations.Keys)
                    {
                        // NOTE: DATA PACKET NOT IMPLEMENTED YET SO I USE ONLY
                        // THE PACKET HEADER OBJECT FOR NOW INSTEAD.

                        // serialize each header for each destination
                        PacketHeader header = CreateHeader(destinationID);
                        byte[] headerBytes = _serializer.Serialize(header);

                        // treansmit header and log
                        _transporter.Transmit(headerBytes, _destinations[destinationID]);
                        _logger.FormatAndLogHeartbeat(LoggingMode.Sent, _srcID, destinationID);
                    }
                    await Task.Delay(SystemConfigurations.HeartbeatDelayMs);
                }
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

        // IMPORTANT NOTE: I KNOW THIS UPDATE FUNCTION IS NOT GOOD, IT CAN CAUSE RACE CONDITIONS
        // AND A 'COLLECTION MODIFIED' ERROR, IT IS SIMPLY A SORT OF BOILERPLATE SO I DON'T FORGET
        // TO UPDATE AND ADD THIS KIND OF METHOD LATER. I DO NOT INTED ON LEAVING IT THIS WAY.

        /// <summary>
        /// Updates old destinations if new provided
        /// </summary>
        /// <param name="newDestinations"></param>
        public void UpdateDestinations(Dictionary<string, IPEndPoint> newDestinations) => _destinations = newDestinations;
    }
}
