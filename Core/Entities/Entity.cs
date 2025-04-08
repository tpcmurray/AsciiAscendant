using System;
using System.Collections.Generic;
using Terminal.Gui;

namespace AsciiAscendant.Core.Entities
{
    public abstract class Entity
    {
        public Point Position { get; set; } = new Point(0, 0);
        public string Name { get; set; }
        public char Symbol { get; protected set; }
        public List<string> AsciiRepresentation { get; protected set; }
        
        protected Entity(string name, char symbol)
        {
            Name = name;
            Symbol = symbol;
            AsciiRepresentation = new List<string>();
        }
        
        public virtual bool CanMoveTo(Map map, int x, int y)
        {
            return map.IsPassable(x, y);
        }
        
        public virtual void Update()
        {
            // Base update logic
        }
        
        // Gets render info without actually drawing
        public virtual (int x, int y, int width, int height) GetRenderDimensions()
        {
            if (AsciiRepresentation != null && AsciiRepresentation.Count > 0)
            {
                int width = AsciiRepresentation[0].Length;
                int height = AsciiRepresentation.Count;
                int centerOffsetX = width / 2;
                int centerOffsetY = height / 2;
                
                int topLeftX = Position.X - centerOffsetX;
                int topLeftY = Position.Y - centerOffsetY;
                
                return (topLeftX, topLeftY, width, height);
            }
            
            // Single character representation
            return (Position.X, Position.Y, 1, 1);
        }
        
        // Get the entity's color for rendering
        public virtual Terminal.Gui.Attribute GetEntityColor()
        {
            // Default entity color
            return new Terminal.Gui.Attribute(Color.White, Color.Black);
        }
    }
}