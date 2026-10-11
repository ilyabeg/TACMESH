using TacMesh.Core.graph_related;
using TacMesh.Core.models;

namespace TacMesh.Core.launcher
{
    public class LauncherControl
    {
        // launcher control is specificaly used only during simuation mode, thus
        // using specificaly the VIRTUAL Radio Model.
        private readonly VirtualRadioModel _radioModel;
        private readonly string _srcNode; // the Node that the Launcher is pushing the data to

        public LauncherControl(VirtualRadioModel radioModel, string srcNode)
        {
            _radioModel = radioModel;
            _srcNode = srcNode;
        }

        // listens for StdInput pushed via the StdInputStream from the Launcher (the scenario manager)
        public void ListenForStdInput()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    // parse input pushed from launcher
                    string input = Console.ReadLine();
                    if (TryParseStdInput(input, out string nodeId, out Location newLocation))
                    {
                        // update the new location received from the 
                        _radioModel.UpdateAgentLocation(nodeId!, newLocation!);
                        Console.WriteLine($"[AGENT LAUNCHER CONTROL] Moved Node <{nodeId!}> to new position: <{newLocation.X},{newLocation.Y}>.\n");
                    }
                }
            });
        }

        // parse launcher position change inputs
        private bool TryParseStdInput(string input, out string? nodeId, out Location? newLocation)
        {
            try
            {
                string[] splitted = input.Split('|');
                if (splitted.Length != 2) throw new ArgumentException("Expected position was invalid");

                // input supposed to simply be 'NodeID|X,Y'
                nodeId = splitted[0];

                // split the 'X,Y' string
                string[] position = splitted[1].Split(',');
                double x = double.Parse(position[0]);
                double y = double.Parse(position[1]);

                // create new pos from the launcher change
                newLocation = new Location(x,y);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't parse StdInput: '{e.Message}'");
                newLocation = null;
                nodeId = null;
                return false;
            }
        }
    }
}
