using System;
using System.Collections.Generic;
using AsciiAscendant.Core.Animations;
using AsciiAscendant.Core.Entities;
using AsciiAscendant.Core.Loot;

namespace AsciiAscendant.Core
{
    public class GameState
    {
        public Player Player { get; private set; } = null!;
        public Map CurrentMap { get; private set; } = null!;
        public List<Enemy> Enemies { get; private set; } = new List<Enemy>();
        public List<Animation> ActiveAnimations { get; private set; } = new List<Animation>();
        public List<Item> DroppedItems { get; private set; } = new List<Item>();
        
        // Random generator with an optional seed for consistent map generation
        private readonly Random _random;
        
        // Safe zone radius around player start position (no enemies)
        private const int SafeZoneRadius = 50;
        
        public GameState(int? mapSeed = null)
        {
            // Initialize random number generator with optional seed
            _random = mapSeed.HasValue ? new Random(mapSeed.Value) : new Random();
            
            // Initialize with large random map
            InitializeLargeWorld();
            
            // Initialize player at a suitable starting location
            Player = new Player();
            PositionPlayerAtSuitableLocation();
            
            // Scatter enemies throughout the world
            ScatterEnemiesAcrossMap();
        }
        
        private void InitializeLargeWorld()
        {
            // Create a large 1000x500 map - this will generate terrain features
            // like water, forests, ruins, rocks, etc.
            CurrentMap = Map.GenerateRandomMap(1000, 500, _random.Next());
            
            // Set initial viewport size
            CurrentMap.ViewportWidth = 80; 
            CurrentMap.ViewportHeight = 40;
        }
        
        private void PositionPlayerAtSuitableLocation()
        {
            // Find a suitable starting position for the player (grass area, not water/wall)
            int x, y;
            int attempts = 0;
            const int maxAttempts = 1000; // Prevent infinite loop
            
            do
            {
                // Try random positions in the central area of the map
                x = CurrentMap.Width / 2 - 100 + _random.Next(200);
                y = CurrentMap.Height / 2 - 50 + _random.Next(100);
                attempts++;
            } while ((!CurrentMap.IsPassable(x, y) || 
                     IsTileNearWater(x, y, 3)) && 
                     attempts < maxAttempts);
            
            // Position the player at the found location or map center as fallback
            if (attempts >= maxAttempts)
            {
                // Fallback to a position that's definitely walkable
                for (int testX = 0; testX < CurrentMap.Width; testX++)
                {
                    for (int testY = 0; testY < CurrentMap.Height; testY++)
                    {
                        if (CurrentMap.IsPassable(testX, testY))
                        {
                            x = testX;
                            y = testY;
                            break;
                        }
                    }
                }
            }
            
            // Use MoveTo to place player at position
            Player.MoveTo(CurrentMap, x, y);
            
            // Center the camera on player
            CurrentMap.CenterCamera(x, y);
        }
        
