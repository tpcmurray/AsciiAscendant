using System;
using AsciiAscendant.Core.Entities;
using Terminal.Gui;

namespace AsciiAscendant.Core.Animations
{
    public class ArrowAnimation : ProjectileAnimation
    {
        public ArrowAnimation(Point source, Point target, Enemy targetEnemy, int damage)
            : base(
                source, 
                target, 
                '→', // Default arrow symbol (will be rotated based on direction)
                new Terminal.Gui.Attribute(Terminal.Gui.Color.White, Terminal.Gui.Color.Black), // White color
                40, // Update speed (ms) - faster than fireball
                targetEnemy,
                damage)
        {
            // Update the arrow symbol based on the direction
            UpdateArrowSymbol();
            
            // Console.WriteLine($"Created arrow: From ({source.X},{source.Y}) to ({target.X},{target.Y})");
        }
        
        private void UpdateArrowSymbol()
        {
            // Calculate the angle of the arrow in radians
            double angle = Math.Atan2(DirectionY, DirectionX);
            
            // Convert to degrees
            double degrees = angle * 180 / Math.PI;
            
            // Choose appropriate arrow symbol based on angle
            if (degrees >= -22.5 && degrees < 22.5)
                Symbol = '→'; // Right
            else if (degrees >= 22.5 && degrees < 67.5)
                Symbol = '↘'; // Down-right
            else if (degrees >= 67.5 && degrees < 112.5)
                Symbol = '↓'; // Down
            else if (degrees >= 112.5 && degrees < 157.5)
                Symbol = '↙'; // Down-left
            else if (degrees >= 157.5 || degrees < -157.5)
                Symbol = '←'; // Left
            else if (degrees >= -157.5 && degrees < -112.5)
                Symbol = '↖'; // Up-left
            else if (degrees >= -112.5 && degrees < -67.5)
                Symbol = '↑'; // Up
            else if (degrees >= -67.5 && degrees < -22.5)
                Symbol = '↗'; // Up-right
        }
        
        public override void Update()
        {
            base.Update();
            
            // Debug movement
            if (!IsCompleted)
            {
                // Console.WriteLine($"Arrow moved to: ({CurrentPosition.X},{CurrentPosition.Y})");
            }
        }
        
        protected override void OnTargetReached()
        {
            // Console.WriteLine("Arrow hit target!");
            base.OnTargetReached();
        }
        
        public override void ApplyEffect()
        {
            base.ApplyEffect();
            
            // Add arrow-specific effect
            if (Target != null && Target.IsAlive)
            {
                // Console.WriteLine($"Arrow hit {Target.Name} for {Damage} damage!");
            }
        }
    }
}