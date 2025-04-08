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
        
        private bool IsPointInsideEntity(int x, int y, Creature entity)
        {
            if (entity.AsciiRepresentation == null || entity.AsciiRepresentation.Count == 0)
            {
                // For simple entities with just a character, check exact position
                return x == entity.Position.X && y == entity.Position.Y;
            }
            
            // For multi-line ASCII representation, check if point is within the bounds
            int centerOffsetX = entity.AsciiRepresentation[0].Length / 2;
            int centerOffsetY = entity.AsciiRepresentation.Count / 2;
            
            int minX = entity.Position.X - centerOffsetX;
            int maxX = minX + entity.AsciiRepresentation[0].Length - 1;
            int minY = entity.Position.Y - centerOffsetY;
            int maxY = minY + entity.AsciiRepresentation.Count - 1;
            
            return x >= minX && x <= maxX && y >= minY && y <= maxY;
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
            
            // Draw all enemies with their ASCII representation
            foreach (var enemy in _gameState.Enemies)
            {
                int enemyX = enemy.Position.X;
                int enemyY = enemy.Position.Y;
                
                // Only draw enemy if within viewport
                if (enemyX >= bounds.X && enemyX < bounds.X + bounds.Width &&
                    enemyY >= bounds.Y && enemyY < bounds.Y + bounds.Height)
                {
                    // Set different colors based on enemy health percentage
                    var healthPercentage = (float)enemy.Health / enemy.MaxHealth;
                    Driver.SetAttribute(GetEnemyColor(healthPercentage));
                    
                    // If the enemy has a multi-line representation, draw it
                    if (enemy.AsciiRepresentation != null && enemy.AsciiRepresentation.Count > 0)
                    {
                        // Calculate the starting position to center the enemy's ASCII art
                        int centerOffsetX = enemy.AsciiRepresentation[0].Length / 2;
                        int centerOffsetY = enemy.AsciiRepresentation.Count / 2;
                        
                        // Draw selection indicators if this is the selected enemy
                        if (_selectedEnemy == enemy)
                        {
                            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.White, Color.Black));
                            
                            // Draw selection brackets to the left and right
                            int leftX = enemyX - centerOffsetX - 2;
                            int rightX = enemyX + centerOffsetX + 1;
                            
                            for (int y = 0; y < enemy.AsciiRepresentation.Count; y++)
                            {
                                int drawY = enemyY - centerOffsetY + y;
                                
                                // Draw left bracket
                                if (leftX >= 0 && leftX < map.Width && drawY >= 0 && drawY < map.Height)
                                {
                                    AddRune(leftX, drawY, (Rune)'[');
                                }
                                
                                // Draw right bracket
                                if (rightX >= 0 && rightX < map.Width && drawY >= 0 && drawY < map.Height)
                                {
                                    AddRune(rightX, drawY, (Rune)']');
                                }
                            }
                            
                            // Reset color for drawing enemy
                            Driver.SetAttribute(GetEnemyColor(healthPercentage));
                        }
                        
                        for (int y = 0; y < enemy.AsciiRepresentation.Count; y++)
                        {
                            string line = enemy.AsciiRepresentation[y];
                            for (int x = 0; x < line.Length; x++)
                            {
                                // Calculate the position relative to the enemy's center
                                int drawX = enemyX - centerOffsetX + x;
                                int drawY = enemyY - centerOffsetY + y;
                                
                                // Make sure we're within the bounds
                                if (drawX >= 0 && drawX < map.Width && drawY >= 0 && drawY < map.Height)
                                {
                                    AddRune(drawX, drawY, (Rune)line[x]);
                                }
                            }
                        }
                        
                        // Draw health bar above enemy
                        DrawHealthBar(enemyX, enemyY - centerOffsetY - 1, healthPercentage, enemy.Name);
                    }
                    else
                    {
                        // Draw selection indicators if this is the selected enemy
                        if (_selectedEnemy == enemy)
                        {
                            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.White, Color.Black));
                            
                            // Draw selection brackets to the left and right
                            if (enemyX - 1 >= 0)
                                AddRune(enemyX - 1, enemyY, (Rune)'[');
                            
                            if (enemyX + 1 < map.Width)
                                AddRune(enemyX + 1, enemyY, (Rune)']');
                            
                            // Reset color for drawing enemy
                            Driver.SetAttribute(GetEnemyColor(healthPercentage));
                        }
                        
                        // Fallback to single character if no ASCII representation
                        AddRune(enemyX, enemyY, (Rune)enemy.Symbol);
                        
                        // Draw health bar above enemy
                        DrawHealthBar(enemyX, enemyY - 1, healthPercentage, enemy.Name);
                    }
                }
            }
            
            // Draw the player with multi-line ASCII representation
            var player = _gameState.Player;
            int playerX = player.Position.X;
            int playerY = player.Position.Y;
            
            // Only draw player if within viewport
            if (playerX >= bounds.X && playerX < bounds.X + bounds.Width &&
                playerY >= bounds.Y && playerY < bounds.Y + bounds.Height)
            {
                Driver.SetAttribute(new Terminal.Gui.Attribute(Color.White, Color.Black));
                
                // If the player has a multi-line representation, draw it
                if (player.AsciiRepresentation != null && player.AsciiRepresentation.Count > 0)
                {
                    // Calculate the starting position to center the player's ASCII art
                    int centerOffsetX = player.AsciiRepresentation[0].Length / 2;
                    int centerOffsetY = player.AsciiRepresentation.Count / 2;
                    
                    for (int y = 0; y < player.AsciiRepresentation.Count; y++)
                    {
                        string line = player.AsciiRepresentation[y];
                        for (int x = 0; x < line.Length; x++)
                        {
                            // Calculate the position relative to the player's center
                            int drawX = playerX - centerOffsetX + x;
                            int drawY = playerY - centerOffsetY + y;
                            
                            // Make sure we're within the bounds
                            if (drawX >= 0 && drawX < map.Width && drawY >= 0 && drawY < map.Height)
                            {
                                AddRune(drawX, drawY, (Rune)line[x]);
                            }
                        }
                    }
                }
                else
                {
                    // Fallback to single character if no ASCII representation
                    AddRune(playerX, playerY, (Rune)player.Symbol);
                }
            }
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
        
        private void DrawHealthBar(int x, int y, float percentage, string name)
        {
            if (y < 0 || y >= _gameState.CurrentMap.Height) return;
            
            // Draw enemy name
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
        
        private Terminal.Gui.Attribute GetEnemyColor(float healthPercentage)
        {
            if (healthPercentage > 0.7f)
                return new Terminal.Gui.Attribute(Color.BrightRed, Color.Black);
            else if (healthPercentage > 0.3f)
                return new Terminal.Gui.Attribute(Color.Red, Color.Black);
            else
                return new Terminal.Gui.Attribute(Color.Red, Color.Black);
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