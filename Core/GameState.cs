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
        public List<Animation> ActiveAnimations { get; private set; }
        
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
                    // Check if this is a projectile that hits its target
                    if (animation is ProjectileAnimation projectile && projectile.Target != null)
                    {
                        // Apply the projectile's effect
                        projectile.ApplyEffect();
                    }
                    
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
                    Console.WriteLine($"Enemy {enemy.Name} is in movement state during animation update");
                }
            }
        }
        
        public void CreateFireballAnimation(Point source, Enemy target, int damage)
        {
            // Create a new fireball projectile animation
            var fireball = new ProjectileAnimation(
                source, 
                target.Position,
                '*',
                new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightRed, Terminal.Gui.Color.Black),
                100, // Move one step every 200ms
                target,
                damage
            );
            
            // Add to active animations
            ActiveAnimations.Add(fireball);
        }
    }
    
    public abstract class Animation
    {
        public bool IsCompleted { get; protected set; }
        
        public abstract void Update();
        public abstract void Draw(Terminal.Gui.View view);
    }
    
    public class ProjectileAnimation : Animation
    {
        public Point CurrentPosition { get; private set; }
        public Point TargetPosition { get; private set; }
        public char Symbol { get; private set; }
        public Terminal.Gui.Attribute Color { get; private set; }
        public Enemy? Target { get; private set; }
        public int Damage { get; private set; }
        
        private readonly int _updateIntervalMs;
        private readonly double _directionX;  // Changed to double for more precise movement
        private readonly double _directionY;  // Changed to double for more precise movement
        private readonly double _moveStep;
        private double _accumulatedTime;
        private DateTime _lastUpdateTime;
        private double _currentX;  // Use double for fractional positions
        private double _currentY;  // Use double for fractional positions
        
        public ProjectileAnimation(Point source, Point target, char symbol, Terminal.Gui.Attribute color, int updateIntervalMs, Enemy? targetEnemy = null, int damage = 0)
        {
            CurrentPosition = source;
            TargetPosition = target;
            Symbol = symbol;
            Color = color;
            _updateIntervalMs = updateIntervalMs;
            _lastUpdateTime = DateTime.Now;
            Target = targetEnemy;
            Damage = damage;
            
            // Store initial position as double for precise movement
            _currentX = source.X;
            _currentY = source.Y;
            
            // Calculate direction vector
            int dx = target.X - source.X;
            int dy = target.Y - source.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            
            if (distance > 0)
            {
                // Store normalized direction vector as doubles
                _directionX = dx / distance;
                _directionY = dy / distance;
                _moveStep = 1.0; // One unit per step
            }
            else
            {
                _directionX = 0;
                _directionY = 0;
                _moveStep = 0;
                IsCompleted = true;
            }
            
            // Debug output to help diagnose movement issues
            Console.WriteLine($"Created fireball: From ({source.X},{source.Y}) to ({target.X},{target.Y})");
            Console.WriteLine($"Direction: ({_directionX:F2},{_directionY:F2}), Step: {_moveStep}");
        }
        
        public override void Update()
        {
            var now = DateTime.Now;
            _accumulatedTime += (now - _lastUpdateTime).TotalMilliseconds;
            _lastUpdateTime = now;
            
            // Check if it's time to move the projectile
            while (_accumulatedTime >= _updateIntervalMs)
            {
                // Move in direction of target using floating point positions
                _currentX += _directionX * _moveStep;
                _currentY += _directionY * _moveStep;
                
                // Update the displayed position
                CurrentPosition = new Point((int)Math.Round(_currentX), (int)Math.Round(_currentY));
                
                // Debug movement
                Console.WriteLine($"Fireball moved to: ({CurrentPosition.X},{CurrentPosition.Y})");
                
                // Check if reached target
                if (ReachedTarget())
                {
                    IsCompleted = true;
                    Console.WriteLine("Fireball reached target!");
                    break;
                }
                
                _accumulatedTime -= _updateIntervalMs;
            }
        }
        
        private bool ReachedTarget()
        {
            // Check if we've reached or passed the target
            return Math.Abs(CurrentPosition.X - TargetPosition.X) <= 1 && 
                   Math.Abs(CurrentPosition.Y - TargetPosition.Y) <= 1;
        }
        
        public void ApplyEffect()
        {
            // Apply damage to target
            if (Target != null && Target.IsAlive)
            {
                Target.TakeDamage(Damage);
                Console.WriteLine($"Fireball hit {Target.Name} for {Damage} damage!");
            }
        }
        
        public override void Draw(Terminal.Gui.View view)
        {
            // Only draw if position is valid
            if (CurrentPosition.X >= 0 && CurrentPosition.Y >= 0)
            {
                Terminal.Gui.Application.Driver.SetAttribute(Color);
                view.AddRune(CurrentPosition.X, CurrentPosition.Y, (Rune)Symbol);
            }
        }
    }
}