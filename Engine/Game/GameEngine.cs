using System;
using Terminal.Gui;
using AsciiAscendant.Core;
using AsciiAscendant.UI;

namespace AsciiAscendant.Engine
{
    public class GameEngine
    {
        private GameState _gameState = null!;
        private GameScreen _gameScreen = null!;

        public void Initialize()
        {
            // Initialize game state
            _gameState = new GameState();
            
            // Initialize UI
            _gameScreen = new GameScreen(_gameState);
            
            // Set the top-level UI element
            Application.Top.Add(_gameScreen);
        }

        public void Run()
        {
            // Start the Terminal.GUI main loop
            Application.Run();
        }
    }
}