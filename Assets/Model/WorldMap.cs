using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMap
{
    Tile[,,] tiles;
    public int worldWidth { get; private set; }
    public int worldHeight { get; private set; }
    public int worldZLevels { get; private set; }

    public WorldMap(int width = 100, int height = 100, int zLevels = 2)
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

        Debug.Log("World created with " + (worldWidth *  worldHeight) + " tiles.");
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

    public void RandomizeTrees(float noiseScale, float treeThreshold)
    {
        int z = 0;

        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                if (tiles[x, y, z].Type == Tile.TileType.Dirt) { return; }

                float noiseValue = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);

                if (noiseValue < treeThreshold)
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

}
