using System;
using System.Collections.Generic;

namespace AsciiAscendant.Core.Entities
{
    public class BasicEnemy : Enemy
    {
        private Random _random = new Random();
        
        public BasicEnemy(string name, int maxHealth, int damage, int experienceValue) 
            : base(name, 'E', maxHealth, damage, experienceValue, 8)
        {
            // Initialize ASCII representation for this enemy type
            AsciiRepresentation = new List<string>
            {
                " /^\\ ",
                "<o_o>",
                " \\_/ "
            };
        }
        
        public override void TakeTurn(GameState gameState)
        {
            // If dead, do nothing
            if (!IsAlive) return;
            
            // Simple AI: If can see player, move toward player
            if (CanSeePlayer(gameState))
            {
                MoveTowardPlayer(gameState);
                
                // If adjacent to player, attack
                if (IsAdjacentTo(gameState.Player))
                {
                    Attack(gameState.Player);
                }
            }
            else
            {
                // Random movement when player not detected
                RandomMove(gameState.CurrentMap);
            }
        }
        
        private void MoveTowardPlayer(GameState gameState)
        {
            int dx = Math.Sign(gameState.Player.Position.X - Position.X);
            int dy = Math.Sign(gameState.Player.Position.Y - Position.Y);
            
            // Try to move in the direction of the player
            Move(gameState.CurrentMap, dx, dy);
        }
        
        private void RandomMove(Map map)
        {
            // Randomly move in one of the cardinal directions
            int direction = _random.Next(4);
            switch (direction)
            {
                case 0: Move(map, 0, -1); break; // North
                case 1: Move(map, 1, 0); break;  // East
                case 2: Move(map, 0, 1); break;  // South
                case 3: Move(map, -1, 0); break; // West
            }
        }
        
        private bool IsAdjacentTo(Creature other)
        {
            int dx = Math.Abs(Position.X - other.Position.X);
            int dy = Math.Abs(Position.Y - other.Position.Y);
            
            // Check if directly adjacent (including diagonals)
            return dx <= 1 && dy <= 1;
        }
    }
}