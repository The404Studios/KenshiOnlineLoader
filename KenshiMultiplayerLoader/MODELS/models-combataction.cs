using System;
using System.Collections.Generic;

namespace KenshiMultiplayerLoader.MODELS
{
    public class CombatAction
    {
        // Basic properties
        public string TargetId { get; set; }
        public string Action { get; set; } // e.g., "attack", "defend", "block", etc.

        // Enhanced properties
        public string WeaponId { get; set; }      // ID of the weapon being used
        public string AttackType { get; set; }    // Slash, Blunt, Cut, Pierce, etc.
        public string TargetLimb { get; set; }    // Head, Chest, LeftArm, etc.
        public float Power { get; set; } = 1.0f;  // Attack power multiplier (0.0-2.0)
        public bool IsCritical { get; set; }      // Whether this is a critical hit
        public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Position data for attack validation
        public float AttackerPosX { get; set; }
        public float AttackerPosY { get; set; }
        public float AttackerPosZ { get; set; }

        // Status effects applied by this attack
        public List<StatusEffect> StatusEffects { get; set; } = new List<StatusEffect>();

        // Animation data
        public string AnimationName { get; set; }
        public float AnimationSpeed { get; set; } = 1.0f;

        // Default constructor
        public CombatAction()
        {
        }

        // Constructor with basic parameters
        public CombatAction(string targetId, string action)
        {
            TargetId = targetId;
            Action = action;
        }
    }

    // Status effect class for combat
    public class StatusEffect
    {
        public string Type { get; set; }       // Bleed, Stun, Poison, etc.
        public float Duration { get; set; }    // In seconds
        public float Power { get; set; } = 1.0f; // Effect power multiplier
        public long AppliedAt { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        // Check if this effect is still active
        public bool IsActive()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long endTime = AppliedAt + (long)(Duration * 1000);
            return now < endTime;
        }

        // Get remaining time in seconds
        public float GetRemainingTime()
        {
            if (!IsActive())
                return 0;

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long endTime = AppliedAt + (long)(Duration * 1000);
            return (endTime - now) / 1000.0f;
        }
    }
}