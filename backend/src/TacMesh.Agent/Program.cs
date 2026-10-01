using TacMesh.Core;
using TacMesh.Core.builders;

namespace TacMesh.Agent
{
    public class Program
    { 
        static void Main(string[] args)
        {
            //if (args.Length != 2)
            //{
            //    Console.WriteLine("ERROR: Agent Process arguments did not match. Terminating process...");
            //    return;
            //}

            Node node = new Node("NodeA"); // init with node id (args[0])
            node.Test();
            Console.ReadKey();
        }
    }
}
