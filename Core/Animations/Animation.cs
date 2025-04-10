using Terminal.Gui;
using AsciiAscendant.UI;

namespace AsciiAscendant.Core.Animations
{
    public abstract class Animation
    {
        public bool IsCompleted { get; protected set; }
        
        public abstract void Update();
        
        // Original draw method - maintaining for backwards compatibility
        public virtual void Draw(View view)
        {
            // Default implementation calls the new method with zero offset
            Draw(view, new Point(0, 0));
        }
        
        // New draw method with shake offset
        public abstract void Draw(View view, Point shakeOffset);
    }
}