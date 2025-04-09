using System;
using System.Collections.Generic;
using Terminal.Gui;
using AsciiAscendant.Core.Loot;

namespace AsciiAscendant.Core.Entities
{
    public abstract class Enemy : Creature
    {
        public int ExperienceValue { get; protected set; }
        public int DetectionRange { get; protected set; }
        public LootTable LootTable { get; protected set; }
        
        protected Enemy(string name, char symbol, int maxHealth, int damage, int experienceValue, int detectionRange) 
            : base(name, symbol, maxHealth, damage)
        {
            ExperienceValue = experienceValue;
            DetectionRange = detectionRange;
            LootTable = new LootTable();
            
            // Each enemy type will add its own loot to the table in its constructor
            SetupLootTable();
        }
        
        protected virtual void SetupLootTable()
        {
            // Base enemy has no loot by default - subclasses will override
        }
        
        public abstract void TakeTurn(GameState gameState);
        
        public bool CanSeePlayer(GameState gameState)
        {
            int dx = Math.Abs(Position.X - gameState.Player.Position.X);
            int dy = Math.Abs(Position.Y - gameState.Player.Position.Y);
            
            // Simple distance calculation to determine if player is in detection range
            return Math.Sqrt(dx * dx + dy * dy) <= DetectionRange;
        }
        
        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
            
            // Visual feedback when damaged - can be expanded later
            // Console.WriteLine($"{Name} takes {amount} damage!");
        }
        
        public virtual void Die(GameState gameState)
        {
            // Grant experience to player when enemy dies
            gameState.Player.GainExperience(ExperienceValue);
            
            // Generate and drop loot
            DropLoot(gameState);
            
            // Remove from active enemies list
            gameState.RemoveEnemy(this);
            
            // Console.WriteLine($"{Name} has been defeated!");
        }
        
        protected virtual void DropLoot(GameState gameState)
        {
            // Generate loot from this enemy's loot table
            List<Item> droppedItems = LootGenerator.GenerateLoot(LootTable, Level);
            
            // Add items to game world at the enemy's position
            foreach (var item in droppedItems)
            {
                // Place item at enemy's position
                item.MoveTo(gameState.CurrentMap, Position.X, Position.Y);
                
                // Add item to game state
                gameState.AddDroppedItem(item);
                
                // Console.WriteLine($"{Name} dropped: {item.Name}");
            }
        }
        
        public override Terminal.Gui.Attribute GetEntityColor()
        {
            float healthPercentage = GetHealthPercentage();
            
            if (healthPercentage > 0.7f)
                return new Terminal.Gui.Attribute(Color.Green, Color.Black);
            else if (healthPercentage > 0.3f)
                return new Terminal.Gui.Attribute(Color.Brown, Color.Black);
            else
                return new Terminal.Gui.Attribute(Color.Red, Color.Black);
        }
    }
}