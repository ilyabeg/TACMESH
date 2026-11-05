using System.Text.Json.Serialization;

namespace TacMesh.Core.hardware_mode.communication
{
    /// <summary>
    /// Define the UserMessage structure to parse and deserialize the json IPC messages
    /// </summary>
    public class UserMessage
    {
        [JsonInclude]
        public string Source { get; private set; }
        [JsonInclude]
        public string Destination { get; private set; }
        [JsonInclude]
        public string Message { get; private set; }


        // TEMP TEST add the logical clock to test out packet drops 

        [JsonInclude]
        public long LogicalClock { get; private set; }


        // empty constructor for json recognition
        public UserMessage() { }
    }
}
