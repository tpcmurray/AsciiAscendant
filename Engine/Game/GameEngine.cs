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
        
        // Track enemy update timing - enemies should update once per second
        private int _enemyUpdateCounter = 0;
        private const int EnemyUpdateFrequency = 10; // Update enemies every 10 ticks (10 * 100ms = 1 second)

        public void Initialize()
        {
            // Initialize game state
            _gameState = new GameState();
            
            // Initialize UI
            _gameScreen = new GameScreen(_gameState);
            
            // Set the top-level UI element
            Application.Top.Add(_gameScreen);
            
            // Initialize the game loop timer
            _gameLoopTimer = new Timer(GameLoopCallback, null, Timeout.Infinite, Timeout.Infinite);
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
            if (!_gameRunning)
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error in game loop: {ex.Message}");
            }
        }
        
        private void UpdateGameState()
        {
            // Update animations and other frequent updates
            _gameState.UpdateAnimations();
            
            // Update enemies less frequently (once per second)
            _enemyUpdateCounter++;
            if (_enemyUpdateCounter >= EnemyUpdateFrequency)
            {
                // Time to update enemies
                _gameState.UpdateEnemies();
                
                // Reset counter
                _enemyUpdateCounter = 0;
            }
            
            // Update skill cooldowns every tick
            foreach (var skill in _gameState.Player.Skills)
            {
                skill.UpdateCooldown();
            }
        }
    }
}