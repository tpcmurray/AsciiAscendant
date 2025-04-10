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
                        // Console.WriteLine($"Selected enemy: {enemy.Name}");
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
            
            // Get screen shake offset
            var shakeOffset = GetShakeOffset();
            
            // Draw the map
            var map = _gameState.CurrentMap;
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    // Only draw what's visible in the viewport
                    // Apply shake offset to rendering positions
                    int drawX = x + shakeOffset.X;
                    int drawY = y + shakeOffset.Y;
                    
                    if (drawX >= bounds.X && drawX < bounds.X + bounds.Width &&
                        drawY >= bounds.Y && drawY < bounds.Y + bounds.Height)
                    {
                        var tile = map.Tiles[x, y];
                        var symbol = tile.Symbol;
                        
                        Driver.SetAttribute(GetTileColor(tile));
                        AddRune(drawX, drawY, (Rune)symbol);
                    }
                }
            }
            
            // Draw dropped items with shake offset
            foreach (var item in _gameState.DroppedItems)
            {
                // Get item render dimensions
                var (itemX, itemY, width, height) = item.GetRenderDimensions();
                
                // Apply shake offset
                itemX += shakeOffset.X;
                itemY += shakeOffset.Y;
                
                // Only draw item if within viewport
                if (itemX + width > bounds.X && itemX < bounds.X + bounds.Width &&
                    itemY + height > bounds.Y && itemY < bounds.Y + bounds.Height)
                {
                    DrawItem(item, shakeOffset);
                }
            }
            
            // Draw all enemies with shake offset
            foreach (var enemy in _gameState.Enemies)
            {
                // Get enemy render dimensions
                var (enemyX, enemyY, width, height) = enemy.GetRenderDimensions();
                
                // Apply shake offset
                enemyX += shakeOffset.X;
                enemyY += shakeOffset.Y;
                
                // Only draw enemy if within viewport
                if (enemyX + width > bounds.X && enemyX < bounds.X + bounds.Width &&
                    enemyY + height > bounds.Y && enemyY < bounds.Y + bounds.Height)
                {
                    // Draw selection indicators if this is the selected enemy
                    if (_selectedEnemy == enemy)
                    {
                        DrawSelectionIndicator(enemy, shakeOffset);
                    }
                    
                    // Draw the enemy
                    DrawCreature(enemy, true, shakeOffset);
                }
            }
            
            // Draw the player with shake offset
            DrawCreature(_gameState.Player, false, shakeOffset);
            
            // Draw damage numbers for creatures with shake offset
            foreach (var enemy in _gameState.Enemies)
            {
                if (enemy.ActiveDamageNumbers.Count > 0)
                {
                    DrawDamageNumbers(enemy, shakeOffset);
                }
            }
            
            // Draw player damage numbers with shake offset
            if (_gameState.Player.ActiveDamageNumbers.Count > 0)
            {
                DrawDamageNumbers(_gameState.Player, shakeOffset);
            }
            
            // Draw particles with shake offset
            foreach (var particle in _particles)
            {
                int drawX = particle.X + shakeOffset.X;
                int drawY = particle.Y + shakeOffset.Y;
                
                // Make sure we're within map bounds
                if (drawX >= 0 && drawX < _gameState.CurrentMap.Width && 
                    drawY >= 0 && drawY < _gameState.CurrentMap.Height)
                {
                    Driver.SetAttribute(new Terminal.Gui.Attribute(particle.Color, Color.Black));
                    AddRune(drawX, drawY, (Rune)particle.Symbol);
                }
            }
            
            // Draw all active animations on top with shake offset
            foreach (var animation in _gameState.ActiveAnimations)
            {
                animation.Draw(this, shakeOffset);
            }
        }
        
        private void DrawDamageNumbers(Creature creature, AsciiAscendant.Core.Point shakeOffset)
        {
            foreach (var damageNumber in creature.ActiveDamageNumbers)
            {
                // Calculate position with rising effect
                int x = damageNumber.Position.X + shakeOffset.X;
                int y = (int)(damageNumber.Position.Y - damageNumber.YOffset) + shakeOffset.Y;
                
                // Make sure we're within map bounds
                if (x >= 0 && x < _gameState.CurrentMap.Width && 
                    y >= 0 && y < _gameState.CurrentMap.Height)
                {
                    // Convert damage to string and calculate width for centering
                    string damageText = damageNumber.Value.ToString();
                    int textWidth = damageText.Length;
                    int startX = x - (textWidth / 2);
                    
                    // Draw with red color for visible damage
                    Driver.SetAttribute(new Terminal.Gui.Attribute(Color.BrightRed, Color.Black));
                    
                    // Draw each character
                    for (int i = 0; i < damageText.Length; i++)
                    {
                        int drawX = startX + i;
                        if (drawX >= 0 && drawX < _gameState.CurrentMap.Width)
                        {
                            AddRune(drawX, y, (Rune)damageText[i]);
                        }
                    }
                }
            }
        }
        
        private void DrawCreature(Creature creature, bool isEnemy, AsciiAscendant.Core.Point shakeOffset)
        {
            // Get creature render dimensions
            var (x, y, width, height) = creature.GetRenderDimensions();
            
            // Apply shake offset
            x += shakeOffset.X;
            y += shakeOffset.Y;
            
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
                DrawHealthBar(creature.Position.X + shakeOffset.X, y - 2, healthPercentage, creature.Name);
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
        
        private void DrawItem(Item item, AsciiAscendant.Core.Point shakeOffset)
        {
            // Get item render dimensions
            var (x, y, width, height) = item.GetRenderDimensions();
            
            // Apply shake offset
            x += shakeOffset.X;
            y += shakeOffset.Y;
            
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
            }
            else
            {
                // Fallback to single character if no ASCII representation
                if (x >= 0 && x < _gameState.CurrentMap.Width && y >= 0 && y < _gameState.CurrentMap.Height)
                {
                    AddRune(x, y, (Rune)item.Symbol);
                }
            }
        }
        
        private void DrawSelectionIndicator(Entity entity, AsciiAscendant.Core.Point shakeOffset)
        {
            var (x, y, width, height) = entity.GetRenderDimensions();
            
            // Apply shake offset
            x += shakeOffset.X;
            y += shakeOffset.Y;
            
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
        
        // Check if a skill is in range of the selected enemy
        public bool IsSkillInRange(Skill skill)
        {
            if (_selectedEnemy == null || !_selectedEnemy.IsAlive)
            {
                return false;
            }
            
            return skill.IsInRange(_gameState.Player.Position, _selectedEnemy.Position);
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