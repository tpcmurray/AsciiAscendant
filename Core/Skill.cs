using System;

namespace AsciiAscendant.Core
{
    public enum SkillType
    {
        Melee,
        Ranged,
        Area,
        Buff
    }

    public class Skill
    {
        public string Name { get; private set; }
        public int Damage { get; private set; }
        
        // Cooldown in seconds (for display purposes)
        public float CooldownInSeconds { get; private set; }
        
        // Internal cooldown in ticks (10 ticks = 1 second)
        private int _cooldownInTicks;
        private int _currentCooldownTicks;
        
        // Expose current cooldown in seconds for UI display
        public float CurrentCooldownInSeconds => _currentCooldownTicks / 10f;
        
        // New properties for skill ranges
        public int MinRange { get; private set; }
        public int MaxRange { get; private set; }
        public SkillType Type { get; private set; }
        
        public Skill(string name, int damage, float cooldownInSeconds)
            : this(name, damage, cooldownInSeconds, SkillType.Melee, 1, 2) // Default to melee
        {
        }
        
        public Skill(string name, int damage, float cooldownInSeconds, SkillType type, int minRange, int maxRange)
        {
            Name = name;
            Damage = damage;
            CooldownInSeconds = cooldownInSeconds;
            _cooldownInTicks = (int)(cooldownInSeconds * 10); // Convert seconds to ticks (10 ticks per second)
            _currentCooldownTicks = 0;
            Type = type;
            MinRange = minRange;
            MaxRange = maxRange;
        }
        
        public bool CanUse()
        {
            return _currentCooldownTicks <= 0;
        }
        
        public void Use()
        {
            if (CanUse())
            {
                _currentCooldownTicks = _cooldownInTicks;
            }
        }
        
        public void UpdateCooldown()
        {
            if (_currentCooldownTicks > 0)
            {
                _currentCooldownTicks--;
            }
        }
        
        // Check if the target is within valid range for this skill
        public bool IsInRange(Point origin, Point target)
        {
            int distance = CalculateDistance(origin, target);
            return distance >= MinRange && distance <= MaxRange;
        }
        
        // Calculate distance between two points
        private int CalculateDistance(Point a, Point b)
        {
            // Use Manhattan distance for simplicity
            return Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
        }
    }
}