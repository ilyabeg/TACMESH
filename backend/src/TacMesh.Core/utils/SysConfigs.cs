using System.Net;

namespace TacMesh.Core.utils
{
    public class SysConfigs
    {       
        // VERY IMPORTANT: TEMPORARILY MAGIC NUMBER!! WILL CHANGE IN THE FUTURE TO GRAB FROM CONFIGURATIONS FILE
        public static int HeartbeatDelayMs = 5000;

        // TEMPORARILY USE THESE MAGIC NUMBERS FOR BINDING AND ADDING THE RADIO SOCKET TO THE
        // MCAST GROUP. WILL BE FIXED. UNTILL I ADD A CONFIGURATIONS OR SETTINGS FILE OR JUST
        // DECIDE WHERE TO KEEP THESE AS CONSTANTS, I WILL USE THIS FOR TESTING THE VRadioModel
        public const int McastPort = 51995;
        public static readonly IPAddress McastGroupIp = IPAddress.Parse("239.0.0.1");
        public static readonly IPEndPoint McastEndPoint = new IPEndPoint(McastGroupIp, McastPort);
    }
}
