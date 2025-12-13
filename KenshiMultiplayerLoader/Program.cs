using KenshiMultiplayerLoader.CLIENT;
using KenshiMultiplayerLoader.UI;
using System;

namespace KenshiMultiplayer
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Kenshi Multiplayer Client Starting...");
                
                // Initialize the logger first
                string logPath = "kenshi_mp_client.log";
                Logger.Initialize(logPath);
                Logger.Log("Application started");
                
                // Parse command line arguments if any
                string serverIP = "127.0.0.1";  // Default to localhost
                int serverPort = 5555;          // Default port
                string username = "Player1";    // Default username
                
                if (args.Length >= 1) serverIP = args[0];
                if (args.Length >= 2) int.TryParse(args[1], out serverPort);
                if (args.Length >= 3) username = args[2];
                
                // Initialize the client manager
                ClientManager clientManager = new ClientManager();
                
                // Command loop
                bool running = true;
                while (running)
                {
                    Console.WriteLine("\nCommands:");
                    Console.WriteLine("1. Connect to server");
                    Console.WriteLine("2. Disconnect");
                    Console.WriteLine("3. Send chat message");
                    Console.WriteLine("4. Update position (test)");
                    Console.WriteLine("5. Perform combat action (test)");
                    Console.WriteLine("6. Exit");
                    Console.Write("\nEnter command: ");
                    
                    string input = Console.ReadLine();
                    
                    switch (input)
                    {
                        case "1":
                            Console.Write("Server IP (default 127.0.0.1): ");
                            string ip = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(ip)) ip = "127.0.0.1";

                            Console.Write("Server Port (default 5555): ");
                            string portStr = Console.ReadLine();
                            int port = 5555;
                            if (!string.IsNullOrWhiteSpace(portStr))
                                int.TryParse(portStr, out port);
                            
                            Console.Write("Username: ");
                            string user = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(user)) user = "Player1";
                            
                            Console.Write("Password: ");
                            string pass = Console.ReadLine();
                            if (string.IsNullOrWhiteSpace(pass)) pass = "password";
                            
                            bool success = clientManager.Connect(ip, port, user, pass);
                            if (success)
                                Console.WriteLine("Connected successfully!");
                            else
                                Console.WriteLine("Connection failed.");
                            break;
                            
                        case "2":
                            clientManager.Disconnect();
                            Console.WriteLine("Disconnected from server.");
                            break;
                            
                        case "3":
                            Console.Write("Enter chat message: ");
                            string message = Console.ReadLine();
                            clientManager.SendChatMessage(message);
                            break;
                            
                        case "4":
                            Console.Write("Enter X position: ");
                            if (!float.TryParse(Console.ReadLine(), out float x))
                                x = 0;

                            Console.Write("Enter Y position: ");
                            if (!float.TryParse(Console.ReadLine(), out float y))
                                y = 0;

                            Console.Write("Enter Z position: ");
                            if (!float.TryParse(Console.ReadLine(), out float z))
                                z = 0;

                            clientManager.SyncPlayerPosition(x, y, z);
                            Console.WriteLine($"Position updated to ({x}, {y}, {z})");
                            break;
                            
                        case "5":
                            Console.Write("Enter target ID: ");
                            string targetId = Console.ReadLine() ?? "target1";
                            
                            Console.Write("Enter action type (attack, block, etc.): ");
                            string actionType = Console.ReadLine() ?? "attack";
                            
                            Console.Write("Enter weapon ID (optional): ");
                            string weaponId = Console.ReadLine();
                            
                            clientManager.PerformCombatAction(targetId, actionType, weaponId);
                            Console.WriteLine($"Combat action '{actionType}' performed on '{targetId}'");
                            break;
                            
                        case "6":
                            running = false;
                            clientManager.Disconnect();
                            Console.WriteLine("Exiting application...");
                            break;
                            
                        default:
                            Console.WriteLine("Unknown command. Please try again.");
                            break;
                    }
                    
                    // Process network messages
                    clientManager.Update();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Logger.Error($"Fatal error: {ex.Message}");
                Logger.Debug(ex.StackTrace);
            }
            
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}