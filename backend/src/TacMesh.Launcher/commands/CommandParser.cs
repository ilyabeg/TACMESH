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
                Console.WriteLine("[COMMAND PARSER] Enter a command at any time [list / start <id> / kill <id> / pos <id> <X,Y>]:\n");
                string command = Console.ReadLine();

                // if Ctrl+C was pressed, 'command' initiates to null, which causes an inifite loop.
                // this is the purpose of this check: to end and break out of the loop
                if (command == null || command.Trim().ToLower().Equals("exit")) 
                    break;
                
                if (string.IsNullOrWhiteSpace(command)) continue;

                ParseCommand(command.Trim());
            }
        }

        // constant command indexes
        private const int _command_index = 0;
        private const int _id_index = 1;
        private const int _pos_index = 2;

        // parse command with switch case - KISS principle
        // (no need to over-engineer with memory heavy dictionary for 4 simple commands)
        private void ParseCommand(string str)
        {
            try
            {
                string[] splitted = str.Split(' ');
                string command = splitted[_command_index].ToLower();


                // Note: TEMPORARILY start the node position advances here by typing
                // '.' on the keyboard until an automated and different 
                // implementation of handling node advancements is written...
                if (command.Equals(".")) {
                    _engineManager.InitAdvances();
                    return;
                }


                switch (command)
                {
                    case "list":
                    {
                        // 'list' command should be a single command word
                        if (splitted.Length != 1) throw new Exception();

                        _engineManager.PrintEngines();
                        break;
                    }
                    case "kill":
                    {
                        // 'kill' command should be a single command word and id only
                        if (splitted.Length != 2) throw new Exception();

                        string id = splitted[_id_index];
                        _engineManager.KillEngine(id);
                        break;
                    }
                    case "start":
                    {
                        // 'start' command should be a single command word and id only 
                        if (splitted.Length != 2) throw new Exception();

                        string id = splitted[_id_index];
                        _engineManager.StartEngine(id);
                        break;
                    }
                    case "pos":
                    {
                        // 'pos' command should be only 3 parts
                        if (splitted.Length > 3) throw new Exception();

                        string id = splitted[_id_index];
                        string pos = splitted[_pos_index];

                        _engineManager.MoveEngine(id, pos);
                        break;
                    }
                    default: throw new Exception();
                }
            }
            catch
            {
                Console.WriteLine("[COMMAND PARSER] Unkown command.");
            }            
        }
    }
}
