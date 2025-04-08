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
            // Check for Ctrl+Q to quit the game
            if (e.KeyEvent.Key == Key.Q && e.KeyEvent.IsCtrl)
            {
                Application.RequestStop();
                return;
            }
            
            bool playerMoved = false;
            
            // Handle player movement with WASD keys
            switch (e.KeyEvent.Key)
            {
                case Key.w:
                    Console.WriteLine($"W key pressed. Current position: {_gameState.Player.Position.X},{_gameState.Player.Position.Y}");
                    _gameState.Player.Move(_gameState.CurrentMap, 0, -1);
                    Console.WriteLine($"After Move call: {_gameState.Player.Position.X},{_gameState.Player.Position.Y}");
                    playerMoved = true;
                    break;
                case Key.a:
                    Console.WriteLine($"A key pressed. Current position: {_gameState.Player.Position.X},{_gameState.Player.Position.Y}");
                    _gameState.Player.Move(_gameState.CurrentMap, -1, 0);
                    Console.WriteLine($"After Move call: {_gameState.Player.Position.X},{_gameState.Player.Position.Y}");
                    playerMoved = true;
                    break;
                case Key.s:
                    Console.WriteLine($"S key pressed. Current position: {_gameState.Player.Position.X},{_gameState.Player.Position.Y}");
                    _gameState.Player.Move(_gameState.CurrentMap, 0, 1);
                    Console.WriteLine($"After Move call: {_gameState.Player.Position.X},{_gameState.Player.Position.Y}");
                    playerMoved = true;
                    break;
                case Key.d:
                    Console.WriteLine($"D key pressed. Current position: {_gameState.Player.Position.X},{_gameState.Player.Position.Y}");
                    _gameState.Player.Move(_gameState.CurrentMap, 1, 0);
                    Console.WriteLine($"After Move call: {_gameState.Player.Position.X},{_gameState.Player.Position.Y}");
                    playerMoved = true;
                    break;
                // Number key handlers for using skills
                case Key.D1:
                case Key.D2:
                case Key.D3:
                    int skillIndex = e.KeyEvent.Key == Key.D1 ? 0 : e.KeyEvent.Key == Key.D2 ? 1 : 2;
                    UsePlayerSkill(skillIndex);
                    break;
            }
            
            // Update enemy turns after player moves
            if (playerMoved)
            {
                // Update all enemies
                _gameState.UpdateEnemies();
                
                // Update cooldowns on skills
                foreach (var skill in _gameState.Player.Skills)
                {
                    skill.UpdateCooldown();
                }
                
                // Update the UI
                _statusBar.SetNeedsDisplay();
                _mapView.SetNeedsDisplay();
                _skillBar.SetNeedsDisplay();
                Application.Refresh(); // Force immediate refresh
            }
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
                    // Apply skill damage to the enemy
                    _gameState.Player.UseSkill(skillIndex, target);
                    
                    Console.WriteLine($"Used {skill.Name} on {target.Name} for {skill.Damage} damage!");
                    
                    // Check if enemy was killed
                    if (!target.IsAlive)
                    {
                        target.Die(_gameState);
                        Console.WriteLine($"{target.Name} was defeated! Gained {target.ExperienceValue} experience.");
                        
                        // Clear the selected enemy since it's now dead
                        _mapView.SelectEnemy(null);
                    }
                    
                    // After using a skill, enemies get a turn
                    _gameState.UpdateEnemies();
                    
                    // Update cooldowns on all other skills
                    foreach (var s in _gameState.Player.Skills)
                    {
                        if (s != skill) // We don't need to update the just-used skill
                            s.UpdateCooldown();
                    }
                    
                    // Update the UI
                    _statusBar.SetNeedsDisplay();
                    _mapView.SetNeedsDisplay();
                    _skillBar.SetNeedsDisplay();
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
    }
}