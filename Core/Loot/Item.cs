using System;
using System.Collections.Generic;
using Terminal.Gui;
using AsciiAscendant.Core.Entities;

namespace AsciiAscendant.Core.Loot
{
    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Accessory
    }
    
    public enum ItemQuality
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    public class Item : Entity
    {
        public ItemType Type { get; private set; }
        public ItemQuality Quality { get; private set; }
        public Dictionary<string, int> Stats { get; private set; }
        public bool IsEquipped { get; set; }
        
        public Item(string name, ItemType type, ItemQuality quality)
            : base(name, GetSymbolForItemType(type))
        {
            Type = type;
            Quality = quality;
            Stats = new Dictionary<string, int>();
            IsEquipped = false;
            
            // Initialize idle ASCII representation based on item type
            InitializeAsciiRepresentation();
        }
        
        private void InitializeAsciiRepresentation()
        {
            // Set ASCII representation based on item type
            switch (Type)
            {
                case ItemType.Weapon:
                    IdleAscii = new List<string> { "⚔" };
                    break;
                case ItemType.Armor:
                    IdleAscii = new List<string> { "⛨" };
                    break;
                case ItemType.Consumable:
                    IdleAscii = new List<string> { "⊕" };
                    break;
                case ItemType.Accessory:
                    IdleAscii = new List<string> { "◎" };
                    break;
            }
            
            // Copy idle representation to movement (items don't have special movement animations)
            MovementAscii = new List<string>(IdleAscii);
        }
        
        private static char GetSymbolForItemType(ItemType type)
        {
            return type switch
            {
                ItemType.Weapon => '⚒',
                ItemType.Armor => '⛨',
                ItemType.Consumable => '⚱',
                ItemType.Accessory => '◎',
                _ => '?'
            };
        }
        
        public void AddStat(string statName, int value)
        {
            if (Stats.ContainsKey(statName))
            {
                Stats[statName] += value;
            }
            else
            {
                Stats[statName] = value;
            }
        }
        
        public int GetStat(string statName)
        {
            return Stats.TryGetValue(statName, out int value) ? value : 0;
        }
        
        public override Terminal.Gui.Attribute GetEntityColor()
        {
            // Return color based on item quality
            return Quality switch
            {
                ItemQuality.Common => new Terminal.Gui.Attribute(Color.Gray, Color.Black),
                ItemQuality.Uncommon => new Terminal.Gui.Attribute(Color.Green, Color.Black),
                ItemQuality.Rare => new Terminal.Gui.Attribute(Color.Blue, Color.Black),
                ItemQuality.Epic => new Terminal.Gui.Attribute(Color.Magenta, Color.Black),
                ItemQuality.Legendary => new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black),
                _ => new Terminal.Gui.Attribute(Color.White, Color.Black)
            };
        }
        
        public string GetDescription()
        {
            string description = $"{Name} ({Quality} {Type})\n";
            
            foreach (var stat in Stats)
            {
                string prefix = stat.Value > 0 ? "+" : "";
                description += $"{prefix}{stat.Value} {stat.Key}\n";
            }
            
            return description;
        }
    }
}