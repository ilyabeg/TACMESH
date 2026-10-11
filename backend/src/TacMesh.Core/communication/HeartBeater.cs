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
        private const int _initialization_delay = 500;//ms

        // heart beater dependencies
        private readonly string _srcID;
        private readonly HeartbeatLogger _logger;
        private readonly PacketHeaderSerializer _serializer;
        private readonly ITransport _transporter;
        private readonly IRadioModel _radioModel;


        // event to handle pre-heartbeat operations
        public event EventHandler<int> PreHeartbeat;


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
                //await Task.Delay(_initialization_delay);
                //int TEMP_PULSE = 1;

                // heartbeat loop
                //while (true)
                //{
                    try
                    {
                        // fire pre-heartbeat event
                        //PreHeartbeat?.Invoke(this, TEMP_PULSE);
                        //TEMP_PULSE++;

                        // get the known peers in the net
                        Dictionary<string, IPEndPoint> approvedDestinations = _radioModel.GetPeers();

                        foreach (string destID in approvedDestinations.Keys)
                        {
                            // NOTE: DATA PACKET NOT IMPLEMENTED YET SO I USE ONLY
                            // THE PACKET HEADER OBJECT FOR NOW INSTEAD.

                            // ignore same node
                            //if (_srcID == destID)
                            //    continue;

                            // pull node address
                            IPEndPoint destAddress = approvedDestinations[destID];

                            // serialize each header for each destination
                            PacketHeader header = CreateHeader(destID);
                            byte[] headerBytes = _serializer.Serialize(header);

                            // transmit header and log
                            _transporter.Transmit(headerBytes, destAddress);
                            _logger.FormatAndLogHeartbeat(LoggingMode.Sent, _srcID, destID);

                            // test heartbeats received and sent
                            //_logger.PrintLogs();
                        }
                        //await Task.Delay(SystemConfigs.HeartbeatDelayMs);
                    }
                    catch (Exception e)
                    {
                        // catch exception and retry
                        //Console.WriteLine($"[HEARTBEATER CREASHED] '{e.Message}'");
                    }
                //}
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
