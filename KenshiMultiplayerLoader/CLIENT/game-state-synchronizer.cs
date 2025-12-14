using KenshiMultiplayerLoader.MODELS;
using KenshiMultiplayerLoader.CLIENT;
using KenshiMultiplayerLoader.NETWORK;
using KenshiMultiplayerLoader.UI;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace KenshiMultiplayerLoader.CLIENT
{
    public class GameStateSynchronizer
    {
        private Dictionary<string, Position> playerPositions = new Dictionary<string, Position>();
        private Dictionary<string, HealthStatus> playerHealth = new Dictionary<string, HealthStatus>();
        private Dictionary<string, Dictionary<string, InventoryItem>> playerInventories = new Dictionary<string, Dictionary<string, InventoryItem>>();
        private Dictionary<string, PlayerState> playerStates = new Dictionary<string, PlayerState>();
        private float positionUpdateThreshold = 0.5f;
        private DateTime lastPositionUpdate = DateTime.MinValue;
        private TimeSpan positionUpdateInterval = TimeSpan.FromMilliseconds(100); // 10 updates per second max
        
        public void UpdatePosition(string playerId, float x, float y, float z, NetworkHandler networkHandler, string sessionId = null)
        {
            // Rate limit position updates
            if (DateTime.Now - lastPositionUpdate < positionUpdateInterval)
                return;

            // Get the player's last known position
            if (!playerPositions.TryGetValue(playerId, out Position lastPosition))
            {
                lastPosition = new Position(0, 0, 0);
                playerPositions[playerId] = lastPosition;
            }

            // Check if position has changed significantly
            float dx = Math.Abs(x - lastPosition.X);
            float dy = Math.Abs(y - lastPosition.Y);
            float dz = Math.Abs(z - lastPosition.Z);

            if (dx > positionUpdateThreshold || dy > positionUpdateThreshold || dz > positionUpdateThreshold)
            {
                var position = new Position(x, y, z);
                var message = new GameMessage
                {
                    Type = MessageType.Position,
                    PlayerId = playerId,
                    SessionId = sessionId,
                    Data = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(position))
                };

                networkHandler.SendMessage(message);

                // Update last known position and timestamp
                playerPositions[playerId] = position;
                lastPositionUpdate = DateTime.Now;
            }
        }
        
        public void UpdateHealth(string playerId, int current, int max, NetworkHandler networkHandler, string sessionId = null)
        {
            // Get the player's last known health
            if (!playerHealth.TryGetValue(playerId, out HealthStatus lastHealth))
            {
                lastHealth = new HealthStatus { CurrentHealth = current, MaxHealth = max };
                playerHealth[playerId] = lastHealth;

                // Always send initial health update
                SendHealthUpdate(playerId, current, max, networkHandler, sessionId);
                return;
            }

            // Check if health has changed
            if (current != lastHealth.CurrentHealth || max != lastHealth.MaxHealth)
            {
                SendHealthUpdate(playerId, current, max, networkHandler, sessionId);

                // Update last known health
                lastHealth.CurrentHealth = current;
                lastHealth.MaxHealth = max;
            }
        }

        private void SendHealthUpdate(string playerId, int current, int max, NetworkHandler networkHandler, string sessionId = null)
        {
            var healthStatus = new HealthStatus { CurrentHealth = current, MaxHealth = max };
            var message = new GameMessage
            {
                Type = MessageType.Health,
                PlayerId = playerId,
                SessionId = sessionId,
                Data = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(healthStatus))
            };

            networkHandler.SendMessage(message);
        }
        
        public void UpdateInventory(string playerId, string itemName, int quantity, NetworkHandler networkHandler, string sessionId = null)
        {
            // Make sure the player's inventory exists
            if (!playerInventories.TryGetValue(playerId, out var inventory))
            {
                inventory = new Dictionary<string, InventoryItem>();
                playerInventories[playerId] = inventory;
            }

            // Create or update the item
            if (!inventory.TryGetValue(itemName, out var item))
            {
                if (quantity <= 0)
                    return; // Don't create an item with zero or negative quantity

                item = new InventoryItem(itemName, quantity);
                inventory[itemName] = item;
            }
            else
            {
                item.Quantity += quantity;
                if (item.Quantity <= 0)
                {
                    inventory.Remove(itemName);
                }
            }

            // Send the update to the server
            var message = new GameMessage
            {
                Type = MessageType.Inventory,
                PlayerId = playerId,
                SessionId = sessionId,
                Data = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(item))
            };

            networkHandler.SendMessage(message);
        }
        
        public void SendCombatAction(string playerId, string targetId, string actionType, string weaponId, NetworkHandler networkHandler, string sessionId = null)
        {
            var combatAction = new CombatAction
            {
                TargetId = targetId,
                Action = actionType,
                WeaponId = weaponId
            };

            // Add player position to combat action for validation
            if (playerPositions.TryGetValue(playerId, out Position pos))
            {
                combatAction.AttackerPosX = pos.X;
                combatAction.AttackerPosY = pos.Y;
                combatAction.AttackerPosZ = pos.Z;
            }

            var message = new GameMessage
            {
                Type = MessageType.Combat,
                PlayerId = playerId,
                SessionId = sessionId,
                Data = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(combatAction))
            };

            networkHandler.SendMessage(message);
        }
        
        // Handle incoming position updates
        public void HandlePositionUpdate(GameMessage message)
        {
            if (message.PlayerId == null || message.Data == null)
                return;
                
            try
            {
                var position = JsonSerializer.Deserialize<Position>(JsonSerializer.Serialize(message.Data));
                
                // Update the position in our local state
                playerPositions[message.PlayerId] = position;
                
                // Apply to game
                ApplyPositionToGame(message.PlayerId, position);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error handling position update: {ex.Message}");
            }
        }
        
        private void ApplyPositionToGame(string playerId, Position position)
        {
            // Here you would implement the code to update the character's position in-game
            // This depends on how Kenshi's character system works
            // For example:
            // KenshiGame.GetCharacter(playerId)?.SetPosition(position.X, position.Y, position.Z);
            
            Logger.Log($"Updated position for {playerId}: {position}");
        }
        
        // Handle incoming inventory updates
        public void HandleInventoryUpdate(GameMessage message)
        {
            if (message.PlayerId == null || message.Data == null)
                return;
                
            try
            {
                var item = JsonSerializer.Deserialize<InventoryItem>(JsonSerializer.Serialize(message.Data));
                
                // Update our local inventory state
                if (!playerInventories.TryGetValue(message.PlayerId, out var inventory))
                {
                    inventory = new Dictionary<string, InventoryItem>();
                    playerInventories[message.PlayerId] = inventory;
                }
                
                if (item.Quantity <= 0)
                {
                    inventory.Remove(item.ItemName);
                }
                else
                {
                    inventory[item.ItemName] = item;
                }
                
                // Apply to game
                ApplyInventoryToGame(message.PlayerId, item);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error handling inventory update: {ex.Message}");
            }
        }
        
        private void ApplyInventoryToGame(string playerId, InventoryItem item)
        {
            // Here you would implement the code to update the character's inventory in-game
            // This depends on how Kenshi's inventory system works
            // For example:
            // KenshiGame.GetCharacter(playerId)?.UpdateInventory(item.ItemName, item.Quantity);
            
            Logger.Log($"Updated inventory for {playerId}: {item.ItemName} x{item.Quantity}");
        }
        
        // Handle incoming health updates
        public void HandleHealthUpdate(GameMessage message)
        {
            if (message.PlayerId == null || message.Data == null)
                return;
                
            try
            {
                var health = JsonSerializer.Deserialize<HealthStatus>(JsonSerializer.Serialize(message.Data));
                
                // Update our local health state
                playerHealth[message.PlayerId] = health;
                
                // Apply to game
                ApplyHealthToGame(message.PlayerId, health);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error handling health update: {ex.Message}");
            }
        }
        
        private void ApplyHealthToGame(string playerId, HealthStatus health)
        {
            // Here you would implement the code to update the character's health in-game
            // This depends on how Kenshi's health system works
            // For example:
            // KenshiGame.GetCharacter(playerId)?.SetHealth(health.CurrentHealth, health.MaxHealth);
            
            Logger.Log($"Updated health for {playerId}: {health.CurrentHealth}/{health.MaxHealth}");
        }
        
        // Handle incoming combat actions
        public void HandleCombatAction(GameMessage message)
        {
            if (message.PlayerId == null || message.Data == null)
                return;
                
            try
            {
                var action = JsonSerializer.Deserialize<CombatAction>(JsonSerializer.Serialize(message.Data));
                
                // Apply to game
                ApplyCombatActionToGame(message.PlayerId, action);
            }
            catch (Exception ex)
            {
                Logger.Log($"Error handling combat action: {ex.Message}");
            }
        }
        
        private void ApplyCombatActionToGame(string playerId, CombatAction action)
        {
            // Here you would implement the code to apply the combat action in-game
            // This depends on how Kenshi's combat system works
            // For example:
            // KenshiGame.GetCharacter(playerId)?.PerformCombatAction(action.TargetId, action.Action, action.WeaponId);
            
            Logger.Log($"Combat action from {playerId}: {action.Action} on {action.TargetId}");
        }
        
        // Handle world state updates (for syncing NPCs, environment, etc.)
        public void HandleWorldStateUpdate(GameMessage message)
        {
            if (message.Data == null)
                return;
                
            try
            {
                // World state updates could contain various types of data
                // This is a placeholder for implementing specific world state synchronization
                Logger.Log("Received world state update");
            }
            catch (Exception ex)
            {
                Logger.Log($"Error handling world state update: {ex.Message}");
            }
        }
    }
}