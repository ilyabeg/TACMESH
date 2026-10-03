using System.Collections.Concurrent;
using System.Net;
using System.Text;
using TacMesh.Core.assets.read_write_lock;
using TacMesh.Core.configurations;
using TacMesh.Core.interfaces.clock_interfaces;

namespace TacMesh.Core.tables.node_related
{
    public class NeighbourTable
    {
        // holds record for each neighbour by his ID
        public Dictionary<string, NeighbourRecord> NeighbourRecords { get; private set; }
        public int N { get; private set; }
        private readonly IClock _tableClock;
        private readonly int _legal_timeDiff_seconds;

        // read-write lock for the table
        // LOCK ORDER RULE: Always acquire access to the NeighbourTable lock before the Buffer lock to prevent Deadlocks
        // Brief explanation documented in the README file.
        private readonly TacReadWriteLock _rwLock = new TacReadWriteLock();


        // inject any clock to the table
        public NeighbourTable(IClock tableClock, int cyclesNumber = 3) // if cycles isn't provided, default = 3
        {
            _tableClock = tableClock;
            N = cyclesNumber;
            _legal_timeDiff_seconds = (N * SystemConfigurations.HeartbeatDelayMs / 1000);
            NeighbourRecords = new Dictionary<string, NeighbourRecord>();
        }

        // adds new record if doesn't exist OR overrides an existing one
        public void UpdateRecord(string neighbourId, IPEndPoint address)
        {
            using (_rwLock.Write())
            {
                NeighbourRecords[neighbourId] = new NeighbourRecord(neighbourId, address, _tableClock.GetTime());
            }
        }

        // checks for expired nodes and removes
        public void CheckExpiration()
        {
            using (_rwLock.Write())
            {
                DateTime current_time = _tableClock.GetTime();
                List<string> expiredNeighbours = new List<string>();

                // O(NeighbourRecords.Length) - iterates through all records
                foreach (NeighbourRecord record in NeighbourRecords.Values)
                {
                    DateTime last_heartbeat = record.LastHeartbeatTime;
                    int time_diff = (int)(current_time - last_heartbeat).TotalSeconds;

                    // if heartbeat wasn't received from this neighbour for over N, heartbeat time cycles
                    if (time_diff >= _legal_timeDiff_seconds)
                        expiredNeighbours.Add(record.NeighbourID);
                }

                // remove all expired neighbours
                foreach (string expired_neighbour in expiredNeighbours)
                    RemoveRecord(expired_neighbour);
            }
        }

        // removes based on the neighbour id key
        private void RemoveRecord(string neighbourId)
        {
            if (!NeighbourRecords.Remove(neighbourId, out _))
                throw new Exception($"Failed to remove neighbour {neighbourId} from table.");
        }

        // background expiration checker
        public void StartExpirationCheck()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    CheckExpiration();
                    PrintTable(); // table test
                    await Task.Delay(SystemConfigurations.HeartbeatDelayMs); // match heartbeat delay
                }
            });
        }


        public void PrintTable()
        {
            // ai design...
            StringBuilder str = new StringBuilder();

            str.AppendLine("┌───────────────────────────────────────────────┐");

            string title = $" NEIGHBOUR TABLE";
            str.AppendLine($"│{title.PadRight(SystemConfigurations._47_padRight)}│");

            str.AppendLine("├────────────┬────────────┬─────────────────────┤");
            str.AppendLine("│ Neighbour  │ Address    │ Last Heartbeat      │");
            str.AppendLine("├────────────┼────────────┼─────────────────────┤");

            using (_rwLock.Read())
            {
                foreach (string neighbourID in NeighbourRecords.Keys)
                {
                    string address = NeighbourRecords[neighbourID].Address.Port.ToString();
                    string heartbeat = NeighbourRecords[neighbourID].LastHeartbeatTime.ToString();

                    string padded_string = PadRecordString(neighbourID, address, heartbeat);
                    str.AppendLine(padded_string);
                }
            }            
            str.AppendLine("└────────────┴────────────┴─────────────────────┘");

            Console.WriteLine(str);
        }

        // method to pad record string and visualy read what is happening
        private string PadRecordString(string id, string address, string heartbeat)
        {
            string padded_id = $"{id, SystemConfigurations._10_padRight}";
            string padded_address = $"{address, SystemConfigurations._10_padRight}";
            string padded_heartbeat = $"{heartbeat,SystemConfigurations._10_padRight}";

            return $"| {padded_id} | {padded_address} | {padded_heartbeat} |";
        }
    }
}
