using System;
using System.Collections.Generic;
using AsciiAscendant.Core.Entities;

namespace AsciiAscendant.Core
{
    public class GameState
    {
        public Player Player { get; private set; }
        public Map CurrentMap { get; private set; }
        public List<Enemy> Enemies { get; private set; }
        
        public GameState()
        {
            // Initialize a default map for testing
            CurrentMap = new Map(90, 50);
            
            // Initialize player at the center of the map
            Player = new Player();
            Player.Position = new Point(CurrentMap.Width / 2, CurrentMap.Height / 2);
            
            // Initialize enemies list
            Enemies = new List<Enemy>();
            
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
                enemy.Position = new Point(x, y);
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
    }
}