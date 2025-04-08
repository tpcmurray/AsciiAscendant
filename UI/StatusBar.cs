using System;
using Terminal.Gui;
using AsciiAscendant.Core;

namespace AsciiAscendant.UI
{
    public class StatusBar : View
    {
        private readonly GameState _gameState;
        
        public StatusBar(GameState gameState)
        {
            _gameState = gameState;
        }
        
        public override void Redraw(Rect bounds)
        {
            base.Redraw(bounds);
            
            var player = _gameState.Player;
            
            // Set status bar color
            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.White, Color.Blue));
            
            // Clear the status bar
            for (int i = 0; i < bounds.Width; i++)
            {
                AddRune(i, 0, (Rune)' ');
            }
            
            // Display player health, level, experience, and position coordinates
            string healthInfo = $"HP: {player.Health}/{player.MaxHealth}";
            string levelInfo = $"Level: {player.Level}";
            string expInfo = $"EXP: {player.Experience}/{player.Level * 100}";
            string posInfo = $"Pos: {player.Position.X},{player.Position.Y}";
            
            // Add status text to the status bar
            DrawText(0, 0, healthInfo);
            DrawText(healthInfo.Length + 3, 0, levelInfo);
            DrawText(healthInfo.Length + levelInfo.Length + 6, 0, expInfo);
            DrawText(healthInfo.Length + levelInfo.Length + expInfo.Length + 9, 0, posInfo);
        }
        
        private void DrawText(int x, int y, string text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                if (x + i < Frame.Width)
                {
                    AddRune(x + i, y, (Rune)text[i]);
                }
            }
        }
    }
}