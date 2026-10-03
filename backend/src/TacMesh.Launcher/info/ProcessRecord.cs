namespace TacMesh.Launcher.info
{
    /// <summary>
    /// Class to easily hold each node's record
    /// </summary>
    public class ProcessRecord
    {
        // record fields
        public string NodeId { get; private set; }
        public string ProcessId { get; private set; }
        public int Port { get; private set; }
        public ProcessRecord(string nodeId, string pId, int port)
        {
            NodeId = nodeId;
            ProcessId = pId;
            Port = port;
        }
    }
}
