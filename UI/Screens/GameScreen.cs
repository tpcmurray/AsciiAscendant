using System;
using Terminal.Gui;
using AsciiAscendant.Core;
using AsciiAscendant.Core.Entities;

namespace AsciiAscendant.UI
{
    public class GameScreen : Window
    {
        private readonly GameState _gameState;
        private MapView _mapView;
        private StatusBar _statusBar;
        private SkillBar _skillBar;
        private Button _closeButton;
        private bool _processingKeyPress = false;
        
        // Add time-based key handling variables
        private DateTime _lastKeyPressTime = DateTime.MinValue;
        private const int KeyPressDelayMs = 150; // Minimum milliseconds between keypresses
        
        public GameScreen(GameState gameState) : base("ASCII Ascendant")
        {
            _gameState = gameState;
            
            // Set up the window to cover the full screen
            X = 0;
            Y = 0;
            Width = Dim.Fill();
            Height = Dim.Fill();
            
            // Create close button [X] at top left
            _closeButton = new Button("X") 
            {
                X = 0,
                Y = 0,
                Width = 1,
                Height = 1
            };
            _closeButton.Clicked += () => Application.RequestStop();
            Add(_closeButton);
            
            // Create the skill bar (positioned at the bottom)
            _skillBar = new SkillBar(_gameState);
            
            // Create the status bar
            _statusBar = new StatusBar(_gameState);
            
            // Create and add the map view
            _mapView = new MapView(_gameState)
            {
                X = 0,
                Y = 1, // Start after close button
                Width = Dim.Fill(),
                Height = Dim.Fill() - 1 // Leave room for close button
            };
            Add(_mapView);
            
            // Add the UI components
            Add(_skillBar);
            Add(_statusBar);
            
            // Update layout positions
            LayoutSubviews();
            
            // Set up key handling for player movement and quit
            KeyPress += GameScreen_KeyPress;
            
            // Listen for window resize events
            Application.Resized += (e) => {
                LayoutSubviews();
                Application.Refresh();
            };
        }
        
        // Override the LayoutSubviews method properly
        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            
            if (_mapView == null || _statusBar == null || _skillBar == null)
                return;
            
            // Get skill bar height as an int
            int skillBarHeight = _skillBar.GetBarHeight();
            
            // Position the skill bar at the bottom of the screen
            _skillBar.X = 0;
            _skillBar.Y = Pos.AnchorEnd(skillBarHeight);
            _skillBar.Width = Dim.Fill();
            _skillBar.Height = skillBarHeight;
            
            // Position the status bar above the skill bar (1 row)
            _statusBar.X = 0;
            _statusBar.Y = Pos.AnchorEnd(skillBarHeight + 1);
            _statusBar.Width = Dim.Fill();
            _statusBar.Height = 1;
            
            // Adjust map view to take remaining space
            _mapView.Height = Dim.Fill() - skillBarHeight - 2; // Top bar + skill bar + status bar
            
            // Update the display
            _mapView.SetNeedsDisplay();
            _statusBar.SetNeedsDisplay();
            _skillBar.SetNeedsDisplay();
        }
        
