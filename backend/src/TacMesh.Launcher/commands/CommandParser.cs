using System.Text.Json;
using TacMesh.Core.utils.ipc.types;
using TacMesh.Launcher.info;
using TacMesh.Launcher.startup;

namespace TacMesh.Launcher.commands
{
    /// <summary>
    /// Command Parser class to handle user input and execute the right command
    /// </summary>
    public class CommandParser
    {
        // engine manager to execute command on
        private readonly EngineManager _engineManager;

        // constructor
        public CommandParser(EngineManager engineManager)
        {
            _engineManager = engineManager;
        }

        // start parsing loop
        public void StartParsing()
        {
            while (true)
            {
                //Console.WriteLine("[COMMAND PARSER] Enter a command at any time [list / start <id> / kill <id> / pos <id> <X,Y>]:\n");
                string input = Console.ReadLine();

                // if Ctrl+C was pressed, 'input' initiates to null, which causes an inifite loop.
                // this is the purpose of this check: to end and break out of the loop
                if (input == null || input.Trim().ToLower().Equals("exit")) 
                    break;
                
                if (string.IsNullOrWhiteSpace(input)) continue;

                TryParseInput(input.Trim());
            }
        }


        // parse input with switch case - KISS principle
        // (no need to over-engineer with memory heavy dictionary for each command)
        private void TryParseInput(string input)
        {
            try
            {
                // test the input receiving...
                Console.WriteLine($"[LAUNCHER RECEIVED] input: \"{input}\"");


                // deserialize as an ipc input message
                StdIOMessage msg = JsonSerializer.Deserialize<StdIOMessage>(input)!;
                IOMessageType msgType = Enum.Parse<IOMessageType>(msg.Type); // parse the message type as an enum


                // test the message type...
                Console.WriteLine($"[LACUNHER] Received IPC Message type: {msgType} ({msg.Type})");


                switch (msgType)
                {
                    // main-process request to get the running engine processes
                    case IOMessageType.ListEngines:
                    {
                        List<ProcessRecord> enginesList = _engineManager.GetEnginesList();
                        StdIOMessage outputMessage = new StdIOMessage(IOMessageType.ReturnEngines, enginesList);

                        // print out the json message
                        string jsonMessage = JsonSerializer.Serialize(outputMessage);
                        Console.WriteLine(jsonMessage);
                        break;
                    }
                    // treminates the specified engine process
                    case IOMessageType.KillEngine:
                    {
                        string engineId = JsonSerializer.Deserialize<string>(msg.Payload)!;
                        _engineManager.KillEngine(engineId);
                        break;
                    }
                    // starts the specified engine process
                    case IOMessageType.StartEngine:
                    {
                        string engineId = JsonSerializer.Deserialize<string>(msg.Payload)!;
                        _engineManager.StartEngine(engineId);
                        break;
                    }
                    // moves the specified engine process
                    case IOMessageType.MoveEngine:
                    {
                        MoveCommand data = JsonSerializer.Deserialize<MoveCommand>(msg.Payload)!;
                        _engineManager.MoveEngine(data.EngineId, data.Location);
                        break;
                    }
                    // start the node position advances
                    case IOMessageType.AdvanceEngine:
                    {
                        _engineManager.InitAdvances();
                        break;                    
                    }
                    // change the specified engine process link range
                    case IOMessageType.ChgLinkRange:
                    {

                        // to do ...

                        break;
                    }
                    // unrecognized command input
                    default: throw new Exception();
                }
            }
            catch {
                Console.WriteLine("[COMMAND PARSER] Unkown command.");
            }            
        }
    }
}
