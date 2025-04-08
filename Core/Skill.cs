using System;

namespace AsciiAscendant.Core
{
    public class Skill
    {
        public string Name { get; private set; }
        public int Damage { get; private set; }
        public int Cooldown { get; private set; }
        public int CurrentCooldown { get; private set; }
        
        public Skill(string name, int damage, int cooldown)
        {
            Name = name;
            Damage = damage;
            Cooldown = cooldown;
            CurrentCooldown = 0;
        }
        
        public bool CanUse()
        {
            return CurrentCooldown <= 0;
        }
        
        public void Use()
        {
            if (CanUse())
            {
                CurrentCooldown = Cooldown;
            }
        }
        
        public void UpdateCooldown()
        {
            if (CurrentCooldown > 0)
            {
                CurrentCooldown--;
            }
        }
    }
}