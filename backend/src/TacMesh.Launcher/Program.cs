using System.Diagnostics;
using TacMesh.Launcher.commands;
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

            // parse N from args
            int numOfAgents = int.Parse(args[0]);

            // inject agents created
            Dictionary<string, Process> agents = ProcessCreator.CreateAgents(numOfAgents);
            AgentManager agentManager = new AgentManager(agents);
            agentManager.StartAgents();

            // inject agent manager
            CommandParser commandParser = new CommandParser(agentManager);

            // kill processes when Ctrl+C is pressed
            Console.CancelKeyPress += (s, e) => myHandler(s,e);
            Console.WriteLine("[LAUNCHER] Press Ctrl+C to end.\n");
            commandParser.StartParsing();

            // shutdown on Ctrl+C because it breaks the main thread out of the command parse loop
            agentManager.Shutdown();
        }

        // don't kill main process if Ctrl+C was pressed just yet
        static void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            // ConsoleCancelEventArgs.Cancel = true
            // The value of the Cancel property indicates whether the current
            // process should resume when the event handler concludes
            args.Cancel = true;
        }      
    }
}
