using System;

namespace AsciiAscendant.Core
{
    public enum TileType
    {
        Floor,
        Wall,
        Door,
        Water
    }
    
    public class Tile
    {
        public TileType Type { get; set; }
        public bool IsPassable => Type != TileType.Wall && Type != TileType.Water;
        public char Symbol => GetSymbolForType(Type);
        
        public Tile(TileType type)
        {
            Type = type;
        }
        
        private char GetSymbolForType(TileType type)
        {
            return type switch
            {
                TileType.Floor => ' ', // Changed from '.' to ' ' (space)
                TileType.Wall => '#',
                TileType.Door => '+',
                TileType.Water => '~',
                _ => ' '
            };
        }
    }
}