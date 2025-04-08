using System;
using System.Collections.Generic;
using Terminal.Gui;
using AsciiAscendant.Core.Entities;

namespace AsciiAscendant.Core
{
    public class Player : Creature
    {
        public int Experience { get; private set; }
        public int Level { get; private set; }
        public List<Skill> Skills { get; private set; }
        
        public Player() 
            : base("Player", '@', 100, 10)
        {
            Level = 1;
            Experience = 0;
            Skills = new List<Skill>();
            
            // Initialize idle ASCII representation for the player
            IdleAscii = new List<string>
            {
                @" σ  ",
                @"/O\",
                @" ╨  "
            };
            
            // Initialize movement ASCII representation for the player (running pose)
            MovementAscii = new List<string>
            {
                @" σ  ",
                @"/O\",
                @"/ \ "
            };
            
            // Add some basic skills
            Skills.Add(new Skill("Slash", 5, 1));
            Skills.Add(new Skill("Fireball", 15, 3));
        }
        
        public void GainExperience(int amount)
        {
            Experience += amount;
            
            // Simple leveling system
            int expNeededForNextLevel = Level * 100;
            if (Experience >= expNeededForNextLevel)
            {
                LevelUp();
            }
        }
        
        private void LevelUp()
        {
            Level++;
            MaxHealth += 10;
            Health = MaxHealth;
            Damage += 2;
            
            // Could add logic here to unlock new skills at certain levels
        }
        
        public void UseSkill(int skillIndex, Creature target)
        {
            if (skillIndex >= 0 && skillIndex < Skills.Count)
            {
                var skill = Skills[skillIndex];
                if (skill.CanUse())
                {
                    target.TakeDamage(skill.Damage);
                    skill.Use();
                }
            }
        }

        public override Terminal.Gui.Attribute GetEntityColor()
        {
            // Player is white regardless of health
            return new Terminal.Gui.Attribute(Color.White, Color.Black);
        }
    }
}