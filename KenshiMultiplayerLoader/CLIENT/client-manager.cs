using KenshiMultiplayerLoader.MODELS;
using KenshiMultiplayerLoader.NETWORK;
using KenshiMultiplayerLoader.UI;
using System;
using System.Threading;

namespace KenshiMultiplayerLoader.CLIENT
{
    public class ClientManager
    {
        private NetworkHandler networkHandler;
        private GameStateSynchronizer stateSynchronizer;
        private UIManager uiManager;
        private bool isConnected = false;
        private string playerId;
        private string sessionId;
        private Thread messageListenerThread;
        private bool shouldListen = false;
        
        public ClientManager()
        {
            networkHandler = new NetworkHandler();
            stateSynchronizer = new GameStateSynchronizer();
            uiManager = new UIManager();
        }
        
        public bool Connect(string serverIP, int port, string username, string password)
        {
            try
            {
                if (isConnected)
                    Disconnect();
                    
                Logger.Log($"Connecting to server {serverIP}:{port}...");
                if (networkHandler.Connect(serverIP, port))
                {
                    // Authenticate with the server
                    sessionId = networkHandler.Authenticate(username, password);
                    if (sessionId != null)
                    {
                        playerId = username;
                        isConnected = true;
                        uiManager.ShowConnectedStatus(true, serverIP);
                        Logger.Log("Connected and authenticated successfully.");
                        
                        // Start message listening thread
                        shouldListen = true;
                        messageListenerThread = new Thread(ListenForMessages);
                        messageListenerThread.IsBackground = true;
                        messageListenerThread.Start();
                        
                        return true;
                    }
                    else
                    {
                        Logger.Log("Authentication failed.");
                        networkHandler.Disconnect();
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Logger.Log($"Connection error: {ex.Message}");
                return false;
            }
        }
        
        public void Disconnect()
        {
            if (isConnected)
            {
                shouldListen = false;
                
                if (messageListenerThread != null && messageListenerThread.IsAlive)
                {
                    try
                    {
                        messageListenerThread.Join(1000); // Wait up to 1 second for thread to finish
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error stopping message thread: {ex.Message}");
                    }
                }
                
                networkHandler.SendLogoutMessage(playerId, sessionId);
                networkHandler.Disconnect();
                isConnected = false;
                sessionId = null;
                uiManager.ShowConnectedStatus(false, null);
                Logger.Log("Disconnected from server.");
            }
        }
        
        public void Update()
        {
            if (isConnected)
            {
                // Update connection status
                if (!networkHandler.IsConnected())
                {
                    HandleDisconnect();
                    return;
                }
                
                // Process message queue
                networkHandler.ProcessMessageQueue();
                
                // Update UI elements
                uiManager.Update();
            }
        }
        
        public void SyncPlayerPosition(float x, float y, float z)
        {
            if (isConnected)
            {
                stateSynchronizer.UpdatePosition(playerId, x, y, z, networkHandler);
            }
        }
        
        public void SyncPlayerHealth(int current, int max)
        {
            if (isConnected)
            {
                stateSynchronizer.UpdateHealth(playerId, current, max, networkHandler);
            }
        }
        
        public void SyncInventoryChange(string itemName, int quantity)
        {
            if (isConnected)
            {
                stateSynchronizer.UpdateInventory(playerId, itemName, quantity, networkHandler);
            }
        }
        
        public void PerformCombatAction(string targetId, string actionType, string weaponId = null)
        {
            if (isConnected)
            {
                stateSynchronizer.SendCombatAction(playerId, targetId, actionType, weaponId, networkHandler);
            }
        }
        
        public void SendChatMessage(string content)
        {
            if (isConnected)
            {
                uiManager.SendChatMessage(content, networkHandler, playerId);
            }
        }
        
        private void ListenForMessages()
        {
            while (shouldListen && isConnected)
            {
                try
                {
                    var message = networkHandler.ReceiveMessage();
                    if (message != null)
                    {
                        HandleGameMessage(message);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error receiving message: {ex.Message}");
                    if (!networkHandler.IsConnected())
                    {
                        break;
                    }
                }
                
                Thread.Sleep(10); // Prevent CPU thrashing
            }
        }
        
        private void HandleGameMessage(GameMessage message)
        {
            try
            {
                switch (message.Type)
                {
                    case MessageType.Position:
                        stateSynchronizer.HandlePositionUpdate(message);
                        break;
                    case MessageType.Inventory:
                        stateSynchronizer.HandleInventoryUpdate(message);
                        break;
                    case MessageType.Combat:
                        stateSynchronizer.HandleCombatAction(message);
                        break;
                    case MessageType.Health:
                        stateSynchronizer.HandleHealthUpdate(message);
                        break;
                    case MessageType.Chat:
                        uiManager.DisplayChatMessage(message);
                        break;
                    case MessageType.WorldState:
                        stateSynchronizer.HandleWorldStateUpdate(message);
                        break;
                    case MessageType.Error:
                        uiManager.DisplayErrorMessage(message);
                        break;
                    case MessageType.Acknowledgment:
                        // Handle acknowledgment
                        break;
                    case MessageType.Notification:
                        uiManager.DisplayNotification(message);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error handling message: {ex.Message}");
            }
        }
        
        private void HandleDisconnect()
        {
            isConnected = false;
            sessionId = null;
            shouldListen = false;
            uiManager.ShowDisconnectedStatus("Connection to server lost");
            Logger.Log("Connection to server lost.");
        }
    }
}