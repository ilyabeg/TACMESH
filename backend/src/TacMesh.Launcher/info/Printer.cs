using System.Collections.Concurrent;
using System.Text;

namespace TacMesh.Launcher.info
{
    // Printer class to print each nodes messages in a fixed format
    // because printing from each nodes output corrupts the visual output
    public class Printer
    {
        // keep all output lines for each engine
        private static readonly ConcurrentDictionary<string, Queue<string>> _output_lines = new();

        // lock to allow only one thread at a time to print out the lines
        private static readonly object _lock = new object();


        // appends the new line to the engine's output lines
        public static void AddNewLine(string engineId, string new_line)
        {
            // gets or creates a new empty queue of strings (if doesn't exist yet) and adds the new line to it
            Queue<string> engine_lines = _output_lines.GetOrAdd(engineId, new Queue<string>());
            engine_lines.Enqueue(new_line);

            // if empty line, print everything
            if (new_line.Trim().Equals(""))
                PrintLines(engineId);
        }

        // prints all of the outputed lines of the engine at once to the console
        private static void PrintLines(string engineId)
        {
            lock (_lock)
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine($"[{engineId}]:");

                foreach (string line in _output_lines[engineId])
                    str.AppendLine(line);

                // print and clear list of lines
                Console.Write(str);
                _output_lines[engineId].Clear();
            }
        }
    }
}
