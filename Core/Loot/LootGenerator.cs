using System;
using System.Collections.Generic;

namespace AsciiAscendant.Core.Loot
{
    public class LootGenerator
    {
        private static readonly Random _random = new Random();
        
        // Stat names that can appear on items
        private static readonly string[] _possibleStats = new string[]
        {
            "Strength",
            "Intelligence",
            "Dexterity",
            "MaxHealth",
            "MaxStamina",
            "Damage",
            "Armor",
            "CritChance"
        };
        
        // Generate an item from the loot table
        public static List<Item> GenerateLoot(LootTable lootTable, int enemyLevel)
        {
            List<Item> generatedLoot = new List<Item>();
            
            foreach (var entry in lootTable.GetEntries())
            {
                // Check if this item should drop based on its drop chance
                if (_random.NextDouble() <= entry.DropChance)
                {
                    // Create the item
                    Item item = CreateRandomItem(entry.ItemName, entry.ItemType, enemyLevel);
                    generatedLoot.Add(item);
                }
            }
            
            return generatedLoot;
        }
        
        // Create a random item with stats based on enemy level
        private static Item CreateRandomItem(string baseName, ItemType type, int enemyLevel)
        {
            // Determine quality based on enemy level and random chance
            ItemQuality quality = DetermineQuality(enemyLevel);
            
            // Create base item
            string itemName = GenerateItemName(baseName, quality);
            Item item = new Item(itemName, type, quality);
            
            // Add primary stat based on item type
            AddPrimaryStat(item, enemyLevel, quality);
            
            // Add secondary stats based on quality
            int secondaryStatCount = GetSecondaryStatCount(quality);
            AddSecondaryStats(item, enemyLevel, quality, secondaryStatCount);
            
            return item;
        }
        
        // Determine item quality based on enemy level and random chance
        private static ItemQuality DetermineQuality(int enemyLevel)
        {
            double roll = _random.NextDouble();
            
            // Base chances modified by enemy level
            double legendaryChance = 0.01 + (enemyLevel * 0.005); // 1% + 0.5% per level
            double epicChance = 0.05 + (enemyLevel * 0.01);       // 5% + 1% per level
            double rareChance = 0.15 + (enemyLevel * 0.015);      // 15% + 1.5% per level
            double uncommonChance = 0.30 + (enemyLevel * 0.02);   // 30% + 2% per level
            
            if (roll < legendaryChance)
                return ItemQuality.Legendary;
            else if (roll < legendaryChance + epicChance)
                return ItemQuality.Epic;
            else if (roll < legendaryChance + epicChance + rareChance)
                return ItemQuality.Rare;
            else if (roll < legendaryChance + epicChance + rareChance + uncommonChance)
                return ItemQuality.Uncommon;
            else
                return ItemQuality.Common;
        }
        
        // Generate a name for the item based on its quality
        private static string GenerateItemName(string baseName, ItemQuality quality)
        {
            string prefix = "";
            string suffix = "";
            
            switch (quality)
            {
                case ItemQuality.Common:
                    // No prefix or suffix for common items
                    break;
                case ItemQuality.Uncommon:
                    prefix = GetRandomPrefix(quality);
                    break;
                case ItemQuality.Rare:
                    prefix = GetRandomPrefix(quality);
                    suffix = GetRandomSuffix(quality);
                    break;
                case ItemQuality.Epic:
                    prefix = GetRandomPrefix(quality);
                    suffix = GetRandomSuffix(quality);
                    break;
                case ItemQuality.Legendary:
                    // Legendary items have unique names
                    return GetRandomLegendaryName(baseName);
            }
            
            return string.IsNullOrEmpty(prefix) 
                ? (string.IsNullOrEmpty(suffix) ? baseName : $"{baseName} {suffix}") 
                : (string.IsNullOrEmpty(suffix) ? $"{prefix} {baseName}" : $"{prefix} {baseName} {suffix}");
        }
        
        // Get a random prefix appropriate for the item's quality
        private static string GetRandomPrefix(ItemQuality quality)
        {
            string[] uncommonPrefixes = { "Fine", "Sturdy", "Sharp", "Strong", "Hardy" };
            string[] rarePrefixes = { "Superior", "Enchanted", "Gleaming", "Masterwork", "Reinforced" };
            string[] epicPrefixes = { "Exquisite", "Magnificent", "Mythical", "Astral", "Draconic" };
            
            switch (quality)
            {
                case ItemQuality.Uncommon:
                    return uncommonPrefixes[_random.Next(uncommonPrefixes.Length)];
                case ItemQuality.Rare:
                    return rarePrefixes[_random.Next(rarePrefixes.Length)];
                case ItemQuality.Epic:
                    return epicPrefixes[_random.Next(epicPrefixes.Length)];
                default:
                    return "";
            }
        }
        
