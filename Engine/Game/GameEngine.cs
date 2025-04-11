using System;
using System.Threading;
using Terminal.Gui;
using AsciiAscendant.Core;
using AsciiAscendant.UI;

namespace AsciiAscendant.Engine
{
    public class GameEngine
    {
        private GameState _gameState = null!;
        private GameScreen _gameScreen = null!;
        private Timer _gameLoopTimer = null!;
        private const int GameTickIntervalMs = 100; // Update game every 100ms
        private bool _gameRunning = false;
        private bool _gamePaused = true; // Start the game in paused state
        
        // Track enemy update timing - enemies should update once per second
        private int _enemyUpdateCounter = 0;
        private const int EnemyUpdateFrequency = 10; // Update enemies every 10 ticks (10 * 100ms = 1 second)
        
        // Track regeneration timing - should regenerate once per second
        private int _regenerationCounter = 0;
        private const int RegenerationFrequency = 10; // Regenerate every 10 ticks (10 * 100ms = 1 second)

        public void Initialize()
        {         
            Terminal.Gui.Application.Init();
            
            // Initialize game state
            _gameState = new GameState();
            
            // Initialize UI
            _gameScreen = new GameScreen(_gameState);
            
            // Set the top-level UI element
            Application.Top.Add(_gameScreen);
            
            // Initialize the game loop timer
            _gameLoopTimer = new Timer(GameLoopCallback, null, Timeout.Infinite, Timeout.Infinite);
            
            // Show controls help dialog on startup
            Application.MainLoop.AddTimeout(TimeSpan.FromMilliseconds(200), ShowHelpDialog);
        }

        public void Run()
        {
            // Start the game loop
            StartGameLoop();
            
            // Start the Terminal.GUI main loop
            Application.Run();
            
            // Stop the game loop when application exits
            StopGameLoop();
        }
        
        private void StartGameLoop()
        {
            _gameRunning = true;
            _gameLoopTimer.Change(0, GameTickIntervalMs);
        }
        
        private void StopGameLoop()
        {
            _gameRunning = false;
            _gameLoopTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }
        
        private void GameLoopCallback(object? state)
        {
            if (!_gameRunning || _gamePaused)
                return;
                
            try
            {
                // Update game state on the UI thread to avoid concurrency issues
                Application.MainLoop.Invoke(() => {
                    // Update the game state
                    UpdateGameState();
                    
                    // Refresh the UI
                    _gameScreen.RefreshDisplay();
                });
            }
            catch (Exception)
            {
                // Exception handling without using the variable
                // We could log errors here in the future if needed
            }
        }
        
        private void UpdateGameState()
        {
            // Use the new UpdateGameTick for stamina regeneration and animations
            _gameState.UpdateGameTick();
            
            // Update visual effects in the map view
            _gameScreen.UpdateMapEffects();
            
            // Update enemies less frequently (once per second)
            _enemyUpdateCounter++;
            if (_enemyUpdateCounter >= EnemyUpdateFrequency)
            {
                // Time to update enemies
                _gameState.UpdateEnemies();
                
                // Reset counter
                _enemyUpdateCounter = 0;
            }
            
            // Update health and stamina regeneration less frequently (once per second)
            _regenerationCounter++;
            if (_regenerationCounter >= RegenerationFrequency)
            {
                // Time to regenerate health and stamina
                _gameState.Player.RegenerateHealth();
                _gameState.Player.RegenerateStamina();
                
                // Reset counter
                _regenerationCounter = 0;
            }
            
            // Update skill cooldowns every tick
            foreach (var skill in _gameState.Player.Skills)
            {
                skill.UpdateCooldown();
            }
        }
        
        // Method to toggle pause state
        public void TogglePause()
        {
            _gamePaused = !_gamePaused;
        }
        
        // Method to show the controls help dialog
        private bool ShowHelpDialog(MainLoop caller)
        {
            var helpDialog = new Dialog("Game Controls", 60, 16);
            
            var helpText = new Label(1, 1, @"
Movement:
  W, A, S, D - Move the player character

Combat:
  Mouse Click - Select an enemy target
  1, 2, 3 - Use skills (when enemy is selected)

Interface:
  I - Open inventory
  H - Show this help screen

Press any key to start the game...");
            
            helpDialog.Add(helpText);
            
            // When the dialog closes, unpause the game
            helpDialog.KeyPress += (e) => {
                _gamePaused = false;
                Application.RequestStop();
                e.Handled = true;
            };
            
            Application.Run(helpDialog);
            
            return false; // Don't repeat this timeout
        }

        // Add these P/Invoke methods at the class level
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        private static void SetConsoleFont(IntPtr handle, string fontName, short fontSize)
        {
            // This is a simplified approach - in a real implementation, you'd use 
            // P/Invoke to call SetCurrentConsoleFontEx with the desired font settings
            
            // For demonstration purposes only - needs actual P/Invoke implementation
            Console.WriteLine($"Setting console font to {fontName}, {fontSize}pt");
        }
    }
}