using System;
using Terminal.Gui;
using AsciiAscendant.Core;
using AsciiAscendant.Core.Entities;
using AsciiAscendant.Core.Animations;
using AsciiAscendant.UI.Screens;

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
        
        // Reference to the inventory screen
        private InventoryScreen _inventoryScreen;
        
        // Flag to prevent multiple inventory screens
        private bool _isInventoryOpen = false;
        
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
            
            // Create the map view first (needed by SkillBar for range calculations)
            _mapView = new MapView(_gameState)
            {
                X = 0,
                Y = 1, // Start after close button
                Width = Dim.Fill(),
                Height = Dim.Fill() - 1 // Leave room for close button
            };
            
            // Create the skill bar with reference to MapView (positioned at the bottom)
            _skillBar = new SkillBar(_gameState, _mapView);
            
            // Create the status bar
            _statusBar = new StatusBar(_gameState);
            
            // Add the UI components
            Add(_mapView);
            Add(_skillBar);
            Add(_statusBar);
            
            // Create inventory screen (doesn't add it to UI yet)
            _inventoryScreen = new InventoryScreen(_gameState);
            
            // Update layout positions
            LayoutSubviews();
            
            // Set up key handling for player movement and quit
            KeyPress += GameScreen_KeyPress;
            KeyUp += GameScreen_KeyUp;
            
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
            
            // Handle player movement with WASD keys
            switch (e.KeyEvent.Key)
            {
                case Key.w:
                    _gameState.Player.Move(_gameState.CurrentMap, 0, -1);
                    break;
                case Key.a:
                    _gameState.Player.Move(_gameState.CurrentMap, -1, 0);
                    break;
                case Key.s:
                    _gameState.Player.Move(_gameState.CurrentMap, 0, 1);
                    break;
                case Key.d:
                    _gameState.Player.Move(_gameState.CurrentMap, 1, 0);
                    break;
                case Key.D1:
                case Key.D2:
                case Key.D3:
                    int skillIndex = e.KeyEvent.Key == Key.D1 ? 0 : e.KeyEvent.Key == Key.D2 ? 1 : 2;
                    UsePlayerSkill(skillIndex);
                    break;
            }
            
            _statusBar.SetNeedsDisplay();
            _mapView.SetNeedsDisplay();
            _skillBar.SetNeedsDisplay();
            Application.Refresh(); // Force immediate refresh
            
            _processingKeyPress = false;
        }


        private void GameScreen_KeyUp(KeyEventEventArgs e)
        {
            // Check for Ctrl+Q to quit the game
            if (e.KeyEvent.Key == Key.Q && e.KeyEvent.IsCtrl)
            {
                Application.RequestStop();
                return;
            }
            
            // Handle inventory toggle with 'I' key
            if (e.KeyEvent.Key == Key.i || e.KeyEvent.Key == Key.I)
            {
                OpenInventory();
                return;
            }
            
            // Space key item pickup has been removed since items are now picked up automatically
        }

        private void UsePlayerSkill(int skillIndex)
        {
            // Get the currently selected enemy
            var target = _mapView.GetSelectedEnemy();
            
            // Check if player has selected an enemy and if the skill index is valid
            if (target != null && skillIndex >= 0 && skillIndex < _gameState.Player.Skills.Count)
            {
                var skill = _gameState.Player.Skills[skillIndex];
                
                // Check if the target is in range for this skill
                bool inRange = _mapView.IsSkillInRange(skill);
                
                if (!inRange) 
                {
                    // Target is out of range
                    return;
                }
                
                // Check if skill can be used (not on cooldown)
                if (skill.CanUse())
                {
                    // Set the skill on cooldown
                    skill.Use();
                    
                    // Check skill name to determine what type of animation to create
                    if (skill.Name == "Fireball")
                    {
                        _gameState.CreateFireballAnimation(_gameState.Player.Position, target, skill.Damage);
                    }
                    else if (skill.Name == "Arrow Shot") 
                    {
                        _gameState.CreateArrowAnimation(_gameState.Player.Position, target, skill.Damage);
                    }
                    else
                    {
                        // For melee skills like Slash, apply damage directly with a flash effect
                        target.TakeDamage(skill.Damage);
                        target.Flash(); // Ensure the enemy flashes red from the hit
                        
                        // Check if enemy was killed
                        if (!target.IsAlive)
                        {
                            HandleEnemyDeath(target);
                        }
                    }
                    
                    // Update UI
                    _statusBar.SetNeedsDisplay();
                    _mapView.SetNeedsDisplay();
                    _skillBar.SetNeedsDisplay();
                    Application.Refresh();
                }
            }
        }
        
        // Helper method to handle enemy death
        private void HandleEnemyDeath(Enemy target)
        {
            // Handle loot and XP
            target.Die(_gameState);
            
            // Clear the selected enemy since it's now dead
            _mapView.SelectEnemy(null);
            
            // Update UI
            _statusBar.SetNeedsDisplay();
            _mapView.SetNeedsDisplay();
            _skillBar.SetNeedsDisplay();
        }

        private void OpenInventory()
        {
            // Set flag to indicate inventory is open
            _isInventoryOpen = true;
            
            // Create a new dialog each time to ensure clean state
            var dialog = new Dialog()
            {
                Title = "Inventory",
                Width = Dim.Percent(80),
                Height = Dim.Percent(80),
                Border = new Border() 
                {
                    BorderStyle = BorderStyle.Single
                }
            };
            
            // Create a new inventory screen instance each time
            _inventoryScreen = new InventoryScreen(_gameState);
            _inventoryScreen.LoadInventory();
            
            // Add the inventory screen to the dialog
            dialog.Add(_inventoryScreen);
            
            // ONE handler for the Escape key that will close immediately
            dialog.KeyPress += (e) => {
                if (e.KeyEvent.Key == Key.Esc)
                {
                    // When the dialog closes with Escape, reset our flag
                    _isInventoryOpen = false;
                    Application.RequestStop();
                    e.Handled = true;
                }
            };
            
            // Show the dialog as a modal
            Application.Run(dialog);
            
            // After closing inventory, reset flag (in case it wasn't reset by the event handlers)
            _isInventoryOpen = false;
            
            // Refresh the game screen
            _mapView.SetNeedsDisplay();
            _statusBar.SetNeedsDisplay();
            _skillBar.SetNeedsDisplay();
            Application.Refresh();
        }
        
        private void PickupItems()
        {
            // Try to pick up items at the player's position
            var pickedUpItems = _gameState.AttemptItemPickup();
            
            if (pickedUpItems.Count > 0)
            {
                // Items were picked up - refresh the display
                _mapView.SetNeedsDisplay();
                Application.Refresh();
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