        private void GameScreen_KeyPress(KeyEventEventArgs e)
        {
            // Only allow keypress if enough time has elapsed since the last one
            var now = DateTime.Now;
            var timeSinceLastKeyPress = (now - _lastKeyPressTime).TotalMilliseconds;
            
            if (timeSinceLastKeyPress < KeyPressDelayMs)
            {
                return; // Ignore this keypress as it's too soon after the previous one
            }
            
            // Update last keypress time
            _lastKeyPressTime = now;
            
            // Check if we're already processing a key press to prevent multiple movements
            if (_processingKeyPress)
                return;
            
            _processingKeyPress = true;
            
            // Check for Ctrl+Q to quit the game
            if (e.KeyEvent.Key == Key.Q && e.KeyEvent.IsCtrl)
            {
                Application.RequestStop();
                _processingKeyPress = false;
                return;
            }
            
            // Handle player movement with WASD keys
            switch (e.KeyEvent.Key)
            {
                case Key.w:
                    Console.WriteLine($"W key pressed. Current position: {_gameState.Player.Position.X},{_gameState.Player.Position.Y}");
                    _gameState.Player.Move(_gameState.CurrentMap, 0, -1);
                    Console.WriteLine($"After Move call: {_gameState.Player.Position.X},{_gameState.Player.Position.Y}");
                    break;
                case Key.a:
                    Console.WriteLine($"A key pressed. Current position: {_gameState.Player.Position.X},{_gameState.Player.Position.Y}");
                    _gameState.Player.Move(_gameState.CurrentMap, -1, 0);
                    Console.WriteLine($"After Move call: {_gameState.Player.Position.X},{_gameState.Player.Position.Y}");
                    break;
                case Key.s:
                    Console.WriteLine($"S key pressed. Current position: {_gameState.Player.Position.X},{_gameState.Player.Position.Y}");
                    _gameState.Player.Move(_gameState.CurrentMap, 0, 1);
                    Console.WriteLine($"After Move call: {_gameState.Player.Position.X},{_gameState.Player.Position.Y}");
                    break;
                case Key.d:
                    Console.WriteLine($"D key pressed. Current position: {_gameState.Player.Position.X},{_gameState.Player.Position.Y}");
                    _gameState.Player.Move(_gameState.CurrentMap, 1, 0);
                    Console.WriteLine($"After Move call: {_gameState.Player.Position.X},{_gameState.Player.Position.Y}");
                    break;
                // Number key handlers for using skills
                case Key.D1:
                case Key.D2:
                case Key.D3:
                    int skillIndex = e.KeyEvent.Key == Key.D1 ? 0 : e.KeyEvent.Key == Key.D2 ? 1 : 2;
                    UsePlayerSkill(skillIndex);
                    break;
            }
            
            // Update the UI after player moves (but don't update enemies, that happens in the game loop)
            _statusBar.SetNeedsDisplay();
            _mapView.SetNeedsDisplay();
            _skillBar.SetNeedsDisplay();
            Application.Refresh(); // Force immediate refresh
            
            // Reset the flag after processing
            _processingKeyPress = false;
        }

        private void UsePlayerSkill(int skillIndex)
        {
            // Get the currently selected enemy
            var target = _mapView.GetSelectedEnemy();
            
            // Check if player has selected an enemy and if the skill index is valid
            if (target != null && skillIndex >= 0 && skillIndex < _gameState.Player.Skills.Count)
            {
                var skill = _gameState.Player.Skills[skillIndex];
                
                // Check if skill can be used (not on cooldown)
                if (skill.CanUse())
                {
                    // Set the skill on cooldown
                    skill.Use();
                    
                    Console.WriteLine($"Used {skill.Name} on {target.Name}");
                    
                    // For Fireball skill, create an animation instead of applying damage directly
                    if (skill.Name == "Fireball")
                    {
                        _gameState.CreateFireballAnimation(_gameState.Player.Position, target, skill.Damage);
                    }
                    else
                    {
                        // For other skills, apply damage directly
                        target.TakeDamage(skill.Damage);
                        
                        // Check if enemy was killed
                        if (!target.IsAlive)
                        {
                            target.Die(_gameState);
                            Console.WriteLine($"{target.Name} was defeated! Gained {target.ExperienceValue} experience.");
                            
                            // Clear the selected enemy since it's now dead
                            _mapView.SelectEnemy(null);
                        }
                    }
                    
                    // Update UI
                    _statusBar.SetNeedsDisplay();
                    _mapView.SetNeedsDisplay();
                    _skillBar.SetNeedsDisplay();
                    Application.Refresh();
                }
                else
                {
                    Console.WriteLine($"{skill.Name} is on cooldown. {skill.CurrentCooldown} turns remaining.");
                }
            }
            else if (target == null)
            {
                Console.WriteLine("No target selected. Click on an enemy to select it.");
            }
        }

        // Add method to refresh the display when game state is updated by game loop
        public void RefreshDisplay()
        {
            // Update the UI components
            _statusBar.SetNeedsDisplay();
            _mapView.SetNeedsDisplay();
            _skillBar.SetNeedsDisplay();
            
            // Force an immediate refresh of the UI
            Application.Refresh();
        }
    }
}