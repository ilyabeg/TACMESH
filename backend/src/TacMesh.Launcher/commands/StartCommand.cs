using TacMesh.Launcher.interfaces;

namespace TacMesh.Launcher.commands
{
    /// <summary>
    /// Command to start any process by their ID
    /// </summary>
    public class StartCommand : ICommand
    {
        private string _agentId;

        public StartCommand(string agentId)
        {
            _agentId = agentId;
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
