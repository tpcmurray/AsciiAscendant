using System;
using AsciiAscendant.Core.Entities;
using Terminal.Gui;

namespace AsciiAscendant.Core.Animations
{
    public class FireballAnimation : ProjectileAnimation
    {
        public FireballAnimation(Point source, Point target, Enemy targetEnemy, int damage)
            : base(
                source, 
                target, 
                '✸', // Fireball symbol
                new Terminal.Gui.Attribute(Terminal.Gui.Color.BrightRed, Terminal.Gui.Color.Black), // Red color
                50, // Update speed (ms)
                targetEnemy,
                damage)
        {
            // Log creation for debugging purposes
            // Console.WriteLine($"Created fireball: From ({source.X},{source.Y}) to ({target.X},{target.Y})");
            // Console.WriteLine($"Direction: ({DirectionX:F2},{DirectionY:F2}), Step: {MoveStep}");
        }
        
        public override void Update()
        {
            base.Update();
            
            // Debug movement - specific to fireball for now
            if (!IsCompleted)
            {
                // Console.WriteLine($"Fireball moved to: ({CurrentPosition.X},{CurrentPosition.Y})");
            }
        }
        
        protected override void OnTargetReached()
        {
            // Console.WriteLine("Fireball reached target!");
            base.OnTargetReached();
        }
        
        public override void ApplyEffect()
        {
            base.ApplyEffect();
            
            // Add fireball-specific effect (like logging)
            if (Target != null && Target.IsAlive)
            {
                // Console.WriteLine($"Fireball hit {Target.Name} for {Damage} damage!");
            }
        }
    }
}