using System.Text.Json;
using TacMesh.Core.interfaces.communication;

namespace TacMesh.Core.hardware_mode.communication
{
    /// <summary>
    /// Input listener class to control the IPC messages received from the User
    /// at simulation mode
    /// </summary>
    public class UserControl : IInputListener
    {
        // Constructor 
        public UserControl() { }


        public void ListenForInput()
        {
            // create a dedicated thread for listening for input instead of using a thread from 
            // the thread pool via a Task, because a Console.ReadLine() is a synchronous operation
            // and will block the thread from working in the meantime of waiting for input
            //
            Thread listener = new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        // parse input pushed from the user frontend
                        string input = Console.ReadLine();
                        TryParseStdInput(input);

                        // test received input
                        //Console.WriteLine($"[USER CONTROL] received: \"{input}\"");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"[USER CONTROL] Error: \"{e.Message}\"\n");
                    }
                }
            });
            listener.IsBackground = true; // set as bg thread to make it die when the main thread dies
            listener.Start();
        }

        // --- parsing methods ---

        private void TryParseStdInput(string input)
        {
            if (TryParseUserMessage(input)) return;
        }

        // try to parse as a user message json
        private bool TryParseUserMessage(string input)
        {
            try
            {
                UserMessage? userMessage = JsonSerializer.Deserialize<UserMessage>(input) ?? throw new Exception();

                // TEST print out the user message
                Console.WriteLine($"[USER CONTROL] received: \"{userMessage.Source}\" -> \"{userMessage.Destination}\": \"{userMessage.Message}\"");

                // build and send the packet to the destination

                // to do ...

                return true;
            }
            catch {
                return false;
            }
        }
    }
}
