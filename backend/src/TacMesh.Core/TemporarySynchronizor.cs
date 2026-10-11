using TacMesh.Core.communication;
using TacMesh.Core.tables.node_related;

namespace TacMesh.Core
{
    public class TemporarySynchronizor
    {

        private readonly HeartBeater heartbeater;
        private readonly NeighbourTable table;

        public TemporarySynchronizor(HeartBeater heartbeater, NeighbourTable table)
        {
            this.heartbeater = heartbeater;
            this.table = table;
        }

        public void Start()
        {
            Task.Run(async () =>
            {

                int TEMP_COUNT = 0;
                while (true)
                {
                    long currentTick = Environment.TickCount64;

                    int msToNextPulse = 5000 - (int)(currentTick % 5000);

                    await Task.Delay(msToNextPulse);
                    Console.WriteLine($"~~~~~~~~~ [PULSE #{++TEMP_COUNT}] ~~~~~~~~~");

                    table.CheckExpiration();
                    table.PrintTable();

                    await Task.Delay(500);
                    heartbeater.TransmitHeartbeat();
                }
            });
        }
    }
}
