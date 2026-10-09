using System.Collections.Concurrent;
using System.Net;
using TacMesh.Core.builders;
using TacMesh.Core.database_related;
using TacMesh.Core.interfaces;
using TacMesh.Core.interfaces.communication_interfaces;
using TacMesh.Core.serializers;
using TacMesh.Core.utils.configurations;

namespace TacMesh.Core.communication
{
    public class HeartBeater
    {        
        private const int _initialization_delay = 10;//ms

        // heart beater dependencies
        private readonly string _srcID;
        private readonly HeartbeatLogger _logger;
        private readonly PacketHeaderSerializer _serializer;
        private readonly ITransport _transporter;
        private readonly IRadioModel _radioModel;              


        // inject dependencies through constructor
        public HeartBeater(string sourceID, HeartbeatLogger logger, PacketHeaderSerializer serializer, ITransport transporter, IRadioModel radioModel)
        {
            _srcID = sourceID;
            _logger = logger;
            _serializer = serializer;
            _transporter = transporter;
            _radioModel = radioModel;
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
                    ConcurrentDictionary<string, IPEndPoint> destinations = _radioModel.GetNetworkDevices();
                    
                    foreach (string destID in destinations.Keys)
                    {
                        // NOTE: DATA PACKET NOT IMPLEMENTED YET SO I USE ONLY
                        // THE PACKET HEADER OBJECT FOR NOW INSTEAD.
                        
                        // pull node address
                        IPEndPoint destAddress = destinations[destID];

                        // filter out packets sent to myself to not create unnecessary packets
                        if (_srcID == destID) continue;

                        // serialize each header for each destination
                        PacketHeader header = CreateHeader(destID);
                        byte[] headerBytes = _serializer.Serialize(header);

                        // transmit header and log
                        _transporter.Transmit(headerBytes, destAddress);
                        _logger.FormatAndLogHeartbeat(LoggingMode.Sent, _srcID, destID);

                        // test heartbeats received and sent
                        //_logger.PrintLogs();
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

        // logs heartbeat
        public void LogHeartbeat(LoggingMode mode, string src, string dst) => _logger.FormatAndLogHeartbeat(mode, src, dst);
    }
}
