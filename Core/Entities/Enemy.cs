using System;
using System.Collections.Generic;
using Terminal.Gui;

namespace AsciiAscendant.Core.Entities
{
    public abstract class Enemy : Creature
    {
        public int ExperienceValue { get; protected set; }
        public int DetectionRange { get; protected set; }
        
        protected Enemy(string name, char symbol, int maxHealth, int damage, int experienceValue, int detectionRange) 
            : base(name, symbol, maxHealth, damage)
        {
            ExperienceValue = experienceValue;
            DetectionRange = detectionRange;
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
            Console.WriteLine($"{Name} takes {amount} damage!");
        }
        
        public virtual void Die(GameState gameState)
        {
            // Grant experience to player when enemy dies
            gameState.Player.GainExperience(ExperienceValue);
            
            // Remove from active enemies list
            gameState.RemoveEnemy(this);
            
            // TODO: Drop loot, play death animation, etc.
            Console.WriteLine($"{Name} has been defeated!");
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