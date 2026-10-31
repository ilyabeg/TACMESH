using System.Text.Json;
using System.Text.Json.Serialization;
using TacMesh.Core.assets.node;

namespace TacMesh.Launcher
{
    // define the structure for parsing the configurations command sent fromt ui process
    //
    public class ConfigsMessage
    {
        // tell json to include private setter fields
        [JsonInclude]
        public string NodeId { get; private set; }
        [JsonInclude]
        public NodeConfigs Configs { get; private set; }

        // empty constructor for json deserialization
        public ConfigsMessage() { }
        public ConfigsMessage(string nodeId, NodeConfigs nodeConfigs) 
        {
            NodeId = nodeId;
            Configs = nodeConfigs;
        }
    }


    // defines the ipc nessage types sent from the launcher to the main process
    // throught the std io stream pipes, and vice versa
    //
    public enum IOMessageType
    {
        // Launcher -> StdOutput stream
        ReturnEngines,
        EngineUp,
        EngineDown,
        EngineUpdate,
        ConfigsAck,

        // StdInput stream -> Launcher
        ListEngines,
        KillEngine,
        StartEngine,
        MoveEngine,
        AdvanceEngine,
        EngineConfigs,
    }

    // class that represents the Json data sent from the Launcher to the main electron
    // process (and vice versa), through the Std IO stream pipes. defines the ipc messages
    //
    public class StdIOMessage
    {
        // the type of message sent to determine the data (as a string for json serialization)
        public string Type { get; set; }

        // the message data/payload which can be any type of object.
        // it is saved as a json element to provide easy generic serialization
        // and deserialization at any time for any type of object
        //
        public JsonElement Payload { get; set; }


        // constructors
        public StdIOMessage() { } // empty constructor for json deserialization

        public StdIOMessage(IOMessageType type, object payload)
        {
            Type = type.ToString(); // convert to string before saving
            Payload = JsonSerializer.SerializeToElement(payload); // save the payload as a json element
        }
    }
}
