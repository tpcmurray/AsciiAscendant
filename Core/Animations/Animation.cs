using Terminal.Gui;
using AsciiAscendant.UI;

namespace AsciiAscendant.Core.Animations
{
    public abstract class Animation
    {
        public bool IsCompleted { get; protected set; }
        public Point Position { get; protected set; } = new Point(0, 0); // Initialize with default position
        
        public abstract void Update();
        
        // Original draw method - maintaining for backwards compatibility
        public virtual void Draw(View view)
        {
            // Default implementation calls the new method with zero offset
            Draw(view, new Point(0, 0));
        }
        
        // New draw method with shake offset
        public abstract void Draw(View view, Point shakeOffset);
        
        // Draw the animation at a specific position (for viewport rendering)
        public virtual void DrawAtPosition(View view, Point position)
        {
            // Store the original position
            Point originalPosition = Position;
            
            // Temporarily set the position to the provided position
            Position = position;
            
            // Draw the animation at this position with no additional offset
            Draw(view, new Point(0, 0));
            
            // Restore the original position
            Position = originalPosition;
        }
    }
}