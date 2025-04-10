using System;
using System.Collections.Generic;
using System.Linq;
using Terminal.Gui;
using AsciiAscendant.Core.Entities;
using AsciiAscendant.Core.Loot;

namespace AsciiAscendant.Core
{
    public class Player : Creature
    {
        public int Experience { get; private set; }
        // We'll use the Level property inherited from Creature, so no need to redeclare it here
        public List<Skill> Skills { get; private set; }
        public List<Item> Inventory { get; private set; }
        
        // Equipment slots
        public Item? EquippedWeapon { get; private set; }
        public Item? EquippedArmor { get; private set; }
        public Item? EquippedAccessory { get; private set; }
        
        // Base attributes
        public int Strength { get; private set; }
        public int Intelligence { get; private set; }
        public int Dexterity { get; private set; }
        
        // Stamina resource
        public int Stamina { get; private set; }
        public int MaxStamina { get; private set; }
        
        // Maximum inventory size
        private const int MaxInventorySize = 20;
        
        public Player() 
            : base("Player", '@', 100, 10)
        {
            Experience = 0;
            Skills = new List<Skill>();
            Inventory = new List<Item>();
            
            // Initialize attributes
            Strength = 5;
            Intelligence = 5;
            Dexterity = 5;
            
            // Initialize stamina
            MaxStamina = 100;
            Stamina = MaxStamina;
            
            // Initialize idle ASCII representation for the player
            IdleAscii = new List<string>
            {
                @" σ",
                @"/O\",
                @" ╨"
            };
            
            // Initialize movement ASCII representation for the player (running pose)
            MovementAscii = new List<string>
            {
                @" σ ",
                @"/O\",
                @"/ \"
            };
            
            // Add skills with appropriate ranges
            Skills.Add(new Skill("Slash", 5, 1.0f, SkillType.Melee, 1, 4)); // Melee
            Skills.Add(new Skill("Fireball", 15, 6.0f, SkillType.Ranged, 3, 70)); // Ranged
            Skills.Add(new Skill("Arrow Shot", 8, 2.5f, SkillType.Ranged, 2, 50)); // Ranged
        }
        
        // Inventory Methods
        
        public bool AddItemToInventory(Item item)
        {
            if (Inventory.Count >= MaxInventorySize)
            {
                return false; // Inventory is full
            }
            
            Inventory.Add(item);
            return true;
        }
        
        public bool RemoveItemFromInventory(Item item)
        {
            return Inventory.Remove(item);
        }
        
        public bool EquipItem(Item item)
        {
            // Check if the item is in inventory
            if (!Inventory.Contains(item))
            {
                return false;
            }
            
            // Unequip current item in this slot if any
            switch (item.Type)
            {
                case ItemType.Weapon:
                    if (EquippedWeapon != null)
                    {
                        UnequipItem(EquippedWeapon);
                    }
                    EquippedWeapon = item;
                    break;
                    
                case ItemType.Armor:
                    if (EquippedArmor != null)
                    {
                        UnequipItem(EquippedArmor);
                    }
                    EquippedArmor = item;
                    break;
                    
                case ItemType.Accessory:
                    if (EquippedAccessory != null)
                    {
                        UnequipItem(EquippedAccessory);
                    }
                    EquippedAccessory = item;
                    break;
                    
                case ItemType.Consumable:
                    // Consumables are used, not equipped
                    UseConsumable(item);
                    return true;
                    
                default:
                    return false;
            }
            
            // Mark the item as equipped
            item.IsEquipped = true;
            
            // Apply item stats to player
            ApplyItemStats();
            
            return true;
        }
        
        public bool UnequipItem(Item item)
        {
            if (!item.IsEquipped)
            {
                return false;
            }
            
            // Remove from equipped slot
            switch (item.Type)
            {
                case ItemType.Weapon:
                    if (EquippedWeapon == item)
                    {
                        EquippedWeapon = null;
                    }
                    break;
                    
                case ItemType.Armor:
                    if (EquippedArmor == item)
                    {
                        EquippedArmor = null;
                    }
                    break;
                    
                case ItemType.Accessory:
                    if (EquippedAccessory == item)
                    {
                        EquippedAccessory = null;
                    }
                    break;
                    
                default:
                    return false;
            }
            
            // Mark the item as unequipped
            item.IsEquipped = false;
            
            // Recalculate player stats
            ApplyItemStats();
            
            return true;
        }
        
        private void UseConsumable(Item item)
        {
            if (item.Type != ItemType.Consumable)
            {
                return;
            }
            
            // Apply consumable effects
            int healAmount = item.GetStat("HealAmount");
            if (healAmount > 0)
            {
                Health = Math.Min(Health + healAmount, MaxHealth);
            }
            
            // Remove the consumable from inventory
            RemoveItemFromInventory(item);
        }
        
        private void ApplyItemStats()
        {
            // Reset base stats
            ResetToBaseStats();
            
            // Apply stats from equipped items
            ApplyItemStatModifiers(EquippedWeapon);
            ApplyItemStatModifiers(EquippedArmor);
            ApplyItemStatModifiers(EquippedAccessory);
        }
        
        private void ApplyItemStatModifiers(Item? item)
        {
            if (item == null)
            {
                return;
            }
            
            // Apply different stats from the item
            Damage += item.GetStat("Damage");
            MaxHealth += item.GetStat("MaxHealth");
            Strength += item.GetStat("Strength");
            Intelligence += item.GetStat("Intelligence");
            Dexterity += item.GetStat("Dexterity");
            MaxStamina += item.GetStat("MaxStamina");
        }
        
        private void ResetToBaseStats()
        {
            // Reset derived stats that could be modified by equipment
            Damage = GetBaseDamage();
            MaxHealth = GetBaseHealth();
            MaxStamina = GetBaseStamina();
            
            // Ensure health and stamina don't exceed new maximums
            Health = Math.Min(Health, MaxHealth);
            Stamina = Math.Min(Stamina, MaxStamina);
        }
        
        private int GetBaseDamage()
        {
            // Base damage calculation (could be refined based on attributes)
            return 10 + (Strength / 2);
        }
        
        private int GetBaseHealth()
        {
            // Base health calculation
            return 100 + (Level * 10);
        }
        
        private int GetBaseStamina()
        {
            // Base stamina calculation
            return 100 + (Dexterity * 2);
        }
        
        // Stamina Methods
        
        public bool UseStamina(int amount)
        {
            if (Stamina < amount)
            {
                return false; // Not enough stamina
            }
            
            Stamina -= amount;
            return true;
        }
        
        public void RegenerateStamina()
        {
            Stamina = Math.Min(Stamina + 1, MaxStamina);
        }
        
        // Health regeneration method
        public void RegenerateHealth()
        {
            // Only regenerate if player is alive
            if (Health > 0)
            {
                Health = Math.Min(Health + 1, MaxHealth);
            }
        }
        
        // Existing methods
        
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
            
            // Increase base attributes
            Strength++;
            Intelligence++;
            Dexterity++;
            
            // Recalculate derived stats
            MaxHealth = GetBaseHealth();
            Health = MaxHealth;
            MaxStamina = GetBaseStamina();
            Stamina = MaxStamina;
            Damage = GetBaseDamage();
            
            // Apply equipment stats on top of new base stats
            ApplyItemStats();
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