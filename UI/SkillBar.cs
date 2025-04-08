using System;
using Terminal.Gui;
using AsciiAscendant.Core;

namespace AsciiAscendant.UI
{
    public class SkillBar : View
    {
        private readonly GameState _gameState;
        private const int BarHeight = 5; // Total height of skill bar
        
        public SkillBar(GameState gameState)
        {
            _gameState = gameState;
            CanFocus = true;
            Height = BarHeight;
        }
        
        public override void Redraw(Rect bounds)
        {
            base.Redraw(bounds);
            
            // Get player skills
            var skills = _gameState.Player.Skills;
            
            // Calculate the width of each skill block (including borders)
            int skillBlockWidth = 12; // Fixed width for each skill block
            int totalWidth = skills.Count * skillBlockWidth;
            
            // Calculate the starting X position to center the skill bar
            int startX = (bounds.Width - totalWidth) / 2;
            if (startX < 0) startX = 0;
            
            // Draw the top border with numbers
            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.White, Color.Black));
            for (int i = 0; i < skills.Count; i++)
            {
                int x = startX + (i * skillBlockWidth);
                
                // Draw top border
                for (int j = 0; j < skillBlockWidth; j++)
                {
                    if (j == 0)
                        AddRune(x + j, 0, (Rune)'┌');
                    else if (j == skillBlockWidth - 1)
                        AddRune(x + j, 0, (Rune)'┐');
                    else
                        AddRune(x + j, 0, (Rune)'─');
                }
                
                // Add number in the middle of the top border
                int keyNum = i + 1;
                if (keyNum <= 9)
                {
                    AddRune(x + (skillBlockWidth / 2), 0, (Rune)(keyNum.ToString()[0]));
                }
            }
            
            // Draw the side borders and skill content
            for (int row = 1; row < BarHeight - 1; row++)
            {
                for (int i = 0; i < skills.Count; i++)
                {
                    int x = startX + (i * skillBlockWidth);
                    
                    // Draw left and right borders
                    AddRune(x, row, (Rune)'│');
                    AddRune(x + skillBlockWidth - 1, row, (Rune)'│');
                    
                    // Draw skill content in the middle rows
                    var skill = skills[i];
                    
                    // Row 1 (index 1): Cooldown info
                    if (row == 1)
                    {
                        if (skill.CurrentCooldown > 0)
                        {
                            string cdText = $"CD: {skill.CurrentCooldown}";
                            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.Red, Color.Black));
                            DrawText(x + 1, row, cdText, skillBlockWidth - 2);
                        }
                        else
                        {
                            string readyText = "Ready";
                            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.Green, Color.Black));
                            DrawText(x + 1, row, readyText, skillBlockWidth - 2);
                        }
                    }
                    // Row 2 (index 2): Skill name
                    else if (row == 2)
                    {
                        Driver.SetAttribute(new Terminal.Gui.Attribute(Color.White, Color.Black));
                        DrawText(x + 1, row, skill.Name, skillBlockWidth - 2);
                    }
                    // Row 3 (index 3): Skill damage
                    else if (row == 3)
                    {
                        Driver.SetAttribute(new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black));
                        DrawText(x + 1, row, $"DMG: {skill.Damage}", skillBlockWidth - 2);
                    }
                }
            }
            
            // Draw the bottom border
            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.White, Color.Black));
            for (int i = 0; i < skills.Count; i++)
            {
                int x = startX + (i * skillBlockWidth);
                
                // Draw bottom border
                for (int j = 0; j < skillBlockWidth; j++)
                {
                    if (j == 0)
                        AddRune(x + j, BarHeight - 1, (Rune)'└');
                    else if (j == skillBlockWidth - 1)
                        AddRune(x + j, BarHeight - 1, (Rune)'┘');
                    else
                        AddRune(x + j, BarHeight - 1, (Rune)'─');
                }
            }
        }
        
        private void DrawText(int x, int y, string text, int maxWidth)
        {
            string displayText = text;
            if (text.Length > maxWidth)
            {
                displayText = text.Substring(0, maxWidth);
            }
            
            for (int i = 0; i < displayText.Length; i++)
            {
                AddRune(x + i, y, (Rune)displayText[i]);
            }
            
            // Clear any remaining space with spaces
            for (int i = displayText.Length; i < maxWidth; i++)
            {
                AddRune(x + i, y, (Rune)' ');
            }
        }
        
        // Instead of overriding GetHeight(), just access the Height property
        public int GetBarHeight()
        {
            return BarHeight;
        }
    }
}