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
            
            // Create and add the map view
            _mapView = new MapView(_gameState)
            {
                X = 0,
                Y = 1, // Leave room for status bar
                Width = Dim.Fill(),
                Height = Dim.Fill() - 2 // Leave room for status bar and skill bar
            };
            Add(_mapView);
            
            // Create and add the status bar - now starting after the close button
            _statusBar = new StatusBar(_gameState)
            {
                X = 6, // Position after the close button
                Y = 0,
                Width = Dim.Fill() - 4, // Adjusted to leave room for close button
                Height = 1
            };
            Add(_statusBar);
            
            // Set up key handling for player movement and quit
            KeyPress += GameScreen_KeyPress;
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
                // Add number key handlers for using skills
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
                
                // Update the UI
                _statusBar.SetNeedsDisplay();
                _mapView.SetNeedsDisplay();
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
                    
                    // Update the UI
                    _statusBar.SetNeedsDisplay();
                    _mapView.SetNeedsDisplay();
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