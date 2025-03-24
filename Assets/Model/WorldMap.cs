using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMap
{
    Tile[,,] tiles;
    public int worldWidth { get; private set; }
    public int worldHeight { get; private set; }
    public int worldZLevels { get; private set; }

    public WorldMap(int width = 100, int height = 100, int zLevels = 4)
    {
        this.worldWidth = width;
        this.worldHeight = height;
        this.worldZLevels = zLevels;

        tiles = new Tile[worldWidth, worldHeight, worldZLevels];

        for (int z = 0; z < worldZLevels; z++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                for (int y = 0; y < worldHeight; y++)
                {

                    tiles[x, y, z] = new Tile(this, x, y, z);

                }
            }
        }

        Debug.Log("World created with " + (worldWidth *  worldHeight * worldZLevels) + " tiles.");
    }
    
    public void RandomizeTiles(float noiseScale, float dirtThreshold)
    {
        for (int z = 0; z < worldZLevels; z++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                for (int y = 0; y < worldHeight; y++)
                {
                    if (z > 0)
                    {
                        tiles[x, y, z].Type = Tile.TileType.Empty;
                        return;
                    }

                    float noiseValue = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);

                    if (noiseValue < dirtThreshold)
                    {
                        tiles[x, y, z].Type = Tile.TileType.Grass;
                    }
                    else
                    {
                        tiles[x, y, z].Type = Tile.TileType.Dirt;
                    }
                }
            }
        }
    }

    public void RandomizeTrees(float noiseScale, float treeThreshold, int treeProximity)
    {
        int layer = 0;

        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                switch (tiles[x, y, layer].Type)
                {
                    case Tile.TileType.Dirt:
                        continue;
                    case Tile.TileType.Grass:

                        List<Tile> proxTiles = GetTilesInProximity(x, y, layer, treeProximity);

                        bool hasTreeInProximity = false;
                        foreach (Tile tile in proxTiles)
                        {
                            if (tile.Type == Tile.TileType.Tree)
                            {
                                hasTreeInProximity = true;
                                break;
                            }
                        }

                        if (hasTreeInProximity)
                        {
                            continue; // Skip to the next tile
                        }

                        float noiseValue = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);

                        if (noiseValue < treeThreshold)
                        {
                            tiles[x, y, layer].Type = Tile.TileType.Tree;
                        }

                        break;
                }
            }
        }

        int treeCount = 0;

        for (int z = 0; z < worldZLevels; z++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                for (int y = 0; y < worldHeight; y++)
                {
                    Tile tile = GetTile(x, y, z);

                    if (tile.Type == Tile.TileType.Tree)
                    {
                        for (int tH = 0; tH < 3; tH++) // th = tree height
                        {
                            if (z + tH >= worldZLevels) { break; } // Ensure we don't go out of bounds

                            tile = GetTile(x, y, z + tH);

                            tile.Type = Tile.TileType.Tree;

                            treeCount += 1;
                        }
                    }
                }
            }
        }

        Debug.Log("There are " + treeCount + " trees");
    }

    public Tile GetTile(int x, int y)
    {
        if (tiles[x, y, 0] == null)
        {
            tiles[x,y, 0] = new Tile(this, x, y, 0);
        }

        return tiles[x, y, 0];
    }

    public Tile GetTile(int x, int y, int z)
    {
        if (tiles[x, y, z] == null)
        {
            tiles[x, y, z] = new Tile(this, x, y, z);
        }

        return tiles[x, y, z];
    }

    public List<Tile> GetTilesInProximity(int x, int y, int z, int proximity)
    {
        List<Tile> tilesInProximity = new List<Tile>();

        for (int dx = -proximity; dx <= proximity; dx++)
        {
            for (int dy = -proximity; dy <= proximity; dy++)
            {
                int nx = x + dx;
                int ny = y + dy;

                if (nx >= 0 && nx < worldWidth && ny >= 0 && ny < worldHeight)
                {
                    tilesInProximity.Add(GetTile(nx, ny, z));
                }
            }
        }

        return tilesInProximity;
    }

}
