using System;

namespace AsciiAscendant.Core
{
    public enum TileType
    {
        Floor,
        Wall,
        Door,
        Water,
        Obstacle // Added new tile type
    }
    
    // JSON-serializable tile data for map serialization
    public class TileData
    {
        public TileType TileType { get; set; } = TileType.Floor;
        public string Foreground { get; set; } = " ";
        public string ForegroundColor { get; set; } = "White";
        public string BackgroundColor { get; set; } = "Black";
        public int MovementCost { get; set; } = 1;
        
        public TileData() { }
    }
    
    public class Tile
    {
        // Changed property name from Type to TileType for consistency
        public TileType TileType { get; set; }
        public bool IsPassable => TileType != TileType.Wall && TileType != TileType.Water;
        public char Symbol => GetSymbolForType(TileType);
        
        // New properties for rich rendering
        public string Foreground { get; set; }
        public string ForegroundColor { get; set; }
        public string BackgroundColor { get; set; }
        public int MovementCost { get; set; }
        
        public Tile(TileType type)
        {
            TileType = type;
            
            // Set defaults for new properties
            Foreground = GetSymbolForType(type).ToString();
            ForegroundColor = "White";
            BackgroundColor = "Black";
            MovementCost = 1;
        }
        
        // Add constructor to create from TileData
        public Tile(TileData data)
        {
            TileType = data.TileType;
            Foreground = data.Foreground;
            ForegroundColor = data.ForegroundColor;
            BackgroundColor = data.BackgroundColor;
            MovementCost = data.MovementCost;
        }
        
        private char GetSymbolForType(TileType type)
        {
            return type switch
            {
                TileType.Floor => ' ', // Changed from '.' to ' ' (space)
                TileType.Wall => '#',
                TileType.Door => '+',
                TileType.Water => '~',
                TileType.Obstacle => 'o', // Symbol for new tile type
                _ => ' '
            };
        }
    }
}