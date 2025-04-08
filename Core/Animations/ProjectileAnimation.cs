using System;
using AsciiAscendant.Core.Entities;
using Terminal.Gui;

namespace AsciiAscendant.Core.Animations
{
    public abstract class ProjectileAnimation : Animation
    {
        public Point CurrentPosition { get; private set; }
        public Point TargetPosition { get; private set; }
        public char Symbol { get; protected set; }
        public Terminal.Gui.Attribute Color { get; protected set; }
        public Enemy? Target { get; private set; }
        public int Damage { get; protected set; }
        
        protected readonly int UpdateIntervalMs;
        protected readonly double DirectionX;
        protected readonly double DirectionY;
        protected readonly double MoveStep;
        protected double AccumulatedTime;
        protected DateTime LastUpdateTime;
        protected double CurrentX;
        protected double CurrentY;
        
        protected ProjectileAnimation(Point source, Point target, char symbol, Terminal.Gui.Attribute color, 
                                   int updateIntervalMs, Enemy? targetEnemy = null, int damage = 0)
        {
            CurrentPosition = source;
            TargetPosition = target;
            Symbol = symbol;
            Color = color;
            UpdateIntervalMs = updateIntervalMs;
            LastUpdateTime = DateTime.Now;
            Target = targetEnemy;
            Damage = damage;
            
            // Store initial position as double for precise movement
            CurrentX = source.X;
            CurrentY = source.Y;
            
            // Calculate direction vector
            int dx = target.X - source.X;
            int dy = target.Y - source.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            
            if (distance > 0)
            {
                // Store normalized direction vector as doubles
                DirectionX = dx / distance;
                DirectionY = dy / distance;
                MoveStep = 1.0; // One unit per step
            }
            else
            {
                DirectionX = 0;
                DirectionY = 0;
                MoveStep = 0;
                IsCompleted = true;
            }
        }
        
        public override void Update()
        {
            var now = DateTime.Now;
            AccumulatedTime += (now - LastUpdateTime).TotalMilliseconds;
            LastUpdateTime = now;
            
            // Check if it's time to move the projectile
            while (AccumulatedTime >= UpdateIntervalMs)
            {
                // Move in direction of target using floating point positions
                CurrentX += DirectionX * MoveStep;
                CurrentY += DirectionY * MoveStep;
                
                // Update the displayed position
                CurrentPosition = new Point((int)Math.Round(CurrentX), (int)Math.Round(CurrentY));
                
                // Check if reached target
                if (ReachedTarget())
                {
                    IsCompleted = true;
                    OnTargetReached();
                    break;
                }
                
                AccumulatedTime -= UpdateIntervalMs;
            }
        }
        
        protected virtual bool ReachedTarget()
        {
            // Check if we've reached or passed the target
            return Math.Abs(CurrentPosition.X - TargetPosition.X) <= 1 && 
                   Math.Abs(CurrentPosition.Y - TargetPosition.Y) <= 1;
        }
        
        // Template method for specialized projectiles to override
        protected virtual void OnTargetReached()
        {
            // Default implementation applies damage
            ApplyEffect();
        }
        
        public virtual void ApplyEffect()
        {
            // Apply damage to target
            if (Target != null && Target.IsAlive)
            {
                Target.TakeDamage(Damage);
            }
        }
        
        public override void Draw(View view)
        {
            // Only draw if position is valid
            if (CurrentPosition.X >= 0 && CurrentPosition.Y >= 0)
            {
                Application.Driver.SetAttribute(Color);
                view.AddRune(CurrentPosition.X, CurrentPosition.Y, (Rune)Symbol);
            }
        }
    }
}