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
            
            // Calculate total length of status info
            int totalLength = healthInfo.Length + levelInfo.Length + expInfo.Length + posInfo.Length + 9; // Spacing between items
            
            // Calculate starting position to center the status text
            int startX = (bounds.Width - totalLength) / 2;
            if (startX < 0) startX = 0;
            
            // Add status text to the status bar, centered
            DrawText(startX, 0, healthInfo);
            DrawText(startX + healthInfo.Length + 3, 0, levelInfo);
            DrawText(startX + healthInfo.Length + levelInfo.Length + 6, 0, expInfo);
            DrawText(startX + healthInfo.Length + levelInfo.Length + expInfo.Length + 9, 0, posInfo);
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