        // Get a random suffix appropriate for the item's quality
        private static string GetRandomSuffix(ItemQuality quality)
        {
            string[] uncommonSuffixes = { "of Skill", "of Power", "of Vigor", "of Haste", "of Defense" };
            string[] rareSuffixes = { "of Might", "of the Adept", "of Precision", "of the Sentinel", "of Quickness" };
            string[] epicSuffixes = { "of the Champion", "of the Archmage", "of Dominance", "of Annihilation", "of the Titan" };
            
            switch (quality)
            {
                case ItemQuality.Uncommon:
                    return uncommonSuffixes[_random.Next(uncommonSuffixes.Length)];
                case ItemQuality.Rare:
                    return rareSuffixes[_random.Next(rareSuffixes.Length)];
                case ItemQuality.Epic:
                    return epicSuffixes[_random.Next(epicSuffixes.Length)];
                default:
                    return "";
            }
        }
        
        // Get a random legendary name for the item
        private static string GetRandomLegendaryName(string baseName)
        {
            string[] legendaryWeaponNames = { "Thunderfury", "Shadowfang", "Doomcaller", "Soulreaver", "Harbinger" };
            string[] legendaryArmorNames = { "Vanguard's Bulwark", "Dreadplate", "Stormshield", "Skyguard", "Ironhide" };
            
            if (baseName.Contains("Sword") || baseName.Contains("Axe") || baseName.Contains("Bow"))
            {
                return legendaryWeaponNames[_random.Next(legendaryWeaponNames.Length)];
            }
            else
            {
                return legendaryArmorNames[_random.Next(legendaryArmorNames.Length)];
            }
        }
        
        // Add a primary stat to the item based on its type
        private static void AddPrimaryStat(Item item, int enemyLevel, ItemQuality quality)
        {
            int qualityMultiplier = GetQualityMultiplier(quality);
            int statValue = CalculateStatValue(enemyLevel, qualityMultiplier, isPrimary: true);
            
            switch (item.Type)
            {
                case ItemType.Weapon:
                    item.AddStat("Damage", statValue);
                    break;
                case ItemType.Armor:
                    item.AddStat("Armor", statValue);
                    break;
                case ItemType.Consumable:
                    item.AddStat("HealAmount", statValue);
                    break;
                case ItemType.Accessory:
                    // Accessories don't have a fixed primary stat, add a random attribute
                    item.AddStat(_possibleStats[_random.Next(_possibleStats.Length)], statValue);
                    break;
            }
        }
        
        // Add secondary stats to the item
        private static void AddSecondaryStats(Item item, int enemyLevel, ItemQuality quality, int statCount)
        {
            int qualityMultiplier = GetQualityMultiplier(quality);
            
            // Create a copy of possible stats to avoid duplicates
            List<string> availableStats = new List<string>(_possibleStats);
            
            // Remove primary stat from available stats to avoid duplicates
            if (item.Type == ItemType.Weapon && item.GetStat("Damage") > 0)
                availableStats.Remove("Damage");
            else if (item.Type == ItemType.Armor && item.GetStat("Armor") > 0)
                availableStats.Remove("Armor");
            
            // Add random secondary stats
            for (int i = 0; i < statCount; i++)
            {
                if (availableStats.Count == 0)
                    break;
                
                // Select a random stat from the available pool
                int statIndex = _random.Next(availableStats.Count);
                string statName = availableStats[statIndex];
                
                // Calculate the stat value
                int statValue = CalculateStatValue(enemyLevel, qualityMultiplier, isPrimary: false);
                
                // Add the stat to the item
                item.AddStat(statName, statValue);
                
                // Remove the used stat from the available pool
                availableStats.RemoveAt(statIndex);
            }
        }
        
        // Get the number of secondary stats based on item quality
        private static int GetSecondaryStatCount(ItemQuality quality)
        {
            return quality switch
            {
                ItemQuality.Common => 0,
                ItemQuality.Uncommon => _random.Next(1, 2), // 1 stat
                ItemQuality.Rare => _random.Next(1, 3),     // 1-2 stats
                ItemQuality.Epic => _random.Next(2, 4),     // 2-3 stats
                ItemQuality.Legendary => _random.Next(3, 5), // 3-4 stats
                _ => 0
            };
        }
        
        // Get a multiplier based on item quality
        private static int GetQualityMultiplier(ItemQuality quality)
        {
            return quality switch
            {
                ItemQuality.Common => 1,
                ItemQuality.Uncommon => 2,
                ItemQuality.Rare => 3,
                ItemQuality.Epic => 4,
                ItemQuality.Legendary => 5,
                _ => 1
            };
        }
        
        // Calculate a stat value based on enemy level, quality multiplier, and whether it's a primary stat
        private static int CalculateStatValue(int enemyLevel, int qualityMultiplier, bool isPrimary)
        {
            // Base value calculation
            int baseValue = enemyLevel + 1;
            
            // Primary stats are stronger than secondary stats
            double statMultiplier = isPrimary ? 2.0 : 1.0;
            
            // Add some randomness
            double randomFactor = 0.8 + (_random.NextDouble() * 0.4); // 0.8 to 1.2
            
            // Final value calculation
            int value = (int)Math.Ceiling(baseValue * qualityMultiplier * statMultiplier * randomFactor);
            
            return value;
        }
    }
}