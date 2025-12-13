using KenshiMultiplayer;
using KenshiMultiplayerLoader.MODELS;
using KenshiMultiplayerLoader.UI;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace KenshiMultiplayerLoader.NETWORK
{
    public class NetworkHandler
    {
        private TcpClient client;
        private NetworkStream stream;
        private Queue<GameMessage> messageQueue = new Queue<GameMessage>();
        private readonly object queueLock = new object();
        private int reconnectAttempts = 0;
        private const int maxReconnectAttempts = 5;
        
        public bool Connect(string serverIP, int port)
        {
            try
            {
                client = new TcpClient();
                client.SendTimeout = 5000; // 5 second timeout
                client.ReceiveTimeout = 5000;
                client.NoDelay = true; // Disable Nagle's algorithm for better responsiveness
                
                // Create a connection with timeout
                IAsyncResult result = client.BeginConnect(serverIP, port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(5000, true);
                
                if (!success)
                {
                    client.Close();
                    Logger.Log("Connection attempt timed out");
                    return false;
                }
                
                client.EndConnect(result);
                stream = client.GetStream();
                reconnectAttempts = 0;
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Connection error: {ex.Message}");
                return false;
            }
        }
        
        public void Disconnect()
        {
            try
            {
                if (client != null && client.Connected)
                {
                    stream?.Close();
                    client.Close();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error during disconnect: {ex.Message}");
            }
        }
        
        public bool IsConnected()
        {
            if (client == null)
                return false;
                
            try
            {
                // Check if client is connected
                if (!client.Connected)
                    return false;
                    
                // This is how you can check if a disconnect has occurred
                if (client.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                    {
                        // Client disconnected
                        return false;
                    }
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public bool TryReconnect(string serverIP, int port)
        {
            if (reconnectAttempts >= maxReconnectAttempts)
                return false;
                
            reconnectAttempts++;
            Logger.Log($"Attempting to reconnect (attempt {reconnectAttempts} of {maxReconnectAttempts})...");
            
            Disconnect();
            
            bool success = Connect(serverIP, port);
            if (success)
            {
                Logger.Log("Reconnection successful");
                reconnectAttempts = 0;
            }
            
            return success;
        }
        
        public string Authenticate(string username, string password)
        {
            try
            {
                var authMessage = new GameMessage
                {
                    Type = MessageType.Login,
                    PlayerId = username,
                    Data = new Dictionary<string, object>
                    {
                        { "username", username },
                        { "password", password }
                    }
                };
                
                SendMessage(authMessage);
                
                // Wait for response (timeout after 5 seconds)
                DateTime startTime = DateTime.Now;
                while ((DateTime.Now - startTime).TotalSeconds < 5)
                {
                    var response = ReceiveMessage();
                    if (response != null && response.Type == MessageType.Login)
                    {
                        bool isSuccess = false;
                        if (response.Data.TryGetValue("success", out object success))
                        {
                            // Handle both JsonElement and bool types
                            if (success is JsonElement jsonElement)
                                isSuccess = jsonElement.GetBoolean();
                            else if (success is bool boolValue)
                                isSuccess = boolValue;
                        }

                        if (isSuccess &&
                            response.Data.TryGetValue("sessionId", out object sessionId) &&
                            sessionId != null)
                        {
                            return sessionId.ToString();
                        }
                        else
                        {
                            // Authentication failed
                            return null;
                        }
                    }
                    
                    Thread.Sleep(100);
                }
                
                // Timeout
                return null;
            }
            catch (Exception ex)
            {
                Logger.Log($"Authentication error: {ex.Message}");
                return null;
            }
        }
        
        public void SendLogoutMessage(string playerId, string sessionId)
        {
            var logoutMessage = new GameMessage
            {
                Type = MessageType.Logout,
                PlayerId = playerId,
                SessionId = sessionId
            };
            
            SendMessage(logoutMessage);
        }
        
        public void SendMessage(GameMessage message)
        {
            if (!IsConnected())
            {
                lock (queueLock)
                {
                    messageQueue.Enqueue(message);
                }
                return;
            }
            
            try
            {
                string jsonMessage = message.ToJson();
                string encryptedMessage = EncryptionHelper.Encrypt(jsonMessage);
                byte[] messageBuffer = Encoding.ASCII.GetBytes(encryptedMessage);
                
                // Prepend message length for proper framing
                byte[] lengthPrefix = BitConverter.GetBytes(messageBuffer.Length);
                stream.Write(lengthPrefix, 0, lengthPrefix.Length);
                stream.Write(messageBuffer, 0, messageBuffer.Length);
                stream.Flush();
            }
            catch (Exception ex)
            {
                Logger.Log($"Error sending message: {ex.Message}");
                lock (queueLock)
                {
                    messageQueue.Enqueue(message);
                }
            }
        }
        
        public GameMessage ReceiveMessage()
        {
            if (!IsConnected())
                return null;
                
            try
            {
                if (stream.DataAvailable)
                {
                    // Read message length prefix
                    byte[] lengthBuffer = new byte[4];
                    int bytesRead = stream.Read(lengthBuffer, 0, lengthBuffer.Length);
                    if (bytesRead < 4)
                        return null;
                        
                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);
                    if (messageLength <= 0 || messageLength > 1048576) // Max 1MB message size
                        return null;
                        
                    // Read the actual message
                    byte[] messageBuffer = new byte[messageLength];
                    int totalBytesRead = 0;
                    
                    while (totalBytesRead < messageLength)
                    {
                        int bytesRemaining = messageLength - totalBytesRead;
                        int bytesReadThisTime = stream.Read(messageBuffer, totalBytesRead, bytesRemaining);
                        
                        if (bytesReadThisTime == 0)
                            break; // Connection closed
                            
                        totalBytesRead += bytesReadThisTime;
                    }
                    
                    if (totalBytesRead == messageLength)
                    {
                        string encryptedMessage = Encoding.ASCII.GetString(messageBuffer);
                        string jsonMessage = EncryptionHelper.Decrypt(encryptedMessage);
                        return GameMessage.FromJson(jsonMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Error receiving message: {ex.Message}");
            }
            
            return null;
        }
        
        public void ProcessMessageQueue()
        {
            if (!IsConnected())
                return;
                
            lock (queueLock)
            {
                int processCount = 0;
                while (messageQueue.Count > 0 && processCount < 10) // Process up to 10 messages per frame
                {
                    var message = messageQueue.Dequeue();
                    try
                    {
                        string jsonMessage = message.ToJson();
                        string encryptedMessage = EncryptionHelper.Encrypt(jsonMessage);
                        byte[] messageBuffer = Encoding.ASCII.GetBytes(encryptedMessage);
                        
                        // Prepend message length for proper framing
                        byte[] lengthPrefix = BitConverter.GetBytes(messageBuffer.Length);
                        stream.Write(lengthPrefix, 0, lengthPrefix.Length);
                        stream.Write(messageBuffer, 0, messageBuffer.Length);
                        stream.Flush();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error processing queued message: {ex.Message}");
                        messageQueue.Enqueue(message); // Put the message back in the queue
                        break; // Stop processing if we hit an error
                    }
                    
                    processCount++;
                }
            }
        }
    }
}