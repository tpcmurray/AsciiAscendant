using System;

namespace AsciiAscendant.Core.Entities
{
    public abstract class Entity
    {
        public Point Position { get; set; } = new Point(0, 0);
        public string Name { get; set; }
        public char Symbol { get; protected set; }
        
        protected Entity(string name, char symbol)
        {
            Name = name;
            Symbol = symbol;
        }
        
        public virtual bool CanMoveTo(Map map, int x, int y)
        {
            return map.IsPassable(x, y);
        }
        
        public virtual void Update()
        {
            // Base update logic
        }
    }
}