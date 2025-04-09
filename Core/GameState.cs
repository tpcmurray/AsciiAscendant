using System;
using System.Collections.Generic;
using AsciiAscendant.Core.Animations;
using AsciiAscendant.Core.Entities;
using AsciiAscendant.Core.Loot;

namespace AsciiAscendant.Core
{
    public class GameState
    {
        public Player Player { get; private set; }
        public Map CurrentMap { get; private set; }
        public List<Enemy> Enemies { get; private set; }
        public List<Animation> ActiveAnimations { get; private set; }
        public List<Item> DroppedItems { get; private set; }
        
        public GameState()
        {
            // Initialize a default map for testing
            CurrentMap = new Map(90, 50);
            
            // Initialize player at the center of the map
            Player = new Player();
            // Use MoveTo instead of directly setting Position
            Player.MoveTo(CurrentMap, CurrentMap.Width / 2, CurrentMap.Height / 2);
            
            // Initialize enemies list
            Enemies = new List<Enemy>();
            
            // Initialize animations list
            ActiveAnimations = new List<Animation>();
            
            // Initialize dropped items list
            DroppedItems = new List<Item>();
            
            // Add some test enemies
            SpawnInitialEnemies();
        }
        
        private void SpawnInitialEnemies()
        {
            // Add a few basic enemies at random positions
            Random rand = new Random();
            
            for (int i = 0; i < 5; i++)
            {
                // Find a valid spawn position (not on a wall or the player)
                int x, y;
                do
                {
                    x = rand.Next(5, CurrentMap.Width - 5);
                    y = rand.Next(5, CurrentMap.Height - 5);
                } while (!CurrentMap.IsPassable(x, y) || 
                         (Math.Abs(x - Player.Position.X) < 10 && Math.Abs(y - Player.Position.Y) < 10));
                
                // Create and add the enemy
                var enemy = new BasicEnemy($"Goblin {i+1}", 20, 5, 25);
                enemy.MoveTo(CurrentMap, x, y);
                Enemies.Add(enemy);
            }
        }
        
        public void AddEnemy(Enemy enemy)
        {
            Enemies.Add(enemy);
        }
        
        public void RemoveEnemy(Enemy enemy)
        {
            Enemies.Remove(enemy);
        }
        
        public void UpdateEnemies()
        {
            // Create a copy of the list to safely iterate while potentially removing enemies
            var enemiesCopy = new List<Enemy>(Enemies);
            
            foreach (var enemy in enemiesCopy)
            {
                // Update animation state first
                enemy.Update();
                
                if (enemy.IsAlive)
                {
                    enemy.TakeTurn(this);
                }
                else
                {
                    enemy.Die(this);
                }
            }
        }
        
        // Dropped items management
        public void AddDroppedItem(Item item)
        {
            DroppedItems.Add(item);
        }
        
        public void RemoveDroppedItem(Item item)
        {
            DroppedItems.Remove(item);
        }
        
        // Check if there are any items at a given position
        public List<Item> GetItemsAtPosition(int x, int y)
        {
            List<Item> itemsAtPosition = new List<Item>();
            
            foreach (var item in DroppedItems)
            {
                if (item.Position.X == x && item.Position.Y == y)
                {
                    itemsAtPosition.Add(item);
                }
            }
            
            return itemsAtPosition;
        }
        
        // Try to pick up an item at the player's position
        public List<Item> AttemptItemPickup()
        {
            List<Item> itemsAtPlayerPos = GetItemsAtPosition(Player.Position.X, Player.Position.Y);
            List<Item> pickedUpItems = new List<Item>();
            
            foreach (var item in itemsAtPlayerPos)
            {
                // Try to add item to player's inventory
                if (Player.AddItemToInventory(item))
                {
                    // Remove item from the world
                    DroppedItems.Remove(item);
                    pickedUpItems.Add(item);
                    // Console.WriteLine($"Picked up: {item.Name}");
                }
                else
                {
                    // Console.WriteLine("Inventory is full!");
                    break; // Stop trying to pick up more items if inventory is full
                }
            }
            
            return pickedUpItems;
        }
        
        // Update a specific item's position (e.g., dropping an item)
        public void UpdateItemPosition(Item item, int x, int y)
        {
            item.MoveTo(CurrentMap, x, y);
        }
        
        public void UpdateAnimations()
        {
            // Update all active animations
            for (int i = ActiveAnimations.Count - 1; i >= 0; i--)
            {
                var animation = ActiveAnimations[i];
                animation.Update();
                
                // Remove completed animations
                if (animation.IsCompleted)
                {
                    ActiveAnimations.RemoveAt(i);
                }
            }
            
            // Update player animation state
            Player.Update();
            
            // Update enemy animation states - make sure we call Update on each enemy every tick
            foreach (var enemy in Enemies)
            {
                enemy.Update();
                // Debug logging for enemy animation states
                if (enemy.IsMoving)
                {
                    // Console.WriteLine($"Enemy {enemy.Name} is in movement state during animation update");
                }
            }
        }
        
        public void CreateFireballAnimation(Point source, Enemy target, int damage)
        {
            // Create a new fireball animation using our specialized class
            var fireball = new FireballAnimation(source, target.Position, target, damage);
            
            // Add to active animations
            ActiveAnimations.Add(fireball);
        }
        
        public void CreateArrowAnimation(Point source, Enemy target, int damage)
        {
            // Create a new arrow animation using our specialized class
            var arrow = new ArrowAnimation(source, target.Position, target, damage);
            
            // Add to active animations
            ActiveAnimations.Add(arrow);
        }
        
        // Combine regeneration of stamina and tick-based updates
        public void UpdateGameTick()
        {
            // Regenerate player stamina
            Player.RegenerateStamina(1); // Regenerate 1 stamina per tick
            
            // Update animations
            UpdateAnimations();
            
            // Other tick-based updates could go here
        }
    }
}