using System.Collections.Concurrent;
using System.Diagnostics;
using TacMesh.Launcher.processes;
using TacMesh.Launcher.startup;

namespace TacMesh.Launcher 
{
    public class Program
    {
        // Ctrl+c event handler
        public static event ConsoleCancelEventHandler? CancelKeyPress;

        static void Main(string[] args)
        {
            // color output for better visibility
            Console.ForegroundColor = ConsoleColor.Green;

            // reject execution without arguments
            if (args.Length < 1)
                throw new ArgumentException("[LAUNCHER] Error: Expected to get Number of Nodes as an Argument.");
            Console.WriteLine("[LAUNCHER] Launching Agent Processes...");

            // arrange everything
            int numOfAgents = int.Parse(args[0]);

            Dictionary<string, Process> agents = ProcessCreator.CreateAgents(numOfAgents);
            TerminalManager terminal = new TerminalManager(agents);
            terminal.StartAgents();

            // kill processes when Ctrl+C is pressed
            Console.CancelKeyPress += (s, e) => myHandler(s, e, terminal);
            Console.WriteLine("[LAUNCHER] Press ENTER to end.\n");
            Console.ReadLine();

            // shutdown processes on exit
            terminal.Shutdown();
        }

        // shutdown processess if Ctrl+C was pressed
        static void myHandler(object sender, ConsoleCancelEventArgs args, TerminalManager terminal)
        {
            args.Cancel = true;
            terminal.Shutdown();
        }      
    }
}
