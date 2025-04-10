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
        
        // Hitbox variables
        public int HitboxWidth => CurrentAscii.Count > 0 ? CurrentAscii[0].Length : 1;
        public int HitboxHeight => CurrentAscii.Count > 0 ? CurrentAscii.Count : 1;
        
        // Flash when hit
        private DateTime _hitFlashTime = DateTime.MinValue;
        private const int HitFlashDurationMs = 200;
        public bool IsFlashing => (DateTime.Now - _hitFlashTime).TotalMilliseconds < HitFlashDurationMs;
        
        protected Entity(string name, char symbol)
        {
            Name = name;
            Symbol = symbol;
            IdleAscii = new List<string>();
            MovementAscii = new List<string>();
            IsMoving = false;
        }
        
        // Calculate hitbox corners
        public (int left, int top, int right, int bottom) GetHitbox()
        {
            int left = Position.X - (HitboxWidth / 2);
            int top = Position.Y - (HitboxHeight / 2);
            int right = left + HitboxWidth - 1;
            int bottom = top + HitboxHeight - 1;
            
            return (left, top, right, bottom);
        }
        
        // Check collision with another entity
        public bool CollidesWith(Entity other)
        {
            var (myLeft, myTop, myRight, myBottom) = GetHitbox();
            var (otherLeft, otherTop, otherRight, otherBottom) = other.GetHitbox();
            
            return !(otherLeft > myRight || 
                    otherRight < myLeft || 
                    otherTop > myBottom ||
                    otherBottom < myTop);
        }
        
        // Modified to check for entity collisions (no actual entity collision checking yet)
        public virtual bool CanMoveTo(Map map, int x, int y)
        {
            // Create a temporary position to check collision
            Point originalPos = Position;
            Position = new Point(x, y);
            
            // First check if the tile is passable
            bool passable = map.IsPassable(x, y);
            
            // We can't access GameState from here, so we'll just check map tiles for now
            // Entity collision will be checked by the calling code in GameState
            
            // Restore original position
            Position = originalPos;
            
            return passable;
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
        
        // Get the entity's color for rendering (now considers flashing)
        public virtual Terminal.Gui.Attribute GetEntityColor()
        {
            // Return red background if flashing from hit
            if (IsFlashing)
            {
                return new Terminal.Gui.Attribute(Color.White, Color.Red);
            }
            
            // Default entity color
            return new Terminal.Gui.Attribute(Color.White, Color.Black);
        }
        
        public void Flash()
        {
            _hitFlashTime = DateTime.Now;
        }
    }
}