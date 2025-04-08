using System;
using System.Collections.Generic;

namespace AsciiAscendant.Core
{
    public class Map
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Tile[,] Tiles { get; private set; }

        public Map(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new Tile[width, height];
            
            // Initialize all tiles with default empty floor tiles
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tiles[x, y] = new Tile(TileType.Floor);
                }
            }
            
            // Add some walls around the edges for testing
            for (int x = 0; x < width; x++)
            {
                Tiles[x, 0] = new Tile(TileType.Wall);
                Tiles[x, height - 1] = new Tile(TileType.Wall);
            }
            
            for (int y = 0; y < height; y++)
            {
                Tiles[0, y] = new Tile(TileType.Wall);
                Tiles[width - 1, y] = new Tile(TileType.Wall);
            }
        }
        
        public bool IsPassable(int x, int y)
        {
            // Check bounds
            if (x < 0 || x >= Width || y < 0 || y >= Height)
            {
                return false;
            }
            
            return Tiles[x, y].IsPassable;
        }
    }
}