using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AsciiAscendant.Core
{
    // JSON-serializable map data for saving/loading
    public class MapData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public TileData[,] Tiles { get; set; } = new TileData[0, 0]; // Initialize with empty array
        public List<TerrainFeature> TerrainFeatures { get; set; } = new List<TerrainFeature>();

        [JsonIgnore]
        public bool IsLoaded { get; private set; } = false;

        public MapData() { }

        public MapData(int width, int height)
        {
            Width = width;
            Height = height;
            Tiles = new TileData[width, height];
            
            // Initialize tiles to default values
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Tiles[x, y] = new TileData 
                    { 
                        TileType = TileType.Floor,
                        Foreground = "\u2591", // Light shade character (░)
                        ForegroundColor = "DarkGreen",
                        BackgroundColor = "Black"
                    };
                }
            }
            
            IsLoaded = true;
        }
        
        public static MapData GenerateRandomMap(int width, int height, int seed = 0)
        {
            Random random = seed != 0 ? new Random(seed) : new Random();
            MapData mapData = new MapData(width, height);
            
            // Generate base terrain (grass, dirt, etc)
            GenerateBaseTerrain(mapData, random);
            
            // Generate water bodies
            GenerateWaterBodies(mapData, random);
            
            // Place terrain features like ruins, trees, rocks
            PlaceTerrainFeatures(mapData, random);
            
            return mapData;
        }
        
        private static void GenerateBaseTerrain(MapData mapData, Random random)
        {
            // Using a simple noise algorithm to create variation in terrain
            SimplexNoise noise = new SimplexNoise(random.Next());
            
            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    // Generate a noise value between -1 and 1
                    float noiseValue = (float)noise.Evaluate(x / 50.0, y / 50.0);
                    
                    // Transform noise to 0-1 range
                    float normalizedNoise = (noiseValue + 1) / 2.0f;
                    
                    if (normalizedNoise < 0.3f)
                    {
                        // Dirt/path
                        mapData.Tiles[x, y].TileType = TileType.Floor;
                        mapData.Tiles[x, y].Foreground = "\u2591"; // Light shade (░)
                        mapData.Tiles[x, y].ForegroundColor = "SaddleBrown";
                    }
                    else if (normalizedNoise < 0.8f)
                    {
                        // Regular grass
                        mapData.Tiles[x, y].TileType = TileType.Floor;
                        mapData.Tiles[x, y].Foreground = "\u2591"; // Light shade (░)
                        mapData.Tiles[x, y].ForegroundColor = "Green";
                    }
                    else
                    {
                        // Dense grass/vegetation
                        mapData.Tiles[x, y].TileType = TileType.Floor;
                        mapData.Tiles[x, y].Foreground = "\u2592"; // Medium shade (▒)
                        mapData.Tiles[x, y].ForegroundColor = "DarkGreen";
                    }
                }
            }
        }
        
        private static void GenerateWaterBodies(MapData mapData, Random random)
        {
            // Generate lakes and rivers with Perlin noise
            SimplexNoise waterNoise = new SimplexNoise(random.Next());
            
            for (int x = 0; x < mapData.Width; x++)
            {
                for (int y = 0; y < mapData.Height; y++)
                {
                    // Use a different scale for water
                    float waterValue = (float)waterNoise.Evaluate(x / 100.0, y / 100.0);
                    float normalizedWaterValue = (waterValue + 1) / 2.0f;
                    
                    if (normalizedWaterValue > 0.7f)
                    {
                        // Water
                        mapData.Tiles[x, y].TileType = TileType.Water;
                        
                        // Different water depths based on noise value
                        if (normalizedWaterValue > 0.85f)
                        {
                            mapData.Tiles[x, y].Foreground = "\u2593"; // Dark shade (▓)
                            mapData.Tiles[x, y].ForegroundColor = "DarkBlue";
                            mapData.Tiles[x, y].BackgroundColor = "Black";
                        }
                        else
                        {
                            mapData.Tiles[x, y].Foreground = "\u2592"; // Medium shade (▒)
                            mapData.Tiles[x, y].ForegroundColor = "Blue";
                            mapData.Tiles[x, y].BackgroundColor = "DarkBlue";
                        }
                    }
                }
            }
            
            // Generate rivers
            int numRivers = 3 + random.Next(5);
            for (int i = 0; i < numRivers; i++)
            {
                GenerateRiver(mapData, random);
            }
        }
        
        private static void GenerateRiver(MapData mapData, Random random)
        {
            // Pick a random edge point to start the river
            int edge = random.Next(4); // 0:top, 1:right, 2:bottom, 3:left
            
            int startX, startY;
            switch (edge)
            {
                case 0: // Top edge
                    startX = random.Next(mapData.Width);
                    startY = 0;
                    break;
                case 1: // Right edge
                    startX = mapData.Width - 1;
                    startY = random.Next(mapData.Height);
                    break;
                case 2: // Bottom edge
                    startX = random.Next(mapData.Width);
                    startY = mapData.Height - 1;
                    break;
                default: // Left edge
                    startX = 0;
                    startY = random.Next(mapData.Height);
                    break;
            }
            
            // Keep track of river path
            int currentX = startX;
            int currentY = startY;
            
            // Generate a winding river
            int length = mapData.Width / 2 + random.Next(mapData.Width / 2);
            int directionBias = random.Next(4); // Bias river flow direction
            
            for (int i = 0; i < length; i++)
            {
                // Mark this tile as water
                if (currentX >= 0 && currentX < mapData.Width && currentY >= 0 && currentY < mapData.Height)
                {
                    mapData.Tiles[currentX, currentY].TileType = TileType.Water;
                    mapData.Tiles[currentX, currentY].Foreground = "\u2593"; // Dark shade (▓)
                    mapData.Tiles[currentX, currentY].ForegroundColor = "Blue";
                    mapData.Tiles[currentX, currentY].BackgroundColor = "DarkBlue";
                    
                    // Make river wider with random width
                    int riverWidth = 1 + random.Next(3);
                    for (int w = 1; w <= riverWidth; w++)
                    {
                        for (int dx = -w; dx <= w; dx++)
                        {
                            for (int dy = -w; dy <= w; dy++)
                            {
                                int nx = currentX + dx;
                                int ny = currentY + dy;
                                
                                if (nx >= 0 && nx < mapData.Width && ny >= 0 && ny < mapData.Height &&
                                    Math.Abs(dx) + Math.Abs(dy) <= w + 1)
                                {
                                    // Randomize the river banks slightly
                                    if (random.NextDouble() < 0.7)
                                    {
                                        mapData.Tiles[nx, ny].TileType = TileType.Water;
                                        mapData.Tiles[nx, ny].Foreground = "\u2592"; // Medium shade (▒)
                                        mapData.Tiles[nx, ny].ForegroundColor = "Blue";
                                        mapData.Tiles[nx, ny].BackgroundColor = "DarkBlue";
                                    }
                                }
                            }
                        }
                    }
                }
                
                // Determine next direction with bias
                int direction = random.Next(100) < 70 ? directionBias : random.Next(4);
                
                switch (direction)
                {
                    case 0: currentY--; break; // North
                    case 1: currentX++; break; // East
                    case 2: currentY++; break; // South
                    case 3: currentX--; break; // West
                }
                
                // Small chance to change direction bias
                if (random.Next(20) == 0)
                {
                    directionBias = random.Next(4);
                }
            }
        }
        
        private static void PlaceTerrainFeatures(MapData mapData, Random random)
        {
            // Place various terrain features like ruins, vegetation, rocks, etc.
            
            // Generate ruins
            int numRuins = 5 + random.Next(10);
            for (int i = 0; i < numRuins; i++)
            {
                PlaceRuin(mapData, random);
            }
            
            // Generate forest clusters
            int numForests = 10 + random.Next(15);
            for (int i = 0; i < numForests; i++)
            {
                PlaceForestCluster(mapData, random);
            }
            
            // Generate rock formations
            int numRockFormations = 10 + random.Next(20);
            for (int i = 0; i < numRockFormations; i++)
            {
                PlaceRockFormation(mapData, random);
            }
        }
        
        private static void PlaceRuin(MapData mapData, Random random)
        {
            // Create ancient fantasy ruin
            int ruinX = random.Next(50, mapData.Width - 50);
            int ruinY = random.Next(50, mapData.Height - 50);
            
            // Determine size of the ruin
            int ruinWidth = 5 + random.Next(10);
            int ruinHeight = 5 + random.Next(10);
            
            // Create a basic rectangular structure with some randomized walls
            for (int x = ruinX; x < ruinX + ruinWidth && x < mapData.Width; x++)
            {
                for (int y = ruinY; y < ruinY + ruinHeight && y < mapData.Height; y++)
                {
                    // Create walls around the perimeter
                    if (x == ruinX || x == ruinX + ruinWidth - 1 || y == ruinY || y == ruinY + ruinHeight - 1)
                    {
                        if (random.NextDouble() < 0.7) // Some walls are crumbled
                        {
                            mapData.Tiles[x, y].TileType = TileType.Wall;
                            mapData.Tiles[x, y].Foreground = "\u2588"; // Full block (█)
                            mapData.Tiles[x, y].ForegroundColor = "Gray";
                        }
                    }
                    else
                    {
                        // Stone floor inside
                        mapData.Tiles[x, y].TileType = TileType.Floor;
                        mapData.Tiles[x, y].Foreground = "\u2591"; // Light shade (░)
                        mapData.Tiles[x, y].ForegroundColor = "DarkGray";
                    }
                }
            }
            
            // Add a door
            int doorSide = random.Next(4);
            switch (doorSide)
            {
                case 0: // North
                    mapData.Tiles[ruinX + ruinWidth / 2, ruinY].TileType = TileType.Door;
                    mapData.Tiles[ruinX + ruinWidth / 2, ruinY].Foreground = "+";
                    mapData.Tiles[ruinX + ruinWidth / 2, ruinY].ForegroundColor = "Brown";
                    break;
                case 1: // East
                    mapData.Tiles[ruinX + ruinWidth - 1, ruinY + ruinHeight / 2].TileType = TileType.Door;
                    mapData.Tiles[ruinX + ruinWidth - 1, ruinY + ruinHeight / 2].Foreground = "+";
                    mapData.Tiles[ruinX + ruinWidth - 1, ruinY + ruinHeight / 2].ForegroundColor = "Brown";
                    break;
                case 2: // South
                    mapData.Tiles[ruinX + ruinWidth / 2, ruinY + ruinHeight - 1].TileType = TileType.Door;
                    mapData.Tiles[ruinX + ruinWidth / 2, ruinY + ruinHeight - 1].Foreground = "+";
                    mapData.Tiles[ruinX + ruinWidth / 2, ruinY + ruinHeight - 1].ForegroundColor = "Brown";
                    break;
                case 3: // West
                    mapData.Tiles[ruinX, ruinY + ruinHeight / 2].TileType = TileType.Door;
                    mapData.Tiles[ruinX, ruinY + ruinHeight / 2].Foreground = "+";
                    mapData.Tiles[ruinX, ruinY + ruinHeight / 2].ForegroundColor = "Brown";
                    break;
            }
            
            // Add interior structure
            for (int i = 0; i < ruinWidth / 3; i++)
            {
                int pillarX = ruinX + 2 + random.Next(ruinWidth - 4);
                int pillarY = ruinY + 2 + random.Next(ruinHeight - 4);
                
                if (pillarX < mapData.Width && pillarY < mapData.Height)
                {
                    mapData.Tiles[pillarX, pillarY].TileType = TileType.Wall;
                    mapData.Tiles[pillarX, pillarY].Foreground = "\u2588"; // Full block (█)
                    mapData.Tiles[pillarX, pillarY].ForegroundColor = "Gray";
                }
            }
            
            // Add to terrain features list
            mapData.TerrainFeatures.Add(new TerrainFeature
            {
                Type = "Ruin",
                X = ruinX,
                Y = ruinY,
                Width = ruinWidth,
                Height = ruinHeight
            });
        }
        
        private static void PlaceForestCluster(MapData mapData, Random random)
        {
            // Create a cluster of trees
            int forestX = random.Next(mapData.Width);
            int forestY = random.Next(mapData.Height);
            
            int forestSize = 10 + random.Next(30);
            int treeDensity = 40 + random.Next(40); // Percentage chance of tree placement
            
            for (int i = 0; i < forestSize; i++)
            {
                // Use a scatter pattern for natural-looking distribution
                int treeX = forestX + random.Next(-10, 11);
                int treeY = forestY + random.Next(-10, 11);
                
                if (treeX >= 0 && treeX < mapData.Width && treeY >= 0 && treeY < mapData.Height &&
                    mapData.Tiles[treeX, treeY].TileType == TileType.Floor)
                {
                    if (random.Next(100) < treeDensity)
                    {
                        bool isLargeTree = random.Next(5) == 0;
                        
                        if (isLargeTree && treeX + 1 < mapData.Width && treeY + 1 < mapData.Height)
                        {
                            // Large 2x2 tree
                            for (int dx = 0; dx <= 1; dx++)
                            {
                                for (int dy = 0; dy <= 1; dy++)
                                {
                                    mapData.Tiles[treeX + dx, treeY + dy].Foreground = "\u2593"; // Dark shade (▓)
                                    mapData.Tiles[treeX + dx, treeY + dy].ForegroundColor = "DarkGreen";
                                    
                                    // Trees are passable but harder to move through
                                    mapData.Tiles[treeX + dx, treeY + dy].MovementCost = 2;
                                }
                            }
                        }
                        else
                        {
                            // Single tree
                            mapData.Tiles[treeX, treeY].Foreground = "\u2593"; // Dark shade (▓)
                            mapData.Tiles[treeX, treeY].ForegroundColor = "ForestGreen";
                            
                            // Trees are passable but harder to move through
                            mapData.Tiles[treeX, treeY].MovementCost = 2;
                        }
                    }
                }
            }
            
            // Add to terrain features list
            mapData.TerrainFeatures.Add(new TerrainFeature
            {
                Type = "Forest",
                X = forestX,
                Y = forestY,
                Width = 20, // Approximate size
                Height = 20
            });
        }
        
        private static void PlaceRockFormation(MapData mapData, Random random)
        {
            // Place rock formations and boulders
            int rockX = random.Next(mapData.Width);
            int rockY = random.Next(mapData.Height);
            
            // Skip if this would overlap with water
            if (rockX < mapData.Width && rockY < mapData.Height && mapData.Tiles[rockX, rockY].TileType == TileType.Water)
            {
                return;
            }
            
            int formationSize = 1 + random.Next(5);
            
            for (int i = 0; i < formationSize; i++)
            {
                int boulderX = rockX + random.Next(-3, 4);
                int boulderY = rockY + random.Next(-3, 4);
                
                if (boulderX >= 0 && boulderX < mapData.Width && boulderY >= 0 && boulderY < mapData.Height &&
                    mapData.Tiles[boulderX, boulderY].TileType == TileType.Floor)
                {
                    mapData.Tiles[boulderX, boulderY].TileType = TileType.Wall;
                    mapData.Tiles[boulderX, boulderY].Foreground = "\u2588"; // Full block (█)
                    mapData.Tiles[boulderX, boulderY].ForegroundColor = "DimGray";
                }
            }
            
            // Add to terrain features list
            mapData.TerrainFeatures.Add(new TerrainFeature
            {
                Type = "RockFormation",
                X = rockX,
                Y = rockY,
                Width = 6,
                Height = 6
            });
        }
        
        public static void SaveToJson(MapData mapData, string filePath)
        {
            // Serialize the map to JSON, handling the 2D array
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            // Create a serializable version with 1D arrays
            var serializableMap = new SerializableMapData
            {
                Width = mapData.Width,
                Height = mapData.Height,
                TileData = FlattenTileArray(mapData.Tiles),
                TerrainFeatures = mapData.TerrainFeatures
            };
            
            string jsonString = JsonSerializer.Serialize(serializableMap, options);
            File.WriteAllText(filePath, jsonString);
        }
        
        public static MapData LoadFromJson(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Map file not found: {filePath}");
            }
            
            string jsonString = File.ReadAllText(filePath);
            var serializableMap = JsonSerializer.Deserialize<SerializableMapData>(jsonString);
            
            if (serializableMap == null)
            {
                throw new InvalidOperationException("Failed to deserialize map data");
            }
            
            // Create a new MapData and populate it
            var mapData = new MapData();
            mapData.Width = serializableMap.Width;
            mapData.Height = serializableMap.Height;
            mapData.Tiles = Unflatten2DArray(serializableMap.TileData, serializableMap.Width, serializableMap.Height);
            mapData.TerrainFeatures = serializableMap.TerrainFeatures ?? new List<TerrainFeature>();
            mapData.IsLoaded = true;
            
            return mapData;
        }
        
        // Helper methods to convert between 2D arrays and flat arrays for JSON serialization
        private static TileData[] FlattenTileArray(TileData[,] tiles)
        {
            int width = tiles.GetLength(0);
            int height = tiles.GetLength(1);
            TileData[] flat = new TileData[width * height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    flat[y * width + x] = tiles[x, y];
                }
            }
            
            return flat;
        }
        
        private static TileData[,] Unflatten2DArray(TileData[] flat, int width, int height)
        {
            TileData[,] tiles = new TileData[width, height];
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    tiles[x, y] = flat[y * width + x];
                }
            }
            
            return tiles;
        }
    }

    // For JSON serialization of 2D array data
    public class SerializableMapData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public TileData[] TileData { get; set; } = Array.Empty<TileData>(); // Initialize with empty array
        public List<TerrainFeature> TerrainFeatures { get; set; } = new List<TerrainFeature>();
    }

    // Represents a significant terrain feature for tracking
    public class TerrainFeature
    {
        public string Type { get; set; } = string.Empty; // Initialize with empty string
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
    
    public class Map
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Tile[,] Tiles { get; private set; }
        public MapData SourceData { get; private set; }

        // For rendering smaller sections of the map (viewport)
        public int CameraX { get; set; } = 0;
        public int CameraY { get; set; } = 0;
        public int ViewportWidth { get; set; } = 80;
        public int ViewportHeight { get; set; } = 40;

        // Default constructor creates a small map
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
            
            // Create a basic map data object
            SourceData = new MapData(width, height);
        }
        
        // Create a map from MapData
        public Map(MapData mapData)
        {
            Width = mapData.Width;
            Height = mapData.Height;
            SourceData = mapData;
            Tiles = new Tile[Width, Height];
            
            // Create tiles from the map data
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var tileData = mapData.Tiles[x, y];
                    Tiles[x, y] = new Tile(tileData);
                }
            }
        }
        
        public static Map GenerateRandomMap(int width, int height, int seed = 0)
        {
            // Generate map data
            MapData mapData = MapData.GenerateRandomMap(width, height, seed);
            
            // Save to file for later editing
            string mapPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "world_map.json");
            MapData.SaveToJson(mapData, mapPath);
            Console.WriteLine($"Map saved to: {mapPath}");
            
            // Create map from data
            return new Map(mapData);
        }
        
        // Get the visible portion of the map based on camera position
        public (int startX, int startY, int endX, int endY) GetVisibleArea()
        {
            int startX = Math.Max(0, CameraX - ViewportWidth / 2);
            int startY = Math.Max(0, CameraY - ViewportHeight / 2);
            int endX = Math.Min(Width, startX + ViewportWidth);
            int endY = Math.Min(Height, startY + ViewportHeight);
            
            return (startX, startY, endX, endY);
        }
        
        // Center camera on a specific point (typically the player)
        public void CenterCamera(int x, int y)
        {
            CameraX = x;
            CameraY = y;
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
        
        // Load map from file
        public static Map LoadFromFile(string filePath)
        {
            try
            {
                var mapData = MapData.LoadFromJson(filePath);
                return new Map(mapData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading map: {ex.Message}");
                throw;
            }
        }
    }
    
    // A simple implementation of Simplex noise for terrain generation
    public class SimplexNoise
    {
        private int[] perm;

        public SimplexNoise(int seed)
        {
            perm = new int[512];
            Random random = new Random(seed);
            
            // Initialize the permutation array
            for (int i = 0; i < 256; i++)
            {
                perm[i] = i;
            }
            
            // Shuffle the permutation array
            for (int i = 0; i < 256; i++)
            {
                int j = random.Next(256);
                int temp = perm[i];
                perm[i] = perm[j];
                perm[j] = temp;
            }
            
            // Duplicate to avoid overflow
            for (int i = 0; i < 256; i++)
            {
                perm[i + 256] = perm[i];
            }
        }

        // 2D Simplex noise
        public double Evaluate(double x, double y)
        {
            const double F2 = 0.366025403; // F2 = 0.5*(sqrt(3.0)-1.0)
            const double G2 = 0.211324865; // G2 = (3.0-Math.sqrt(3.0))/6.0
            
            // Skew the input space to determine which simplex cell we're in
            double s = (x + y) * F2;
            int i = FastFloor(x + s);
            int j = FastFloor(y + s);
            
            // Unskew the cell origin back to (x,y) space
            double t = (i + j) * G2;
            double X0 = i - t;
            double Y0 = j - t;
            double x0 = x - X0; // The x,y distances from the cell origin
            double y0 = y - Y0;
            
            // For the 2D case, the simplex shape is an equilateral triangle.
            // Determine which simplex we are in.
            int i1, j1; // Offsets for second (middle) corner of simplex in (i,j) coords
            if (x0 > y0) { i1 = 1; j1 = 0; } // lower triangle, XY order: (0,0)->(1,0)->(1,1)
            else { i1 = 0; j1 = 1; }      // upper triangle, YX order: (0,0)->(0,1)->(1,1)
            
            // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
            // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
            // c = (3-sqrt(3))/6
            double x1 = x0 - i1 + G2; // Offsets for middle corner in (x,y) unskewed coords
            double y1 = y0 - j1 + G2;
            double x2 = x0 - 1.0 + 2.0 * G2; // Offsets for last corner in (x,y) unskewed coords
            double y2 = y0 - 1.0 + 2.0 * G2;
            
            // Work out the hashed gradient indices of the three simplex corners
            int ii = i & 255;
            int jj = j & 255;
            int gi0 = perm[ii + perm[jj]] % 12;
            int gi1 = perm[ii + i1 + perm[jj + j1]] % 12;
            int gi2 = perm[ii + 1 + perm[jj + 1]] % 12;
            
            // Calculate the contribution from the three corners
            double n0, n1, n2;
            
            double t0 = 0.5 - x0 * x0 - y0 * y0;
            if (t0 < 0) n0 = 0.0;
            else
            {
                t0 *= t0;
                n0 = t0 * t0 * Dot(gi0, x0, y0);
            }
            
            double t1 = 0.5 - x1 * x1 - y1 * y1;
            if (t1 < 0) n1 = 0.0;
            else
            {
                t1 *= t1;
                n1 = t1 * t1 * Dot(gi1, x1, y1);
            }
            
            double t2 = 0.5 - x2 * x2 - y2 * y2;
            if (t2 < 0) n2 = 0.0;
            else
            {
                t2 *= t2;
                n2 = t2 * t2 * Dot(gi2, x2, y2);
            }
            
            // Add contributions from each corner to get the final noise value.
            // The result is scaled to return values in the range [-1,1].
            return 70.0 * (n0 + n1 + n2);
        }
        
        private int FastFloor(double x)
        {
            int xi = (int)x;
            return x < xi ? xi - 1 : xi;
        }
        
        private double Dot(int gi, double x, double y)
        {
            // Gradient lookup table
            double[] grad3 = {
                1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1, 0,
                1, 0, 1, -1, 0, 1, 1, 0, -1, -1, 0, -1,
                0, 1, 1, 0, -1, 1, 0, 1, -1, 0, -1, -1
            };
            
            // Calculate dot product
            return grad3[gi * 3] * x + grad3[gi * 3 + 1] * y;
        }
    }
}