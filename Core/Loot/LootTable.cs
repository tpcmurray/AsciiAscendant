using System;
using System.Collections.Generic;

namespace AsciiAscendant.Core.Loot
{
    // Represents a possible item drop with its drop chance
    public class LootEntry
    {
        public string ItemName { get; private set; }
        public ItemType ItemType { get; private set; }
        public float DropChance { get; private set; } // 0.0f to 1.0f
        
        public LootEntry(string itemName, ItemType itemType, float dropChance)
        {
            ItemName = itemName;
            ItemType = itemType;
            DropChance = Math.Clamp(dropChance, 0.0f, 1.0f);
        }
    }
    
    // Represents a collection of possible item drops
    public class LootTable
    {
        private List<LootEntry> _entries = new List<LootEntry>();
        
        public void AddEntry(LootEntry entry)
        {
            _entries.Add(entry);
        }
        
        public void AddEntry(string itemName, ItemType itemType, float dropChance)
        {
            _entries.Add(new LootEntry(itemName, itemType, dropChance));
        }
        
        public List<LootEntry> GetEntries()
        {
            return _entries;
        }
    }
}