        // Check if a tile is near water (for better player starting position)
        private bool IsTileNearWater(int x, int y, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int checkX = x + dx;
                    int checkY = y + dy;
                    
                    // Check bounds
                    if (checkX >= 0 && checkX < CurrentMap.Width && 
                        checkY >= 0 && checkY < CurrentMap.Height)
                    {
                        if (CurrentMap.Tiles[checkX, checkY].TileType == TileType.Water)
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }
        
        private void ScatterEnemiesAcrossMap()
        {
            // Calculate the number of enemies based on map size (1 enemy per 500 tiles)
            int mapArea = CurrentMap.Width * CurrentMap.Height;
            int totalEnemies = mapArea / 500;
            
            // Cap the maximum number of enemies to prevent performance issues
            totalEnemies = Math.Min(totalEnemies, 500);
            
            // Distribute enemies by type
            int goblinCount = totalEnemies * 3 / 5; // 60% goblins
            int archerCount = totalEnemies - goblinCount; // 40% archers
            
            Console.WriteLine($"Spawning {totalEnemies} enemies across the map ({goblinCount} goblins, {archerCount} archers)");
            
            // Spawn goblins
            for (int i = 0; i < goblinCount; i++)
            {
                SpawnEnemyAtRandomLocation<Goblin>();
            }
            
            // Spawn skeleton archers
            for (int i = 0; i < archerCount; i++)
            {
                SpawnEnemyAtRandomLocation<SkeletonArcher>();
            }
        }
        
        private T SpawnEnemyAtRandomLocation<T>() where T : Enemy, new()
        {
            int x, y;
            int attempts = 0;
            const int maxAttempts = 100;
            bool validSpot;
            
            do
            {
                // Pick a random position on the map
                x = _random.Next(CurrentMap.Width);
                y = _random.Next(CurrentMap.Height);
                
                // Check if this is a valid spawn position
                validSpot = IsValidEnemySpawnPosition(x, y);
                
                // Also check that it's not within the safe zone around the player
                if (validSpot)
                {
                    int distanceToPlayer = Math.Abs(x - Player.Position.X) + Math.Abs(y - Player.Position.Y);
                    validSpot = distanceToPlayer > SafeZoneRadius;
                }
                
                attempts++;
                
            } while (!validSpot && attempts < maxAttempts);
            
            // If we couldn't find a valid position after many attempts, try one last time
            // with just the basic requirements
            if (attempts >= maxAttempts)
            {
                for (int testX = 0; testX < CurrentMap.Width; testX += 10)
                {
                    for (int testY = 0; testY < CurrentMap.Height; testY += 10)
                    {
                        if (CurrentMap.IsPassable(testX, testY))
                        {
                            // Make sure it's outside the safe zone
                            int distanceToPlayer = Math.Abs(testX - Player.Position.X) + Math.Abs(testY - Player.Position.Y);
                            if (distanceToPlayer > SafeZoneRadius)
                            {
                                x = testX;
                                y = testY;
                                goto FoundPosition; // break out of nested loops
                            }
                        }
                    }
                }
            }
            
        FoundPosition:
            // Create and add the enemy
            var enemy = new T();
            enemy.MoveTo(CurrentMap, x, y);
            Enemies.Add(enemy);
            
            return enemy;
        }
        
        // Check if position is valid for enemy spawn
        private bool IsValidEnemySpawnPosition(int x, int y)
        {
            // Check bounds and passability
            if (x < 0 || x >= CurrentMap.Width || y < 0 || y >= CurrentMap.Height || !CurrentMap.IsPassable(x, y))
            {
                return false;
            }
            
            // Check if this tile is water
            if (CurrentMap.Tiles[x, y].TileType == TileType.Water)
            {
                return false;
            }
            
            // Don't spawn in buildings/ruins
            // This is a simple check: if there are walls nearby, it might be inside a ruin
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dy = -2; dy <= 2; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    
                    if (nx >= 0 && nx < CurrentMap.Width && ny >= 0 && ny < CurrentMap.Height &&
                        CurrentMap.Tiles[nx, ny].TileType == TileType.Wall)
                    {
                        return false;
                    }
                }
            }
            
            return true;
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
        
        // New method for enemies to fire arrow projectiles at the player
        public void CreateEnemyArrowAnimation(Point source, int damage)
        {
            // Create a new arrow animation targeting the player
            var arrow = new ArrowAnimation(source, Player.Position, null!, damage);
            
            // Add to active animations
            ActiveAnimations.Add(arrow);
            
            // Note: Damage is applied in the SkeletonArcher's RangedAttack method
        }
        
        // Handles animation updates and automatic item pickup
        public void UpdateGameTick()
        {
            // Health and stamina regeneration have been moved to GameEngine class
            // to occur at a rate of once per second instead of every tick
            
            // Check for automatic item pickup (new functionality)
            CheckForAutomaticItemPickup();
            
            // Update animations
            UpdateAnimations();
            
            // Always ensure camera is centered on player for scrolling map
            CurrentMap.CenterCamera(Player.Position.X, Player.Position.Y);
            
            // Other tick-based updates could go here
        }
        
        // New method to automatically pick up items that collide with the player
        private void CheckForAutomaticItemPickup()
        {
            if (DroppedItems.Count == 0)
                return;
                
            // Create a temporary collection to avoid modifying collection during iteration
            List<Item> itemsToPickup = new List<Item>();
            
            // Check each item for collision with the player
            foreach (var item in DroppedItems)
            {
                if (item.CollidesWith(Player))
                {
                    itemsToPickup.Add(item);
                }
            }
            
            // Process all colliding items
            foreach (var item in itemsToPickup)
            {
                // Try to add item to player's inventory
                if (Player.AddItemToInventory(item))
                {
                    // Remove item from the world
                    DroppedItems.Remove(item);
                    // Console.WriteLine($"Auto-picked up: {item.Name}");
                }
                else
                {
                    // Console.WriteLine("Inventory is full!");
                    break; // Stop trying to pick up more items if inventory is full
                }
            }
        }
    }
}