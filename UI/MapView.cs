using System;
using Terminal.Gui;
using AsciiAscendant.Core;
using AsciiAscendant.Core.Entities;
using AsciiAscendant.Core.Animations;
using AsciiAscendant.Core.Loot;
using System.Collections.Generic;

namespace AsciiAscendant.UI
{
    public class MapView : View
    {
        private readonly GameState _gameState;
        private Enemy? _selectedEnemy = null;
        
        // Screen shake properties
        private bool _isShaking = false;
        private int _shakeDuration = 0;
        private int _shakeIntensity = 0;
        private Random _shakeRandom = new Random();
        
        // ASCII particle effect properties
        private List<AsciiParticle> _particles = new List<AsciiParticle>();
        
        public MapView(GameState gameState)
        {
            _gameState = gameState;
            
            // Enable mouse tracking for enemy selection
            WantMousePositionReports = true;
            CanFocus = true;
        }
        
        // Trigger screen shake with specified duration and intensity
        public void StartShake(int duration, int intensity)
        {
            _isShaking = true;
            _shakeDuration = duration;
            _shakeIntensity = intensity;
        }
        
        // Create a particle effect at the specified position
        public void CreateParticleEffect(AsciiAscendant.Core.Point position, int particleCount, int duration, bool isDeathEffect = false)
        {
            char[] particleChars = isDeathEffect 
                ? new char[] { '*', '#', '+', '\\', '/', '|', '-' }  // Death effect chars
                : new char[] { '*', '!', '.', ',', '\'', '`' };      // Hit effect chars
                
            for (int i = 0; i < particleCount; i++)
            {
                int velX = _shakeRandom.Next(-2, 3);
                int velY = _shakeRandom.Next(-2, 3);
                char particleChar = particleChars[_shakeRandom.Next(particleChars.Length)];
                
                Color color = isDeathEffect 
                    ? (Color)_shakeRandom.Next((int)Color.Red, (int)Color.BrightYellow + 1)
                    : Color.White;
                
                _particles.Add(new AsciiParticle(
                    position.X, position.Y,
                    velX, velY,
                    particleChar,
                    color,
                    duration
                ));
            }
        }
        
        public void OnUpdateFrame()
        {
            // Update map camera to follow player
            var map = _gameState.CurrentMap;
            var player = _gameState.Player;
            
            // Center camera on player
            map.CenterCamera(player.Position.X, player.Position.Y);
            
            // Update particles
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                _particles[i].Update();
                if (_particles[i].IsExpired)
                {
                    _particles.RemoveAt(i);
                }
            }
            
            // Update screen shake
            if (_isShaking && _shakeDuration > 0)
            {
                _shakeDuration--;
                if (_shakeDuration <= 0)
                {
                    _isShaking = false;
                }
                
                // Need to refresh display each shake frame
                SetNeedsDisplay();
            }
        }
        
        // Get screen shake offset based on current shake state
        private AsciiAscendant.Core.Point GetShakeOffset()
        {
            if (!_isShaking || _shakeDuration <= 0)
                return new AsciiAscendant.Core.Point(0, 0);
                
            // Calculate random offset based on intensity
            int xOffset = _shakeRandom.Next(-_shakeIntensity, _shakeIntensity + 1);
            int yOffset = _shakeRandom.Next(-_shakeIntensity, _shakeIntensity + 1);
            
            return new AsciiAscendant.Core.Point(xOffset, yOffset);
        }
        
