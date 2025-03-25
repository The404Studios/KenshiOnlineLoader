This mod adds multiplayer functionality to Kenshi, allowing players to connect to a server and play together in the same world.
Features

Player position synchronization
Health synchronization
Inventory synchronization
Combat action synchronization
In-game chat
Multiplayer notifications
Player list with status information

How It Works
The mod uses a client-server architecture where each player runs a client that connects to a central server. The server manages the world state and synchronizes information between all connected clients.
Architecture Overview

KenshiMultiplayerLoader: The main entry point for the mod that initializes all components.
ClientManager: Handles communication with the server, including authentication, sending updates, and receiving updates from other players.
GameIntegration: Interacts directly with the Kenshi game process to read and modify game state.
GameStateSynchronizer: Manages the synchronization of game state between the client and server.
UIManager: Handles the user interface elements for chat, player list, and notifications.
NetworkHandler: Provides low-level networking functionality for sending and receiving messages.

Technical Details
Game Integration
The mod integrates with Kenshi by:

Memory Reading: Reading player position, health, and inventory data directly from the game's memory.
Input Handling: Capturing and processing chat commands and other mod-specific inputs.
Overlay Rendering: Displaying chat, player list, and notifications on top of the game UI.

Network Protocol
Communication between the client and server uses a secure TCP connection with:

Encryption: All messages are encrypted using AES-256 encryption.
Message Framing: Messages include length prefixes to ensure proper message boundaries.
Message Types: Different types of messages for position updates, combat actions, chat, etc.
Authentication: Secure user authentication to prevent unauthorized access.

Installation

Prerequisites:

Kenshi game (works with both 32-bit and 64-bit versions)
.NET 8.0 Runtime


Installation Steps:

Copy the mod files to your Kenshi installation directory
Run the KenshiMultiplayerLoader.exe before starting Kenshi
Alternatively, use a mo


