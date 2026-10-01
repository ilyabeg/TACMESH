using System.Text;

namespace TacMesh.Core.database_related
{
    // Logging mode to determine if Sent/Received 
    public enum LoggingMode
    {
        Sent,
        Received
    }

    public class Logger
    {
        // VERY IMPORTANT NOTE: THIS IS A TEMPORARY IMPLEMENTATION OF A LOGGER CLASS,
        // I AM AWARE THAT THIS KIND OF STATIC LIST IS NOT A GOOD IDEA AND IS THREATENED
        // BY RACE CONNDITIONS + USING A LOCK IS NOT IDEAL AND WILL CAUSE BOTTLENECKS IF ALOT
        // PACKETS ARE SENT SIMULTANEOUSLY, I WILL FIX THIS, BUT ONLY FOR TEMPORARY TESING I
        // WILL USE THIS FOR NOW.
        public static List<string> Logs { get; private set; } = new List<string>();
        private static readonly object _lock = new object();
        
        public static void LogHeartbeat(LoggingMode mode, string sender, string receiver, DateTime time)
        {
            lock (_lock)
            {
                string m = (mode == LoggingMode.Sent) ? "Sent" : "Received";
                Logs.Add($"| {m,-8} | {sender,-10} | {receiver,-10} | {time,-10} |");
            }
        }

        // NO LOCK NEEDED BECAUSE I PRINT FROM THE MAIN THREAD AFTER THE HEARTBEATS ARE DONE IN THE EXAMPLE CODE
        public static void PrintLogs()
        {
            // ai design...
            StringBuilder str = new StringBuilder();

            str.AppendLine("\n┌──────────────────────────────────────────────────────────┐");

            string title = $" HEARTBEAT LOGS (Total: {Logs.Count})";
            str.AppendLine($"│{title.PadRight(58)}│");

            str.AppendLine("├──────────┬────────────┬────────────┬─────────────────────┤");
            str.AppendLine("│ ACTION   │ SOURCE     │ DEST       │ TIMESTAMP           │");
            str.AppendLine("├──────────┼────────────┼────────────┼─────────────────────┤");

            foreach (string log in Logs)
                str.AppendLine(log);

            str.AppendLine("└──────────┴────────────┴────────────┴─────────────────────┘");

            Console.WriteLine(str);
        }
    }
}
