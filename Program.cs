using System;
using Terminal.Gui;
using AsciiAscendant.Engine;

namespace AsciiAscendant
{
    class Program
    {
        static void Main(string[] args)
        {
            // Initialize Terminal.GUI
            Application.Init();
            
            try
            {
                // Create and run the game engine
                var game = new GameEngine();
                game.Initialize();
                game.Run();
            }
            finally
            {
                // Clean up Terminal.GUI resources
                Application.Shutdown();
            }
        }
    }
}
