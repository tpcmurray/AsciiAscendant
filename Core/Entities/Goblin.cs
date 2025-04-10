using System.Collections.Generic;
using AsciiAscendant.Core.Loot;

namespace AsciiAscendant.Core.Entities
{
    public class Goblin : Enemy
    {
        public Goblin() : base("Goblin", 'G', 20, 5, 25, 15)
        {
            IdleAscii = new List<string>
            {
                @"^o_o^",
                @" /Θ\ ",
                @"  ╨  "
            };
            
            MovementAscii = new List<string>
            {
                @"^-_-^",
                @" /Θ\ ",
                @" / \  "
            };
            
            Level = 1;
            IsRanged = false;
            AttackRange = 1; // Melee range
        }
        
        protected override void SetupLootTable()
        {
            // Add possible loot drops with their chances
            LootTable.AddEntry("Sword", ItemType.Weapon, 0.3f);
            LootTable.AddEntry("Leather Armor", ItemType.Armor, 0.2f);
            LootTable.AddEntry("Health Potion", ItemType.Consumable, 0.5f);
            LootTable.AddEntry("Ring", ItemType.Accessory, 0.1f);
        }
        
        // Using the base implementation from Enemy class for TakeTurn
    }
}