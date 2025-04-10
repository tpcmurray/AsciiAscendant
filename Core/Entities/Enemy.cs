using System;
using System.Collections.Generic;
using Terminal.Gui;
using AsciiAscendant.Core.Loot;

namespace AsciiAscendant.Core.Entities
{
    public abstract class Enemy : Creature
    {
        public int ExperienceValue { get; protected set; }
        public int DetectionRange { get; protected set; }
        public LootTable LootTable { get; protected set; }
        protected Random _random = new Random();
        
        public bool IsRanged { get; protected set; }
        public int AttackRange { get; protected set; }
        
        // Add aggro state properties
        public bool IsAggro { get; protected set; }
        public const int AggroMovementMultiplier = 3;
        
        protected Enemy(string name, char symbol, int maxHealth, int damage, int experienceValue, int detectionRange) 
            : base(name, symbol, maxHealth, damage)
        {
            ExperienceValue = experienceValue;
            DetectionRange = detectionRange;
            LootTable = new LootTable();
            IsRanged = false;
            AttackRange = 1;
            IsAggro = false;
            
            SetupLootTable();
        }
        
        protected virtual void SetupLootTable()
        {
        }
        
        public virtual void TakeTurn(GameState gameState)
        {
            if (!IsAlive) return;
            
            int distanceToPlayer = GetDistanceToPlayer(gameState);
            
            if (IsAggro || CanSeePlayer(gameState))
            {
                if (IsRanged && distanceToPlayer <= AttackRange)
                {
                    // Within ranged attack range
                    RangedAttack(gameState);
                    // No longer needs to be in aggro state since it can attack
                    IsAggro = false;
                }
                else if (IsRanged && distanceToPlayer > AttackRange)
                {
                    // Move aggressively toward player for ranged enemies
                    MoveTowardPlayer(gameState);
                }
                else if (!IsRanged)
                {
                    // For melee enemies
                    MoveTowardPlayer(gameState);
                    
                    if (IsAdjacentTo(gameState.Player))
                    {
                        Attack(gameState.Player);
                        // No longer needs to be in aggro state since it can attack
                        IsAggro = false;
                    }
                }
            }
            else
            {
                RandomMove(gameState.CurrentMap);
            }
        }
        
        protected virtual void RangedAttack(GameState gameState)
        {
        }
        
        protected void MoveTowardPlayer(GameState gameState)
        {
            int dx = Math.Sign(gameState.Player.Position.X - Position.X);
            int dy = Math.Sign(gameState.Player.Position.Y - Position.Y);
            
            // If in aggro state, attempt multiple moves per turn
            if (IsAggro)
            {
                for (int i = 0; i < AggroMovementMultiplier; i++)
                {
                    // Check if we're already in attack range
                    if ((IsRanged && GetDistanceToPlayer(gameState) <= AttackRange) ||
                        (!IsRanged && IsAdjacentTo(gameState.Player)))
                    {
                        // Stop moving if we're in attack range
                        break;
                    }
                    
                    Move(gameState.CurrentMap, dx, dy);
                }
            }
            else
            {
                // Normal movement (1 step per turn)
                Move(gameState.CurrentMap, dx, dy);
            }
        }
        
        protected void RandomMove(Map map)
        {
            Point oldPosition = Position;
            
            int direction = _random.Next(4);
            switch (direction)
            {
                case 0: Move(map, 0, -1); break;
                case 1: Move(map, 1, 0); break;
                case 2: Move(map, 0, 1); break;
                case 3: Move(map, -1, 0); break;
            }
            
            if (oldPosition.X == Position.X && oldPosition.Y == Position.Y)
            {
                IsMoving = false;
            }
        }
        
        protected bool IsAdjacentTo(Creature other)
        {
            int dx = Math.Abs(Position.X - other.Position.X);
            int dy = Math.Abs(Position.Y - other.Position.Y);
            
            return dx <= 1 && dy <= 1;
        }
        
        protected int GetDistanceToPlayer(GameState gameState)
        {
            int dx = Math.Abs(Position.X - gameState.Player.Position.X);
            int dy = Math.Abs(Position.Y - gameState.Player.Position.Y);
            
            return (int)Math.Sqrt(dx * dx + dy * dy);
        }
        
        public bool CanSeePlayer(GameState gameState)
        {
            return GetDistanceToPlayer(gameState) <= DetectionRange;
        }
        
        public override void TakeDamage(int amount)
        {
            base.TakeDamage(amount);
            
            // When damaged, the enemy becomes aggressive (assumes player is the attacker)
            IsAggro = true;
        }
        
        public virtual void Die(GameState gameState)
        {
            gameState.Player.GainExperience(ExperienceValue);
            DropLoot(gameState);
            gameState.RemoveEnemy(this);
        }
        
        protected virtual void DropLoot(GameState gameState)
        {
            List<Item> droppedItems = LootGenerator.GenerateLoot(LootTable, Level);
            
            foreach (var item in droppedItems)
            {
                item.MoveTo(gameState.CurrentMap, Position.X, Position.Y);
                gameState.AddDroppedItem(item);
            }
        }
        
        public override Terminal.Gui.Attribute GetEntityColor()
        {
            float healthPercentage = GetHealthPercentage();
            
            if (healthPercentage > 0.7f)
                return new Terminal.Gui.Attribute(Color.Green, Color.Black);
            else if (healthPercentage > 0.3f)
                return new Terminal.Gui.Attribute(Color.Brown, Color.Black);
            else
                return new Terminal.Gui.Attribute(Color.Red, Color.Black);
        }
    }
}