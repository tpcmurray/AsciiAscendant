using System;
using Terminal.Gui;
using AsciiAscendant.Core;
using AsciiAscendant.Core.Entities;

namespace AsciiAscendant.UI
{
    public class MapView : View
    {
        private readonly GameState _gameState;
        private Enemy? _selectedEnemy = null;
        
        public MapView(GameState gameState)
        {
            _gameState = gameState;
            
            // Enable mouse tracking for enemy selection
            WantMousePositionReports = true;
            CanFocus = true;
        }
        
        public override bool MouseEvent(MouseEvent me)
        {
            if (me.Flags == MouseFlags.Button1Clicked)
            {
                // Check if the player clicked on an enemy
                int mouseX = me.X;
                int mouseY = me.Y;
                
                _selectedEnemy = null;
                
                // Check each enemy to see if the mouse click is within its ASCII representation
                foreach (var enemy in _gameState.Enemies)
                {
                    if (enemy.IsAlive && IsPointInsideEntity(mouseX, mouseY, enemy))
                    {
                        _selectedEnemy = enemy;
                        Console.WriteLine($"Selected enemy: {enemy.Name}");
                        break;
                    }
                }
                
                // Redraw to show selection indicator
                SetNeedsDisplay();
                return true;
            }
            
            return base.MouseEvent(me);
        }
        
        private bool IsPointInsideEntity(int x, int y, Entity entity)
        {
            var (entityX, entityY, width, height) = entity.GetRenderDimensions();
            
            return x >= entityX && x < entityX + width && 
                   y >= entityY && y < entityY + height;
        }
        
        public override void Redraw(Rect bounds)
        {
            base.Redraw(bounds);
            
            // Draw the map
            var map = _gameState.CurrentMap;
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    // Only draw what's visible in the viewport
                    if (x >= bounds.X && x < bounds.X + bounds.Width &&
                        y >= bounds.Y && y < bounds.Y + bounds.Height)
                    {
                        var tile = map.Tiles[x, y];
                        var symbol = tile.Symbol;
                        
                        Driver.SetAttribute(GetTileColor(tile));
                        AddRune(x, y, (Rune)symbol);
                    }
                }
            }
            
            // Draw all enemies
            foreach (var enemy in _gameState.Enemies)
            {
                // Get enemy render dimensions
                var (enemyX, enemyY, width, height) = enemy.GetRenderDimensions();
                
                // Only draw enemy if within viewport
                if (enemyX + width > bounds.X && enemyX < bounds.X + bounds.Width &&
                    enemyY + height > bounds.Y && enemyY < bounds.Y + bounds.Height)
                {
                    // Draw selection indicators if this is the selected enemy
                    if (_selectedEnemy == enemy)
                    {
                        DrawSelectionIndicator(enemy);
                    }
                    
                    // Draw the enemy
                    DrawCreature(enemy, true);
                }
            }
            
            // Draw the player
            DrawCreature(_gameState.Player, false);
            
