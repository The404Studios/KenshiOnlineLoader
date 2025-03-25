using KenshiMultiplayerLoader.CLIENT;
using KenshiMultiplayerLoader.UI;
using System;
using System.IO;
using System.Reflection;

namespace KenshiMultiplayerLoader.CLIENT
{
    public class KenshiMultiplayerLoader
    {
        private static ClientManager clientManager;

        public static void Initialize()
        {
            try
            {
                // Setup logging
                string logPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "kenshi_mp_log.txt");
                Logger.Initialize(logPath);
                Logger.Log("Kenshi Multiplayer mod initializing...");

                // Initialize the client manager
                clientManager = new ClientManager();

                // Register hooks into Kenshi's game loop
                RegisterGameHooks();

                Logger.Log("Kenshi Multiplayer mod initialized successfully.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Failed to initialize Kenshi Multiplayer mod: {ex.Message}");
                Logger.Log(ex.StackTrace);
            }
        }

        private static void RegisterGameHooks()
        {
            // Here you would use your preferred modding API to hook into Kenshi's game loop
            // This depends on what modding framework you're using (e.g., FCS, direct hooks, etc.)

            // Example hooks (pseudocode):
            // KenshiGame.OnUpdate += OnGameUpdate;
            // KenshiGame.OnPlayerMove += OnPlayerMove;
            // KenshiGame.OnGameSave += OnGameSave;
            // KenshiGame.OnGameLoad += OnGameLoad;
        }

        // Called when Kenshi's game loop updates
        private static void OnGameUpdate()
        {
            clientManager.Update();
        }

        // Called when player moves in-game
        private static void OnPlayerMove(float x, float y, float z)
        {
            clientManager.SyncPlayerPosition(x, y, z);
        }

        // Called when player's health changes
        private static void OnPlayerHealthChange(int current, int max)
        {
            clientManager.SyncPlayerHealth(current, max);
        }

        // Called when player's inventory changes
        private static void OnInventoryChange(string itemName, int quantity)
        {
            clientManager.SyncInventoryChange(itemName, quantity);
        }

        // Called when player performs a combat action
        private static void OnCombatAction(string targetId, string actionType, string weaponId)
        {
            clientManager.PerformCombatAction(targetId, actionType, weaponId);
        }

        // Called when the game is exiting
        public static void OnGameExit()
        {
            clientManager.Disconnect();
            Logger.Log("Kenshi Multiplayer mod shutting down.");
        }

        // Public interface for UI/console commands
        public static bool ConnectToServer(string serverIP, int port, string username, string password)
        {
            return clientManager.Connect(serverIP, port, username, password);
        }

        public static void DisconnectFromServer()
        {
            clientManager.Disconnect();
        }

        public static void SendChatMessage(string message)
        {
            clientManager.SendChatMessage(message);
        }
    }
}