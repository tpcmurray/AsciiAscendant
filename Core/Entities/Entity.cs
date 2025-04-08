using System;
using System.Collections.Generic;
using Terminal.Gui;

namespace AsciiAscendant.Core.Entities
{
    public abstract class Entity
    {
        public Point Position { get; private set; } = new Point(0, 0);
        public string Name { get; set; }
        public char Symbol { get; protected set; }
        
        // Replace single AsciiRepresentation with separate idle and movement representations
        public List<string> IdleAscii { get; protected set; }
        public List<string> MovementAscii { get; protected set; }
        
        // Track whether the entity is currently in a movement animation state
        public bool IsMoving { get; protected set; }
        
        // Timer for switching back to idle state after movement
        private DateTime _moveAnimationStartTime;
        private const int MovementAnimationDurationMs = 100;
        
        protected Entity(string name, char symbol)
        {
            Name = name;
            Symbol = symbol;
            IdleAscii = new List<string>();
            MovementAscii = new List<string>();
            IsMoving = false;
        }
        
        public virtual bool CanMoveTo(Map map, int x, int y)
        {
            return map.IsPassable(x, y);
        }
        
        public virtual void Update()
        {
            // Check if we need to switch back from movement animation to idle
            if (IsMoving && (DateTime.Now - _moveAnimationStartTime).TotalMilliseconds >= MovementAnimationDurationMs)
            {
                IsMoving = false;
            }
        }
        
        // Modified to move and trigger movement animation
        public virtual void MoveTo(Map map, int x, int y)
        {
            Point oldPosition = Position;
            Position = new Point(x, y);
            
            // Only trigger movement animation if position actually changed
            if (oldPosition.X != x || oldPosition.Y != y)
            {
                IsMoving = true;
                _moveAnimationStartTime = DateTime.Now;
            }
        }
        
        // Helper property to get the current ascii representation based on movement state
        public List<string> CurrentAscii => IsMoving ? 
            (MovementAscii.Count > 0 ? MovementAscii : IdleAscii) : 
            IdleAscii;
        
        // Gets render info without actually drawing
        public virtual (int x, int y, int width, int height) GetRenderDimensions()
        {
            var asciiRep = CurrentAscii;
            
            if (asciiRep != null && asciiRep.Count > 0)
            {
                int width = asciiRep[0].Length;
                int height = asciiRep.Count;
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