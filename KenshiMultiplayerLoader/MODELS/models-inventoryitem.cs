using System;
using System.Collections.Generic;

namespace KenshiMultiplayerLoader.MODELS
{
    public class InventoryItem
    {
        // Basic properties
        public string ItemName { get; set; }
        public int Quantity { get; set; }

        // Enhanced properties
        public string ItemId { get; set; } = Guid.NewGuid().ToString();
        public string ItemType { get; set; } // Weapon, Armor, Food, Resource, etc.
        public float Condition { get; set; } = 1.0f; // 0.0-1.0 representing condition
        public float Weight { get; set; } = 1.0f; // Weight in kg
        public int Value { get; set; } = 0; // Value in cats (currency)

        // Item quality affects stats
        public ItemQuality Quality { get; set; } = ItemQuality.Normal;

        // For stackable items, track individual conditions
        public List<float> StackConditions { get; set; } = new List<float>();

        // Equipment properties
        public bool IsEquippable { get; set; }
        public string EquipSlot { get; set; } // Head, Chest, Legs, etc.
        public bool IsEquipped { get; set; }

        // Weapon properties
        public float Damage { get; set; }
        public float AttackSpeed { get; set; } = 1.0f;
        public float Reach { get; set; }
        public string DamageType { get; set; } // Cutting, Blunt, etc.

        // Armor properties
        public float BluntProtection { get; set; }
        public float CutProtection { get; set; }
        public float PierceProtection { get; set; }

        // Food properties
        public int Nutrition { get; set; }
        public int Hydration { get; set; }
        public bool IsPerishable { get; set; }
        public float SpoilRate { get; set; } = 0.0f;
        public DateTime ExpiryDate { get; set; } = DateTime.MaxValue;

        // Crafting/building
        public bool IsCraftingMaterial { get; set; }
        public bool IsBuildingMaterial { get; set; }

        // Special effects when used/equipped
        public Dictionary<string, float> StatModifiers { get; set; } = new Dictionary<string, float>();

        // Default constructor
        public InventoryItem()
        {
        }

        // Basic constructor
        public InventoryItem(string name, int quantity)
        {
            ItemName = name;
            Quantity = quantity;
        }

        // Constructor with type
        public InventoryItem(string name, string type, int quantity)
        {
            ItemName = name;
            ItemType = type;
            Quantity = quantity;
        }

        // Get quality modifier
        private float GetQualityModifier()
        {
            switch (Quality)
            {
                case ItemQuality.Poor: return 0.7f;
                case ItemQuality.Normal: return 1.0f;
                case ItemQuality.Good: return 1.2f;
                case ItemQuality.Excellent: return 1.5f;
                case ItemQuality.Masterwork: return 2.0f;
                default: return 1.0f;
            }
        }

        // Get total weight of the stack
        public float GetTotalWeight()
        {
            return Weight * Quantity;
        }

        // Get total value of the stack
        public int GetTotalValue()
        {
            // Value is affected by condition
            float conditionFactor = Condition * 0.8f + 0.2f; // Even at 0 condition, an item has 20% of its value
            return (int)(Value * Quantity * conditionFactor);
        }
    }

    // Item quality enum
    public enum ItemQuality
    {
        Poor,
        Normal,
        Good,
        Excellent,
        Masterwork
    }
}