        public override bool MouseEvent(MouseEvent me)
        {
            if (me.Flags == MouseFlags.Button1Clicked)
            {
                var map = _gameState.CurrentMap;
                var visibleArea = map.GetVisibleArea();
                
                // Calculate actual map coordinates from screen position
                int mapX = me.X + visibleArea.startX;
                int mapY = me.Y + visibleArea.startY;
                
                _selectedEnemy = null;
                
                // Check each enemy to see if the mouse click is within its ASCII representation
                foreach (var enemy in _gameState.Enemies)
                {
                    if (enemy.IsAlive && IsPointInsideEntity(mapX, mapY, enemy))
                    {
                        _selectedEnemy = enemy;
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
        
        // Convert world coordinates to screen coordinates
        private (int screenX, int screenY) WorldToScreen(int worldX, int worldY)
        {
            var map = _gameState.CurrentMap;
            var visibleArea = map.GetVisibleArea();
            
            return (worldX - visibleArea.startX, worldY - visibleArea.startY);
        }
        
        // Convert screen coordinates to world coordinates
        private (int worldX, int worldY) ScreenToWorld(int screenX, int screenY)
        {
            var map = _gameState.CurrentMap;
            var visibleArea = map.GetVisibleArea();
            
            return (screenX + visibleArea.startX, screenY + visibleArea.startY);
        }
        
        public override void Redraw(Rect bounds)
        {
            base.Redraw(bounds);
            
            // Get screen shake offset
            var shakeOffset = GetShakeOffset();
            
            // Get visible map area
            var map = _gameState.CurrentMap;
            var (startX, startY, endX, endY) = map.GetVisibleArea();
            
            // Set viewport dimensions in map
            map.ViewportWidth = bounds.Width;
            map.ViewportHeight = bounds.Height;
            
            // Draw the visible map tiles
            for (int worldY = startY; worldY < endY; worldY++)
            {
                for (int worldX = startX; worldX < endX; worldX++)
                {
                    // Convert to screen coordinates
                    int screenX = worldX - startX;
                    int screenY = worldY - startY;
                    
                    // Apply shake offset
                    int drawX = screenX + shakeOffset.X;
                    int drawY = screenY + shakeOffset.Y;
                    
                    // Only draw if within bounds
                    if (drawX >= 0 && drawX < bounds.Width && drawY >= 0 && drawY < bounds.Height)
                    {
                        var tile = map.Tiles[worldX, worldY];
                        
                        // Use the rich tile data for rendering
                        Terminal.Gui.Attribute tileAttr;
                        
                        try
                        {
                            // Parse colors from strings
                            var fg = Enum.Parse<Color>(tile.ForegroundColor);
                            var bg = Enum.Parse<Color>(tile.BackgroundColor);
                            tileAttr = new Terminal.Gui.Attribute(fg, bg);
                        }
                        catch
                        {
                            // Fallback if color parsing fails
                            tileAttr = GetTileColor(tile);
                        }
                        
                        Driver.SetAttribute(tileAttr);
                        
                        // Use rich foreground character if available
                        if (!string.IsNullOrEmpty(tile.Foreground) && tile.Foreground.Length > 0)
                        {
                            AddRune(drawX, drawY, (Rune)tile.Foreground[0]);
                        }
                        else
                        {
                            // Use explicit method to get the symbol to avoid ambiguity
                            char symbol = GetTileSymbol(tile);
                            AddRune(drawX, drawY, (Rune)symbol);
                        }
                    }
                }
            }
            
            // Draw dropped items
            foreach (var item in _gameState.DroppedItems)
            {
                // Convert to screen coordinates
                var (screenX, screenY) = WorldToScreen(item.Position.X, item.Position.Y);
                
                // Apply shake offset
                int drawX = screenX + shakeOffset.X;
                int drawY = screenY + shakeOffset.Y;
                
                // Only draw item if within viewport
                if (drawX >= 0 && drawX < bounds.Width && drawY >= 0 && drawY < bounds.Height)
                {
                    DrawItem(item, drawX, drawY);
                }
            }
            
            // Draw all enemies
            foreach (var enemy in _gameState.Enemies)
            {
                if (!enemy.IsAlive) continue;
                
                // Convert to screen coordinates
                var (screenX, screenY) = WorldToScreen(enemy.Position.X, enemy.Position.Y);
                
                // Apply shake offset
                int drawX = screenX + shakeOffset.X;
                int drawY = screenY + shakeOffset.Y;
                
                // Only draw enemy if within viewport
                var (_, _, width, height) = enemy.GetRenderDimensions();
                if (drawX + width > 0 && drawX < bounds.Width && 
                    drawY + height > 0 && drawY < bounds.Height)
                {
                    // Draw selection indicators if this is the selected enemy
                    if (_selectedEnemy == enemy)
                    {
                        DrawSelectionIndicator(enemy, drawX, drawY, width, height);
                    }
                    
                    // Draw the enemy
                    DrawCreature(enemy, true, drawX, drawY);
                }
            }
            
            // Draw the player (always centered)
            int playerScreenX = bounds.Width / 2;
            int playerScreenY = bounds.Height / 2;
            
            // Apply shake offset
            int playerDrawX = playerScreenX + shakeOffset.X;
            int playerDrawY = playerScreenY + shakeOffset.Y;
            
            // Draw the player
            DrawCreature(_gameState.Player, false, playerDrawX, playerDrawY);
            
            // Draw damage numbers
            foreach (var enemy in _gameState.Enemies)
            {
                if (enemy.ActiveDamageNumbers.Count > 0)
                {
                    foreach (var damageNumber in enemy.ActiveDamageNumbers)
                    {
                        // Convert to screen coordinates
                        var (screenX, screenY) = WorldToScreen(damageNumber.Position.X, 
                            (int)(damageNumber.Position.Y - damageNumber.YOffset));
                        
                        // Apply shake offset
                        int drawX = screenX + shakeOffset.X;
                        int drawY = screenY + shakeOffset.Y;
                        
                        if (drawX >= 0 && drawX < bounds.Width && drawY >= 0 && drawY < bounds.Height)
                        {
                            DrawDamageNumber(damageNumber.Value, drawX, drawY);
                        }
                    }
                }
            }
            
            // Draw player damage numbers
            if (_gameState.Player.ActiveDamageNumbers.Count > 0)
            {
                foreach (var damageNumber in _gameState.Player.ActiveDamageNumbers)
                {
                    // Player is always centered, so damage numbers are relative to center
                    int drawX = playerScreenX + shakeOffset.X;
                    int drawY = (int)(playerScreenY - damageNumber.YOffset) + shakeOffset.Y;
                    
                    if (drawX >= 0 && drawX < bounds.Width && drawY >= 0 && drawY < bounds.Height)
                    {
                        DrawDamageNumber(damageNumber.Value, drawX, drawY);
                    }
                }
            }
            
            // Draw particles
            foreach (var particle in _particles)
            {
                // Convert to screen coordinates
                var (screenX, screenY) = WorldToScreen(particle.X, particle.Y);
                
                // Apply shake offset
                int drawX = screenX + shakeOffset.X;
                int drawY = screenY + shakeOffset.Y;
                
                // Draw if within bounds
                if (drawX >= 0 && drawX < bounds.Width && drawY >= 0 && drawY < bounds.Height)
                {
                    Driver.SetAttribute(new Terminal.Gui.Attribute(particle.Color, Color.Black));
                    AddRune(drawX, drawY, (Rune)particle.Symbol);
                }
            }
            
            // Draw all active animations
            foreach (var animation in _gameState.ActiveAnimations)
            {
                // Convert to screen coordinates and draw with viewport adjustment
                var (animScreenX, animScreenY) = WorldToScreen(animation.Position.X, animation.Position.Y);
                
                animation.DrawAtViewportCoordinates(this, animScreenX + shakeOffset.X, animScreenY + shakeOffset.Y);
            }
        }
        
        private void DrawDamageNumber(int damage, int x, int y)
        {
            // Convert damage to string and calculate width for centering
            string damageText = damage.ToString();
            int textWidth = damageText.Length;
            int startX = x - (textWidth / 2);
            
            // Draw with red color for visible damage
            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.BrightRed, Color.Black));
            
            // Draw each character
            for (int i = 0; i < damageText.Length; i++)
            {
                AddRune(startX + i, y, (Rune)damageText[i]);
            }
        }
        
        private void DrawCreature(Creature creature, bool isEnemy, int screenX, int screenY)
        {
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
                        int drawX = screenX + xOffset;
                        int drawY = screenY + yOffset;
                        
                        AddRune(drawX, drawY, (Rune)line[xOffset]);
                    }
                }
                
                // Draw health bar for creatures - moved up by one additional row
                float healthPercentage = creature.GetHealthPercentage();
                DrawHealthBar(screenX + currentAscii[0].Length / 2, screenY - 2, healthPercentage, creature.Name);
            }
            else
            {
                // Fallback to single character if no ASCII representation
                AddRune(screenX, screenY, (Rune)creature.Symbol);
                
                // Draw health bar for single-character creatures
                float healthPercentage = creature.GetHealthPercentage();
                DrawHealthBar(screenX, screenY - 2, healthPercentage, creature.Name);
            }
        }
        
        private void DrawItem(Item item, int screenX, int screenY)
        {
            // Use the item's own color method
            Driver.SetAttribute(item.GetEntityColor());
            
            // Draw the ASCII representation or symbol
            var currentAscii = item.CurrentAscii;
            if (currentAscii != null && currentAscii.Count > 0)
            {
                for (int yOffset = 0; yOffset < currentAscii.Count; yOffset++)
                {
                    string line = currentAscii[yOffset];
                    for (int xOffset = 0; xOffset < line.Length; xOffset++)
                    {
                        AddRune(screenX + xOffset, screenY + yOffset, (Rune)line[xOffset]);
                    }
                }
            }
            else
            {
                // Fallback to single character if no ASCII representation
                AddRune(screenX, screenY, (Rune)item.Symbol);
            }
        }
        
        private void DrawSelectionIndicator(Entity entity, int screenX, int screenY, int width, int height)
        {
            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.White, Color.Black));
            
            var currentAscii = entity.CurrentAscii;
            if (currentAscii != null && currentAscii.Count > 0)
            {
                // Draw selection brackets to the left and right
                int leftX = screenX - 2;
                int rightX = screenX + width + 1;
                
                for (int yOffset = 0; yOffset < height; yOffset++)
                {
                    int drawY = screenY + yOffset;
                    
                    // Draw left bracket
                    AddRune(leftX, drawY, (Rune)'[');
                    
                    // Draw right bracket
                    AddRune(rightX, drawY, (Rune)']');
                }
            }
            else
            {
                // Draw selection brackets to the left and right for single-character entities
                AddRune(screenX - 1, screenY, (Rune)'[');
                AddRune(screenX + 1, screenY, (Rune)']');
            }
        }
        
        private void DrawHealthBar(int screenX, int screenY, float percentage, string name)
        {
            // Draw creature name
            string nameDisplay = name.Length <= 10 ? name : name.Substring(0, 10);
            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.White, Color.Black));
            
            // Center the name above the entity
            int nameX = screenX - (nameDisplay.Length / 2);
            for (int i = 0; i < nameDisplay.Length; i++)
            {
                AddRune(nameX + i, screenY, (Rune)nameDisplay[i]);
            }
            
            // Draw health bar below the name
            int barWidth = 7;
            int filledWidth = (int)(barWidth * percentage);
            
            // Health bar color based on health percentage
            var healthBarColor = GetHealthBarColor(percentage);
            
            for (int i = 0; i < barWidth; i++)
            {
                int drawX = screenX - (barWidth / 2) + i;
                char barChar = i < filledWidth ? '█' : '░';
                Driver.SetAttribute(healthBarColor);
                AddRune(drawX, screenY + 1, (Rune)barChar);
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
        
        // Check if a skill is in range of the selected enemy
        public bool IsSkillInRange(Skill skill)
        {
            if (_selectedEnemy == null || !_selectedEnemy.IsAlive)
            {
                return false;
            }
            
            return skill.IsInRange(_gameState.Player.Position, _selectedEnemy.Position);
        }
        
        // Fallback tile color logic when rich colors aren't available
        private Terminal.Gui.Attribute GetTileColor(Tile tile)
        {
            return tile.TileType switch
            {
                TileType.Floor => new Terminal.Gui.Attribute(Color.Gray, Color.Black),
                TileType.Wall => new Terminal.Gui.Attribute(Color.White, Color.Black),
                TileType.Door => new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black),
                TileType.Water => new Terminal.Gui.Attribute(Color.Blue, Color.Black),
                TileType.Obstacle => new Terminal.Gui.Attribute(Color.DarkGray, Color.Black),
                _ => new Terminal.Gui.Attribute(Color.White, Color.Black)
            };
        }
        
        // Helper method to avoid ambiguity with the Symbol property
        private char GetTileSymbol(Tile tile)
        {
            switch (tile.TileType)
            {
                case TileType.Floor: return ' ';
                case TileType.Wall: return '#';
                case TileType.Door: return '+';
                case TileType.Water: return '~';
                case TileType.Obstacle: return 'o';
                default: return ' ';
            }
        }
    }
    
    // Enhanced animation draw interface for the scrolling viewport
    public static class AnimationExtensions
    {
        public static void DrawAtViewportCoordinates(this Animation animation, MapView view, int screenX, int screenY)
        {
            // Draw the animation at viewport coordinates
            animation.DrawAtPosition(view, new AsciiAscendant.Core.Point(screenX, screenY));
        }
    }
    
    // ASCII particle class for visual effects
    public class AsciiParticle
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        private readonly int _velX;
        private readonly int _velY;
        public char Symbol { get; }
        public Color Color { get; }
        private int _lifetime;
        public bool IsExpired => _lifetime <= 0;
        
        public AsciiParticle(int x, int y, int velX, int velY, char symbol, Color color, int lifetime)
        {
            X = x;
            Y = y;
            _velX = velX;
            _velY = velY;
            Symbol = symbol;
            Color = color;
            _lifetime = lifetime;
        }
        
        public void Update()
        {
            X += _velX;
            Y += _velY;
            _lifetime--;
        }
    }
}