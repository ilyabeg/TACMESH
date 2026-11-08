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
        public int N { get; private set; } // number of legal cycles before dropping an unheard neighbor
        private readonly IClock _tableClock;
        private readonly int _legal_timeDiff_seconds;

        // read-write lock for the table
        // LOCK ORDER RULE: Always acquire access to the NeighborTable lock before the Buffer lock to prevent Deadlocks
        // Brief explanation documented in the README file.
        private readonly TacReadWriteLock _rwLock = new TacReadWriteLock();

        // public event to invoke when the neighbor table changes (the local node's link state)
        public event EventHandler<List<string>>? OnLinkStateChanged;


        // inject any clock to the table
        public NeighborTable(IClock tableClock, int cyclesNumber = 3) // if cycles isn't provided, default = 3
        {
            _tableClock = tableClock;
            N = cyclesNumber;
            _legal_timeDiff_seconds = (N * SysConfigs.HeartbeatDelayMs / 1000);
            NeighborRecords = new Dictionary<string, NeighborRecord>();

            // invoke the link state update every a couple of seconds to ensure no 
            // "graph blind spots" happen (when no change in the link state happens and
            // nodes remain "blind" to the actual net topology without an update)
            //
            // Note: this will be documented in the project book
            //
            RunLinkStateUpdate();
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
                if (NeighborRecords.TryGetValue(neighborId, out NeighborRecord? value)) 
                {
                    return value.Address;
                }
                else {
                    return null;
                }
            }
        }

        /// <summary>
        /// Method to get the current neighbor identifiers
        /// </summary>
        /// <returns>A list of all the current neighbors linked to this node</returns>
        public List<string> GetCurrentNeighbors()
        {
            using (_rwLock.Read())
            {
                return NeighborRecords.Keys.ToList();
            }
        }

        // adds new record if doesn't exist OR overrides an existing one
        public void UpdateRecord(string neighborId, Location neighborLocation, IPEndPoint address)
        {
            List<string>? linkState = null; // hold the neighbors links if the table changed changed
            bool isNew; // if a new neighbor added this is true

            using (_rwLock.Write())
            {
                // TryGetValue() returns true if the dictionary contains a value with the specified key.
                // if a new neighbor is added the result of TryGetValue() is false, because there is no value
                // associated with that specific key, so the flag isNew should be true (the opposite of the result)
                //
                isNew = !NeighborRecords.TryGetValue(neighborId, out _);

                // adds new record or updates if exists
                //
                NeighborRecords[neighborId] = new NeighborRecord(neighborId, address, neighborLocation, _tableClock.GetTime());

                // save the new link chaged state
                //
                if (isNew)
                    linkState = NeighborRecords.Keys.ToList();
            }

            // if the neighbor table changed, invoke event, and pass current neighbor identifiers
            //
            if (linkState != null) OnLinkStateChanged?.Invoke(this, linkState);
        }

        // checks for expired nodes and removes
        public void CheckExpiration()
        {                        
            bool tableChanged = false; // flag to determine if to invoke the changed event
            List<string>? linkState = null; // hold the neighbors links if the table changed

            using (_rwLock.Write())
            {
                List<string> expiredNeighbors = new List<string>();
                DateTime current_time = _tableClock.GetTime();

                // O(NeighborRecords.Length) - iterates through all records
                //
                foreach (NeighborRecord record in NeighborRecords.Values)
                {
                    DateTime last_heartbeat = record.LastHeartbeatTime;
                    int time_diff = (int)(current_time - last_heartbeat).TotalSeconds;

                    // if heartbeat wasn't received from this neighbor for over N, heartbeat time cycles
                    if (time_diff >= _legal_timeDiff_seconds)
                        expiredNeighbors.Add(record.NeighborID);
                }

                // remove all expired neighbours
                //
                foreach (string expired_neighbour in expiredNeighbors)
                {
                    RemoveRecord(expired_neighbour);
                    tableChanged = true;
                }

                // save the new changed link state
                //
                if (tableChanged) 
                    linkState = NeighborRecords.Keys.ToList();
            }

            // if the neighbor table changed, invoke event, and pass current neighbor identifiers
            //
            if (linkState != null)
                OnLinkStateChanged?.Invoke(this, linkState);
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
                    //Console.WriteLine($"~~~~~~~~~ [PULSE #{TEMP_COUNT}] ~~~~~~~~~");
                    TEMP_COUNT++;

                    CheckExpiration();
                    //PrintTable(); // table test
                    await Task.Delay(SysConfigs.HeartbeatDelayMs); // match heartbeat delay
                }
            });
        }

        /// <summary>
        /// background thread to invoke the link state update every a couple of seconds
        /// to avoid network "blind spots"
        /// </summary>
        private void RunLinkStateUpdate()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    // await the delay before invoking to let the link state
                    // dispatcher subscribe to the event
                    //
                    await Task.Delay(SysConfigs.HeartbeatDelayMs); // match heartbeat delay between event calls

                    List<string> currLinkState = GetCurrentNeighbors();

                    if (currLinkState.Count > 0)
                        OnLinkStateChanged?.Invoke(this, currLinkState);
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

            return $"│ {padded_id} │ {padded_address} │ {padded_location} │ {padded_heartbeat} │";
        }
    }
}
