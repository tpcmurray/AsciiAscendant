using System;
using System.Collections.Generic;

namespace AsciiAscendant.Core.Entities
{
    public class DamageNumber
    {
        public int Value { get; private set; }
        public Point Position { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public float YOffset { get; private set; }
        
        public const int LifetimeMs = 1500; // How long the damage number stays visible
        public const float RiseSpeed = 0.1f; // How fast the damage number rises
        
        public DamageNumber(int value, Point position)
        {
            Value = value;
            Position = position;
            CreatedAt = DateTime.Now;
            YOffset = 0;
        }
        
        public void Update()
        {
            // Make the damage number rise
            YOffset += RiseSpeed;
        }
        
        public bool IsExpired()
        {
            return (DateTime.Now - CreatedAt).TotalMilliseconds >= LifetimeMs;
        }
    }
    
    public abstract class Creature : Entity
    {
        public int Health { get; protected set; }
        public int MaxHealth { get; protected set; }
        public int Damage { get; protected set; }
        public int Level { get; protected set; } = 1; // Default level is 1
        
        // List of active damage numbers
        public List<DamageNumber> ActiveDamageNumbers { get; protected set; } = new List<DamageNumber>();
        
        protected Creature(string name, char symbol, int maxHealth, int damage) 
            : base(name, symbol)
        {
            MaxHealth = maxHealth;
            Health = maxHealth;
            Damage = damage;
            Level = 1;
            ActiveDamageNumbers = new List<DamageNumber>();
        }
        
        public override void Update()
        {
            base.Update();
            
            // Update damage numbers and remove expired ones
            for (int i = ActiveDamageNumbers.Count - 1; i >= 0; i--)
            {
                ActiveDamageNumbers[i].Update();
                if (ActiveDamageNumbers[i].IsExpired())
                {
                    ActiveDamageNumbers.RemoveAt(i);
                }
            }
        }
        
        public virtual void Move(Map map, int dx, int dy)
        {
            int newX = Position.X + dx;
            int newY = Position.Y + dy;
            
            if (CanMoveTo(map, newX, newY))
            {
                // Use MoveTo method instead of setting Position directly
                MoveTo(map, newX, newY);
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
            
            ActiveDamageNumbers.Add(new DamageNumber(amount, new Point(Position.X, Position.Y - 4)));
            
            // Flash the creature red
            Flash();
        }
        
        public bool IsAlive => Health > 0;
        
        // Get health percentage for rendering
        public float GetHealthPercentage()
        {
            return (float)Health / MaxHealth;
        }
    }
}