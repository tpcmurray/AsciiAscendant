using System;
using System.Collections.Generic;
using AsciiAscendant.Core.Loot;

namespace AsciiAscendant.Core.Entities
{
    public class SkeletonArcher : Enemy
    {
        public SkeletonArcher() : base("Skeleton Archer", 'S', 40, 10, 40, 20)
        {
            IdleAscii = new List<string>
            {
                @" 💀  ",
                @"{/╪\",
                @"  ╨  "
            };
            
            MovementAscii = new List<string>
            {
                @" 💀  ",
                @"{/╪\",
                @" / \ "
            };
            
            Level = 1;
            IsRanged = true;
            AttackRange = 20; // Can attack from up to 30 tiles away
        }
        
        protected override void SetupLootTable()
        {
            // Add possible loot drops with their chances
            LootTable.AddEntry("Bow", ItemType.Weapon, 0.4f);
            LootTable.AddEntry("Leather Armor", ItemType.Armor, 0.2f);
            LootTable.AddEntry("Health Potion", ItemType.Consumable, 0.4f);
            LootTable.AddEntry("Ring", ItemType.Accessory, 0.1f);
        }
        
        // Implement ranged attack behavior
        protected override void RangedAttack(GameState gameState)
        {
            // Create arrow animation from skeleton to player
            gameState.CreateEnemyArrowAnimation(Position, Damage);
            
            // Deal damage to the player
            gameState.Player.TakeDamage(Damage);
        }
    }
}