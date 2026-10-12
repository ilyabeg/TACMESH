using System.Text;
using TacMesh.Core.interfaces.data_interfaces;
using TacMesh.Core.utils.design;

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

                string title = $" HEARTBEAT LOGS (Total: {Logs.Count})";
                str.AppendLine($"│{title.PadRight(ConsoleDesign._58_padRight)}│");

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

            action = $"{action,ConsoleDesign._8_padRight}";
            src = $"{src,ConsoleDesign._10_padRight}";
            dst = $"{dst,ConsoleDesign._10_padRight}";
            string time = $"{DateTime.Now,ConsoleDesign._10_padRight}";

            string log = $"| {action} | {src} | {dst} | {time} |";
            Log(log);
        }
    }
}
