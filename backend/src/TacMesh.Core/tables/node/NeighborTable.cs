using System.Net;
using System.Text;
using TacMesh.Core.assets;
using TacMesh.Core.assets.graph;
using TacMesh.Core.interfaces.clock_interfaces;
using TacMesh.Core.utils;
using TacMesh.Core.utils.design;

namespace TacMesh.Core.tables.node
{
    public class NeighborTable
    {
        // holds record for each neighbour by his ID
        public Dictionary<string, NeighborRecord> NeighborRecords { get; private set; }
        public int N { get; private set; }
        private readonly IClock _tableClock;
        private readonly int _legal_timeDiff_seconds;

        // read-write lock for the table
        // LOCK ORDER RULE: Always acquire access to the NeighborTable lock before the Buffer lock to prevent Deadlocks
        // Brief explanation documented in the README file.
        private readonly TacReadWriteLock _rwLock = new TacReadWriteLock();


        // inject any clock to the table
        public NeighborTable(IClock tableClock, int cyclesNumber = 3) // if cycles isn't provided, default = 3
        {
            _tableClock = tableClock;
            N = cyclesNumber;
            _legal_timeDiff_seconds = (N * SysConfigs.HeartbeatDelayMs / 1000);
            NeighborRecords = new Dictionary<string, NeighborRecord>();
        }

        // determines wether the provided neighbor id exists in the neighbor table
        public bool Contains(string neighborId)
        {
            using (_rwLock.Read())
            {
                return NeighborRecords.ContainsKey(neighborId);
            }
        }

        // returns the ip endpoint destination of the provided neighbor id
        public IPEndPoint? GetNeigborAddress(string neighborId)
        {
            using (_rwLock.Read())
            {
                // if neigbor exists
                if (NeighborRecords.ContainsKey(neighborId)) 
                {
                    return NeighborRecords[neighborId].Address;
                }
                else {
                    return null;
                }
            }
        }

        // adds new record if doesn't exist OR overrides an existing one
        public void UpdateRecord(string neighborId, Location neighborLocation, IPEndPoint address)
        {
            using (_rwLock.Write())
            {
                NeighborRecords[neighborId] = new NeighborRecord(neighborId, address, neighborLocation, _tableClock.GetTime());
            }
        }

        // checks for expired nodes and removes
        public void CheckExpiration()
        {
            using (_rwLock.Write())
            {
                DateTime current_time = _tableClock.GetTime();
                List<string> expiredNeighbours = new List<string>();

                // O(NeighborRecords.Length) - iterates through all records
                foreach (NeighborRecord record in NeighborRecords.Values)
                {
                    DateTime last_heartbeat = record.LastHeartbeatTime;
                    int time_diff = (int)(current_time - last_heartbeat).TotalSeconds;

                    // if heartbeat wasn't received from this neighbor for over N, heartbeat time cycles
                    if (time_diff >= _legal_timeDiff_seconds)
                        expiredNeighbours.Add(record.NeighborID);
                }

                // remove all expired neighbours
                foreach (string expired_neighbour in expiredNeighbours)
                    RemoveRecord(expired_neighbour);
            }
        }

        // removes based on the neighbor id key
        private void RemoveRecord(string neighbourId)
        {
            if (!NeighborRecords.Remove(neighbourId, out _))
                throw new Exception($"Failed to remove neighbour {neighbourId} from table.");
        }

        // background expiration checker
        public void StartExpirationCheck()
        {
            Task.Run(async () =>
            {
                int TEMP_COUNT = 1;
                while (true)
                {
                    // TMPORARILY SHOW ON SCRREN WHICH PULSE OF CHECK IS SHOWN
                    Console.WriteLine($"~~~~~~~~~ [PULSE #{TEMP_COUNT}] ~~~~~~~~~");
                    TEMP_COUNT++;

                    CheckExpiration();
                    PrintTable(); // table test
                    await Task.Delay(SysConfigs.HeartbeatDelayMs); // match heartbeat delay
                }
            });
        }


        public void PrintTable()
        {
            StringBuilder str = new StringBuilder();

            str.AppendLine("┌────────────────────────────────────────────────────────────┐");

            string title = $" NEIGHBOUR TABLE";
            str.AppendLine($"│{title.PadRight(ConsoleDesign._60_padRight)}│");

            str.AppendLine("├────────────┬────────────┬────────────┬─────────────────────┤");
            str.AppendLine("│ Neighbor   │ Location   │ Address    │ Last Heartbeat      │");
            str.AppendLine("├────────────┼────────────┼────────────┼─────────────────────┤");

            using (_rwLock.Read())
            {
                foreach (string neighborID in NeighborRecords.Keys)
                {
                    string address = NeighborRecords[neighborID].Address.Port.ToString();
                    string location = NeighborRecords[neighborID].Location.ToString();
                    string heartbeat = NeighborRecords[neighborID].LastHeartbeatTime.ToString();

                    string padded_string = PadRecordString(neighborID, address, location, heartbeat);
                    str.AppendLine(padded_string);
                }
            }
            str.AppendLine("└────────────┴────────────┴────────────┴─────────────────────┘");
            Console.WriteLine(str);
        }

        // method to pad record string and visualy read what is happening
        private string PadRecordString(string id, string address, string location, string heartbeat)
        {
            string padded_id = $"{id,ConsoleDesign._10_padRight}";
            string padded_address = $"{address,ConsoleDesign._10_padRight}";
            string padded_location = $"{location,ConsoleDesign._10_padRight}";
            string padded_heartbeat = $"{heartbeat,ConsoleDesign._10_padRight}";

            return $"| {padded_id} | {padded_address} | {padded_location} | {padded_heartbeat} |";
        }
    }
}
