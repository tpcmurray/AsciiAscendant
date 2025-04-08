using System;
using System.Collections.Generic;

namespace AsciiAscendant.Core.Entities
{
    public abstract class Creature : Entity
    {
        public int Health { get; protected set; }
        public int MaxHealth { get; protected set; }
        public int Damage { get; protected set; }
        
        protected Creature(string name, char symbol, int maxHealth, int damage) 
            : base(name, symbol)
        {
            MaxHealth = maxHealth;
            Health = maxHealth;
            Damage = damage;
        }
        
        public virtual void Move(Map map, int dx, int dy)
        {
            int newX = Position.X + dx;
            int newY = Position.Y + dy;
            
            if (CanMoveTo(map, newX, newY))
            {
                Position = new Point(newX, newY);
            }
        }
        
        public virtual void Attack(Creature target)
        {
            target.TakeDamage(Damage);
        }
        
        public virtual void TakeDamage(int amount)
        {
            Health -= amount;
            if (Health < 0)
            {
                Health = 0;
            }
        }
        
        public bool IsAlive => Health > 0;
        
        // Get health percentage for rendering
        public float GetHealthPercentage()
        {
            return (float)Health / MaxHealth;
        }
    }
}