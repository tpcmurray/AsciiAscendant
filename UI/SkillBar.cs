using System;
using Terminal.Gui;
using AsciiAscendant.Core;

namespace AsciiAscendant.UI
{
    public class SkillBar : View
    {
        private readonly GameState _gameState;
        private readonly MapView _mapView;
        private const int BarHeight = 6; // Total height of skill bar (increased to accommodate cooldown bar)
        
        public SkillBar(GameState gameState, MapView mapView)
        {
            _gameState = gameState;
            _mapView = mapView;
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
                    
                    // Check if the skill is available (cooldown + target + range)
                    bool isSkillAvailable = skill.CanUse();
                    
                    // Check if there's a selected enemy and if the skill is in range
                    bool isTargetSelected = _mapView.GetSelectedEnemy() != null;
                    bool isInRange = isTargetSelected && _mapView.IsSkillInRange(skill);
                    
                    // Row 1 (index 1): Cooldown info
                    if (row == 1)
                    {
                        if (skill.CurrentCooldownInSeconds > 0)
                        {
                            // Format the cooldown with one decimal place
                            string cdText = $"CD: {skill.CurrentCooldownInSeconds:F1}s";
                            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.Red, Color.Black));
                            DrawText(x + 1, row, cdText, skillBlockWidth - 2);
                        }
                        else if (!isTargetSelected)
                        {
                            string noTargetText = "No Target";
                            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.Gray, Color.Black));
                            DrawText(x + 1, row, noTargetText, skillBlockWidth - 2);
                        }
                        else if (!isInRange)
                        {
                            string outRangeText = "Out Range";
                            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.BrightYellow, Color.Black));
                            DrawText(x + 1, row, outRangeText, skillBlockWidth - 2);
                        }
                        else
                        {
                            string readyText = "Ready";
                            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.Green, Color.Black));
                            DrawText(x + 1, row, readyText, skillBlockWidth - 2);
                        }
                        
                        // Draw cooldown progress bar directly below skill status text
                        if (skill.CooldownInSeconds > 0)
                        {
                            int barRow = row + 1;
                            int barWidth = skillBlockWidth - 2;
                            
                            // Calculate fill percentage (inverted: 0% means full cooldown, 100% means ready)
                            float percentage = 1f;
                            if (skill.CurrentCooldownInSeconds > 0)
                            {
                                percentage = 1f - (skill.CurrentCooldownInSeconds / skill.CooldownInSeconds);
                            }
                            
                            int filledWidth = (int)Math.Ceiling(barWidth * percentage);
                            
                            // Draw progress bar background first
                            Driver.SetAttribute(new Terminal.Gui.Attribute(Color.Gray, Color.Black));
                            for (int j = 0; j < barWidth; j++)
                            {
                                AddRune(x + 1 + j, barRow, (Rune)'░');
                            }
                            
                            // Draw filled portion
                            Color fillColor = percentage < 1f ? Color.BrightYellow : Color.Green;
                            Driver.SetAttribute(new Terminal.Gui.Attribute(fillColor, Color.Black));
                            for (int j = 0; j < filledWidth && j < barWidth; j++)
                            {
                                AddRune(x + 1 + j, barRow, (Rune)'█');
                            }
                        }
                    }
                    // Row 2 (index 2): Skill name - shift one down to accommodate cooldown bar
                    else if (row == 3)
                    {
                        // Gray out the name if skill isn't available
                        var color = (isTargetSelected && isInRange && isSkillAvailable) ? 
                                    Color.White : Color.Gray;
                        Driver.SetAttribute(new Terminal.Gui.Attribute(color, Color.Black));
                        DrawText(x + 1, row, skill.Name, skillBlockWidth - 2);
                    }
                    // Row 3 (index 3): Skill damage and type/range - shift one down
                    else if (row == 4)
                    {
                        // Gray out the damage if skill isn't available
                        var color = (isTargetSelected && isInRange && isSkillAvailable) ? 
                                    Color.BrightYellow : Color.Gray;
                        Driver.SetAttribute(new Terminal.Gui.Attribute(color, Color.Black));
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