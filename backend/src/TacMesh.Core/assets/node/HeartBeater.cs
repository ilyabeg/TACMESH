using System.Net;
using TacMesh.Core.assets.clocks;
using TacMesh.Core.assets.node.packet;
using TacMesh.Core.database_related;
using TacMesh.Core.interfaces;
using TacMesh.Core.interfaces.communication_interfaces;
using TacMesh.Core.packet;
using TacMesh.Core.utils;

namespace TacMesh.Core.assets.node
{
    public class HeartBeater
    {        
        private const int _initialization_delay = 500;//ms

        // heart beater dependencies
        private readonly string _srcId; // id of the src node
        private readonly NodeConfigs _nodeConfigs; // keep access to node location
        private readonly HeartbeatLogger _logger;
        private readonly ITransport _transporter;
        private readonly IRadioModel _radioModel;
        private readonly LamportClock _lamportClock;
        private readonly IByteSerializer<DataPacket> _packetSerializer;
        private readonly SenderCounter _senderCounter;


        // inject dependencies through constructor
        public HeartBeater(string nodeId, NodeConfigs nodeConfigs, HeartbeatLogger logger, ITransport transporter, IRadioModel radioModel, LamportClock lamportClock, IByteSerializer<DataPacket> packetSerializer, SenderCounter senderCounter)
        {
            _srcId = nodeId;
            _nodeConfigs = nodeConfigs;
            _logger = logger;
            _transporter = transporter;
            _radioModel = radioModel;
            _lamportClock = lamportClock;
            _packetSerializer = packetSerializer;
            _senderCounter = senderCounter;
        }

        /// <summary>
        /// Transmits heartbeat messages to all destinations in the dictionary each 5 seconds
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
                    try
                    {
                        // get the known peers in the net
                        Dictionary<string, IPEndPoint> destinations = _radioModel.GetPeers();

                        foreach (string destID in destinations.Keys)
                        {
                            // ignore same node or unpermitted neighbors
                            //
                            if (destID.Equals(_srcId) || !_nodeConfigs.PermittedNeighbors.Contains(destID))
                                continue;

                            // pull node address
                            IPEndPoint destAddress = destinations[destID];

                            // serialize each packet for each destination
                            DataPacket packet = CreateHeartbeatPacket(destID);
                            byte[] packetBytes = _packetSerializer.Serialize(packet);

                            // transmit packet and log heartbeat if successfuly sent bytes
                            if (_transporter.Transmit(packetBytes, destAddress) > 0)
                            { 
                                _logger.FormatAndLogHeartbeat(LoggingMode.Sent, _srcId, destID);
                            }

                            // test heartbeats received and sent
                            //_logger.PrintLogs();
                        }
                        await Task.Delay(SysConfigs.HeartbeatDelayMs);
                    }
                    catch (Exception e)
                    {
                        // catch exception and retry
                        Console.WriteLine($"[HEARTBEATER CREASHED] '{e.Message}'");
                    }
                }
            });
        }

        private DataPacket CreateHeartbeatPacket(string destinationID)
        {
            // init heartbeat data packet packet header
            //
            PacketHeader header = new PacketHeader(
                msgType: PacketType.Heartbeat,
                srcID: _srcId,
                dstID: destinationID,
                msgID: Guid.NewGuid(),
                logicalClock: _lamportClock.GetNext(),
                senderCounter: _senderCounter.GetCurrent()
            );

            // serialize location and add to the payload
            //
            byte[] payload = _nodeConfigs.Location.ToBytes();

            return new DataPacket(header, payload);
        }

        // logs heartbeat
        public void LogHeartbeat(LoggingMode mode, string src, string dst) => _logger.FormatAndLogHeartbeat(mode, src, dst);
    }
}
