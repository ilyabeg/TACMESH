using System.Net;
using System.Text.Json;
using TacMesh.Core.graph_related;

namespace TacMesh.Core.utils.configurations
{
    public class SystemConfigurations
    {
        // VERY IMPORTANT: TEMPORARILY MAGIC NUMBER!! WILL CHANGE IN THE FUTURE TO GRAB FROM CONFIGURATIONS FILE
        public static int HeartbeatDelayMs = 5000;


        // TEMPORARILY SAVE STATIC SCENARIO NODE POSITIONS HERE SO IT'S EASIER TO PULL IN
        // NODE CLASS FOR TESTING PURPOSES, THE POSITIONS AREN'T INTENDED IN STAYING
        // STATIC OR SAVED THIS WAY. WILL BE FIXED IN THE FUTURE.
        public static Dictionary<string, Location> NodePositions { get; private set; }

        // TEMPORARILY USE THESE MAGIC NUMBERS FOR BINDING AND ADDING THE RADIO SOCKET TO THE
        // MCAST GROUP. WILL BE FIXED. UNTILL I ADD A CONFIGURATIONS OR SETTINGS FILE OR JUST
        // DECIDE WHERE TO KEEP THESE AS CONSTANTS, I WILL USE THIS FOR TESTING THE VRadioModel
        public const int McastPort = 51995;
        public static readonly IPAddress McastGroupIp = IPAddress.Parse("239.0.0.1");
        public static readonly IPEndPoint McastEndPoint = new IPEndPoint(McastGroupIp, McastPort);


        // loads provided configs to memory
        public static void LoadConfigurations(string filePath)
        {
            // read the json and deserialize
            string scenario_positions = File.ReadAllText(filePath);

            // if positions are null throw exception
            NodePositions = JsonSerializer.Deserialize<Dictionary<string, Location>>(scenario_positions) 
                ?? throw new ArgumentNullException("Scenario file can't be empty.");
        }
    }
}
