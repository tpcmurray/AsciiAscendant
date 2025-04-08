using Terminal.Gui;

namespace AsciiAscendant.Core.Animations
{
    public abstract class Animation
    {
        public bool IsCompleted { get; protected set; }
        
        public abstract void Update();
        public abstract void Draw(View view);
    }
}