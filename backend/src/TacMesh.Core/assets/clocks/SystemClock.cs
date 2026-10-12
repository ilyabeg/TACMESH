using TacMesh.Core.interfaces.clock_interfaces;

namespace TacMesh.Core.assets.clocks
{
    public class SystemClock : IClock
    {
        public DateTime GetTime() => DateTime.Now; // return current System's time
    }
}
