namespace TacMesh.Core.interfaces.data_interfaces
{
    // Generic Logger Interface in case I build other types of loggers in the future

    /// <summary>
    /// Logger Interface to log information of type T.
    /// </summary>
    /// <typeparam name="T">The kind of information to log</typeparam>
    public interface ILogger<T>
    {
        List<T> Logs { get; }
        void Log(T data);
        void PrintLogs();
    }
}
