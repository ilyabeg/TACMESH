using TacMesh.Launcher.interfaces;

namespace TacMesh.Launcher.commands
{
    /// <summary>
    /// Command to kill any process by their ID
    /// </summary>
    public class KillCommand : ICommand
    {
        private string _agentId;

        public KillCommand(string agentId)
        {
            _agentId = agentId;
        }

        public void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
