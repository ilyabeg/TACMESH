namespace TacMesh.Core.custom_events
{
    /// <summary>
    /// Event arguments for the ReceiveError event
    /// </summary>
    public class CrashEventArgs : EventArgs
    {
        public Exception Exception { get; }

        public CrashEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}
