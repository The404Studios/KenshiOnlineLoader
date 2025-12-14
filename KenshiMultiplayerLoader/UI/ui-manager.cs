using KenshiMultiplayerLoader.MODELS;
using KenshiMultiplayerLoader.NETWORK;
using System;
using System.Collections.Generic;

namespace KenshiMultiplayerLoader.UI
{
    public class UIManager
    {
        private List<ChatMessage> chatMessages = new List<ChatMessage>();
        private int maxChatMessages = 50;
        private bool isConnected = false;
        private string serverInfo = "";
        private string disconnectReason = "";
        private List<string> notificationMessages = new List<string>();
        private int maxNotifications = 5;
        private Dictionary<string, PlayerInfo> playerInfos = new Dictionary<string, PlayerInfo>();
        
        // Call this from your UI rendering code
        public void Update()
        {
            // This method would be called from the main game loop to update any UI elements
            // Since we don't have direct access to Kenshi's UI system, this is a placeholder
            // that would be implemented based on how you're integrating with Kenshi's UI
            
            // Example implementation:
            // 1. Update connection status UI
            // 2. Update chat window
            // 3. Update player list
            // 4. Process UI inputs
        }
        
        public void ShowConnectedStatus(bool connected, string serverIP)
        {
            isConnected = connected;
            serverInfo = serverIP;
            
            if (connected)
            {
                Logger.Log($"UI: Connected to {serverIP}");
                notificationMessages.Add($"Connected to server: {serverIP}");
                TrimNotifications();
            }
            else
            {
                Logger.Log("UI: Disconnected");
                notificationMessages.Add("Disconnected from server");
                TrimNotifications();
            }
        }
        
        public void ShowDisconnectedStatus(string reason)
        {
            isConnected = false;
            disconnectReason = reason;
            
            Logger.Log($"UI: Disconnected - {reason}");
            notificationMessages.Add($"Disconnected: {reason}");
            TrimNotifications();
        }
        
        public void DisplayChatMessage(GameMessage message)
        {
            if (message.Type != MessageType.Chat || 
                !message.Data.TryGetValue("message", out object chatContent))
                return;
            
            // Ensure chatContent is not null
            string content = chatContent?.ToString() ?? ""; 
                
            var chatMessage = new ChatMessage
            {
                PlayerId = message.PlayerId,
                Content = content,
                Timestamp = DateTime.Now
            };
            
            chatMessages.Add(chatMessage);
            
            // Limit the number of chat messages
            if (chatMessages.Count > maxChatMessages)
            {
                chatMessages.RemoveAt(0);
            }
            
            Logger.Log($"Chat: [{chatMessage.PlayerId}] {chatMessage.Content}");
        }
        
        public void DisplayErrorMessage(GameMessage message)
        {
            if (!message.Data.TryGetValue("error", out object errorContent))
                return;

            string errorMessage = errorContent?.ToString() ?? "";
            Logger.Log($"Error: {errorMessage}");
            notificationMessages.Add($"Error: {errorMessage}");
            TrimNotifications();
        }
        
        public void DisplayNotification(GameMessage message)
        {
            if (!message.Data.TryGetValue("message", out object notificationContent))
                return;

            string notification = notificationContent?.ToString() ?? "";
            Logger.Log($"Notification: {notification}");
            notificationMessages.Add(notification);
            TrimNotifications();
        }
        
        private void TrimNotifications()
        {
            // Keep only the most recent notifications
            while (notificationMessages.Count > maxNotifications)
            {
                notificationMessages.RemoveAt(0);
            }
        }
        
        public void SendChatMessage(string content, NetworkHandler networkHandler, string playerId, string sessionId = null)
        {
            if (string.IsNullOrWhiteSpace(content))
                return;

            var message = new GameMessage
            {
                Type = MessageType.Chat,
                PlayerId = playerId,
                SessionId = sessionId,
                Data = new Dictionary<string, object>
                {
                    { "message", content }
                }
            };

            networkHandler.SendMessage(message);
        }
        
        public void UpdatePlayerInfo(string playerId, PlayerInfo info)
        {
            playerInfos[playerId] = info;
        }
        
        public void RemovePlayerInfo(string playerId)
        {
            playerInfos.Remove(playerId);
        }
        
        // The following methods would be called by your Kenshi UI integration code
        
        // Show connection dialog
        public void ShowConnectionDialog()
        {
            // This would display a connection dialog in Kenshi's UI
            // Since we don't have direct access to Kenshi's UI system, this is a placeholder
        }
        
        // Show chat window
        public void ShowChatWindow()
        {
            // This would display the chat window in Kenshi's UI
            // Since we don't have direct access to Kenshi's UI system, this is a placeholder
        }
        
        // Show player list
        public void ShowPlayerList()
        {
            // This would display the player list in Kenshi's UI
            // Since we don't have direct access to Kenshi's UI system, this is a placeholder
        }
        
        // Get chat messages for rendering
        public List<ChatMessage> GetChatMessages()
        {
            return chatMessages;
        }
        
        // Get notifications for rendering
        public List<string> GetNotifications()
        {
            return notificationMessages;
        }
        
        // Get connection status for rendering
        public (bool isConnected, string serverInfo, string disconnectReason) GetConnectionStatus()
        {
            return (isConnected, serverInfo, disconnectReason);
        }
        
        // Get player list for rendering
        public Dictionary<string, PlayerInfo> GetPlayerList()
        {
            return playerInfos;
        }
    }
    
    public class ChatMessage
    {
        public string PlayerId { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    public class PlayerInfo
    {
        public string PlayerId { get; set; }
        public string DisplayName { get; set; }
        public int Level { get; set; }
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public string FactionName { get; set; }
        public Position Position { get; set; }
        public PlayerState State { get; set; }
    }
}