            // Draw all active animations on top
            foreach (var animation in _gameState.ActiveAnimations)
            {
                animation.Draw(this);
            }
        }
        
        private void DrawCreature(Creature creature, bool isEnemy)
        {
            // Get creature render dimensions
            var (x, y, width, height) = creature.GetRenderDimensions();
            
            // Use the entity's own color method
            Driver.SetAttribute(creature.GetEntityColor());
            
            // Draw the ASCII representation or symbol
            var currentAscii = creature.CurrentAscii;
            if (currentAscii != null && currentAscii.Count > 0)
            {
                for (int yOffset = 0; yOffset < currentAscii.Count; yOffset++)
                {
                    string line = currentAscii[yOffset];
                    for (int xOffset = 0; xOffset < line.Length; xOffset++)
                    {
                        int drawX = x + xOffset;
                        int drawY = y + yOffset;
                        
                        // Make sure we're within map bounds
                        if (drawX >= 0 && drawX < _gameState.CurrentMap.Width && 
                            drawY >= 0 && drawY < _gameState.CurrentMap.Height)
                        {
                            AddRune(drawX, drawY, (Rune)line[xOffset]);
                        }
                    }
                }
                
                // Draw health bar for creatures - moved up by one additional row (y-2 instead of y-1)
                float healthPercentage = creature.GetHealthPercentage();
                DrawHealthBar(creature.Position.X, y - 2, healthPercentage, creature.Name);
            }
            else
            {
                // Fallback to single character if no ASCII representation
                if (x >= 0 && x < _gameState.CurrentMap.Width && y >= 0 && y < _gameState.CurrentMap.Height)
                {
                    AddRune(x, y, (Rune)creature.Symbol);
                    
                    // Draw health bar for single-character creatures - moved up by one row
                    float healthPercentage = creature.GetHealthPercentage();
                    DrawHealthBar(x, y - 2, healthPercentage, creature.Name);
                }
            }
        }
        
        private void DrawSelectionIndicator(Entity entity)
        {
            var (x, y, width, height) = entity.GetRenderDimensions();
            
            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.White, Color.Black));
            
            var currentAscii = entity.CurrentAscii;
            if (currentAscii != null && currentAscii.Count > 0)
            {
                // Draw selection brackets to the left and right
                int leftX = x - 2;
                int rightX = x + width + 1;
                
                for (int yOffset = 0; yOffset < height; yOffset++)
                {
                    int drawY = y + yOffset;
                    
                    // Draw left bracket
                    if (leftX >= 0 && leftX < _gameState.CurrentMap.Width && 
                        drawY >= 0 && drawY < _gameState.CurrentMap.Height)
                    {
                        AddRune(leftX, drawY, (Rune)'[');
                    }
                    
                    // Draw right bracket
                    if (rightX >= 0 && rightX < _gameState.CurrentMap.Width && 
                        drawY >= 0 && drawY < _gameState.CurrentMap.Height)
                    {
                        AddRune(rightX, drawY, (Rune)']');
                    }
                }
            }
            else
            {
                // Draw selection brackets to the left and right for single-character entities
                if (x - 1 >= 0 && x - 1 < _gameState.CurrentMap.Width)
                    AddRune(x - 1, y, (Rune)'[');
                
                if (x + 1 >= 0 && x + 1 < _gameState.CurrentMap.Width)
                    AddRune(x + 1, y, (Rune)']');
            }
        }
        
        private void DrawHealthBar(int x, int y, float percentage, string name)
        {
            if (y < 0 || y >= _gameState.CurrentMap.Height) return;
            
            // Draw creature name
            string nameDisplay = name.Length <= 10 ? name : name.Substring(0, 10);
            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.White, Color.Black));
            
            // Center the name above the entity
            int nameX = x - (nameDisplay.Length / 2);
            for (int i = 0; i < nameDisplay.Length; i++)
            {
                int drawX = nameX + i;
                if (drawX >= 0 && drawX < _gameState.CurrentMap.Width)
                {
                    AddRune(drawX, y, (Rune)nameDisplay[i]);
                }
            }
            
            // Draw health bar below the name
            if (y + 1 < _gameState.CurrentMap.Height)
            {
                int barWidth = 7;
                int filledWidth = (int)(barWidth * percentage);
                
                // Health bar color based on health percentage
                var healthBarColor = GetHealthBarColor(percentage);
                
                for (int i = 0; i < barWidth; i++)
                {
                    int drawX = x - (barWidth / 2) + i;
                    if (drawX >= 0 && drawX < _gameState.CurrentMap.Width)
                    {
                        char barChar = i < filledWidth ? '█' : '░';
                        Driver.SetAttribute(healthBarColor);
                        AddRune(drawX, y + 1, (Rune)barChar);
                    }
                }
            }
        }
        
        private Terminal.Gui.Attribute GetHealthBarColor(float percentage)
        {
            if (percentage > 0.7f)
                return new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black);
            else if (percentage > 0.3f)
                return new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black);
            else
                return new Terminal.Gui.Attribute(Color.BrightRed, Color.Black);
        }
        
        // Get the selected enemy (for skill usage)
        public Enemy? GetSelectedEnemy()
        {
            return _selectedEnemy;
        }
        
        // Method to select an enemy programmatically (if needed)
        public void SelectEnemy(Enemy? enemy)
        {
            _selectedEnemy = enemy;
            SetNeedsDisplay();
        }
        
        private Terminal.Gui.Attribute GetTileColor(Tile tile)
        {
            return tile.Type switch
            {
                TileType.Floor => new Terminal.Gui.Attribute(Color.Gray, Color.Black),
                TileType.Wall => new Terminal.Gui.Attribute(Color.White, Color.Black),
                TileType.Door => new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black),
                TileType.Water => new Terminal.Gui.Attribute(Color.Blue, Color.Black),
                _ => new Terminal.Gui.Attribute(Color.White, Color.Black)
            };
        }
    }
}