using System.Text;
using TacMesh.Core.configurations;
using TacMesh.Core.interfaces.data_interfaces;

namespace TacMesh.Core.database_related
{
    // Logging mode to determine if Sent/Received 
    public enum LoggingMode
    {
        Sent,
        Received
    }

    public class HeartbeatLogger : ILogger<string>
    {
        // VERY IMPORTANT NOTE: THIS IS A TEMPORARY IMPLEMENTATION OF A LOGGER CLASS,
        // I AM AWARE THAT THIS KIND OF WRITING IS NOT A GOOD IDEA AND USING A LOCK IS NO
        // T IDEAL AND WILL CAUSE BOTTLENECKS IF ALOT OF PACKETS ARE RECEIVED
        // SIMULTANEOUSLY, I WILL FIX THIS, BUT ONLY FOR TEMPORARY TESING I
        // WILL USE THIS FOR NOW.
        public List<string> Logs { get; private set; }
        private static readonly object _lock = new object();

        // Constructor
        public HeartbeatLogger()
        {
            Logs = new List<string>();
        }
        
        // interface methods
        public void Log(string data)
        {
            lock (_lock)
            {
                Logs.Add(data);
            }
        }

        public void PrintLogs()
        {
            lock (_lock)
            {
                // ai design...
                StringBuilder str = new StringBuilder();

                str.AppendLine("┌──────────────────────────────────────────────────────────┐");

                string title = $" LOGS (Total: {Logs.Count})";
                str.AppendLine($"│{title.PadRight(SystemConfigurations._58_padRight)}│");

                str.AppendLine("├──────────┬────────────┬────────────┬─────────────────────┤");
                str.AppendLine("│ ACTION   │ SOURCE     │ DEST       │ TIMESTAMP           │");
                str.AppendLine("├──────────┼────────────┼────────────┼─────────────────────┤");

                foreach (string log in Logs)
                    str.AppendLine(log);

                str.AppendLine("└──────────┴────────────┴────────────┴─────────────────────┘");

                Console.WriteLine(str);
            }
        }

        // Heartbeat log padding method
        public void FormatAndLogHeartbeat(LoggingMode mode, string src, string dst)
        {
            string action = (mode == LoggingMode.Sent) ? "Sent" : "Received";

            action = $"{action, SystemConfigurations._8_padRight}";
            src = $"{src, SystemConfigurations._10_padRight}";
            dst = $"{dst, SystemConfigurations._10_padRight}";
            string time = $"{DateTime.Now, SystemConfigurations._10_padRight}";

            string log = $"| {action} | {src} | {dst} | {time} |";
            Log(log);
        }
    }
}
