using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldMap
{
    public readonly Vector2Int[] neighbourPositions =
    {
        Vector2Int.up, // N
        Vector2Int.left, // W
        Vector2Int.right, // E
        Vector2Int.down, // S
        Vector2Int.up + Vector2Int.right, // NE
        Vector2Int.up + Vector2Int.left, // NW
        Vector2Int.down + Vector2Int.right, // SE
        Vector2Int.down + Vector2Int.left // SW
    };

    Tile[,,] tiles;
    public int worldWidth { get; private set; }
    public int worldHeight { get; private set; }
    public int worldZLevels { get; private set; }

    public WorldMap(int width = 100, int height = 100, int zLevels = 5)
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

    public void RandomizeTiles(float noiseScale, float dirtThreshold, float waterThreshold)
    {
        // Generate random offsets for Perlin noise
        float offsetX = UnityEngine.Random.Range(0f, 10000f);
        float offsetY = UnityEngine.Random.Range(0f, 10000f);

        for (int z = 0; z < worldZLevels; z++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                for (int y = 0; y < worldHeight; y++)
                {
                    if (z > 0)
                    {
                        tiles[x, y, z].Type = Tile.TileType.Empty;
                        continue;
                    }

                    float noiseValue = Mathf.PerlinNoise((x * noiseScale) + offsetX, (y * noiseScale) + offsetY);

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

    public void RandomizeVegetation(float noiseScale, float treeThreshold, float bushThreshold, int treeHeight)
    {
        int layer = 0;
        int treeCount = 0;
        int vegProximity = 0;

        for (int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                switch (tiles[x, y, layer].Type)
                {
                    case Tile.TileType.Dirt:
                        continue;
                    case Tile.TileType.Grass:

                        UnityEngine.Random.InitState(DateTime.Now.Millisecond);

                        vegProximity = UnityEngine.Random.Range(3, 10);

                        List<Tile> proxTiles = GetTilesInProximity(x, y, layer, vegProximity);

                        bool hasVegInProximity = false;

                        foreach (Tile tile in proxTiles)
                        {
                            if (tile.installedObject != null)
                            {
                                if (tile.installedObject.Type == InstalledObject.ObjectType.Tree || tile.installedObject.Type == InstalledObject.ObjectType.Bush)
                                { 
                                    hasVegInProximity = true;
                                    break;
                                }
                            }
                        }

                        if (hasVegInProximity)
                        {
                            continue; // Skip to the next tile
                        }

                        float noiseValue = Mathf.PerlinNoise(x * noiseScale, y * noiseScale);

                        if (noiseValue < treeThreshold && noiseValue > bushThreshold)
                        {
                            //tiles[x, y, layer].installedObject.Type = InstalledObject.ObjectType.Tree;
                            
                            for (int i = 0; i <= treeHeight; i++)
                            {
                                InstalledObject tree = new InstalledObject(this, x, y, layer + i);

                                tree.Type = InstalledObject.ObjectType.Tree;

                                tiles[x, y, layer + i].installedObject = tree;
                            }

                            InstalledObject firstLeaf = new InstalledObject(this, x, y, treeHeight + 1);

                            firstLeaf.Type = InstalledObject.ObjectType.Leaves;

                            tiles[x, y, treeHeight + 1].installedObject = firstLeaf;

                            foreach(var dir in neighbourPositions)
                            {
                                int leafDirX = x + dir.x;
                                int leafDirY = y + dir.y;

                                InstalledObject leaves = new InstalledObject(this, leafDirX, leafDirY, treeHeight + 1);

                                leaves.Type = InstalledObject.ObjectType.Leaves;

                                if (leafDirX >= 0 && leafDirX < worldWidth && leafDirY >= 0 && leafDirY < worldHeight)
                                {
                                    tiles[x + dir.x, y + dir.y, treeHeight + 1].installedObject = leaves;
                                }
                            }
                        }
                        else if (noiseValue < bushThreshold)
                        {
                            InstalledObject bush = new InstalledObject(this, x, y, layer);

                            bush.Type = InstalledObject.ObjectType.Bush;

                            tiles[x, y, layer].installedObject = bush;
                        }

                        break;
                }
            }
        }

        for (int z = 0; z < worldZLevels; z++)
        {
            for (int x = 0; x < worldWidth; x++)
            {
                for (int y = 0; y < worldHeight; y++)
                {
                    Tile tile = GetTile(x, y, z);

                    if(tile.installedObject != null && tile.installedObject.Type == InstalledObject.ObjectType.Tree)
                    {
                        treeCount += 1;
                    }
                }
            }
        }

        Debug.Log("There are " + treeCount + " trees");
    }

    public void CreateWater()
    {
        AStar aStar = new AStar();

        Random.InitState(DateTime.Now.Millisecond);

        int sourceX = UnityEngine.Random.Range(0, worldWidth);
        int sourceY = 99;

        int endingX = UnityEngine.Random.Range(0, worldWidth);

        List<Tile> river = aStar.AStarPath(GetTile(sourceX, sourceY, 0), GetTile(endingX, 0, 0), this);

        if(river == null || river.Count == 0)
        {
            Debug.Log("No river path found.");
            return;
        }

        foreach(Tile tile in river)
        {
            float perlinWidth = Mathf.PerlinNoise(tile.x * 0.2f, tile.y * 0.2f);
            int width = Mathf.RoundToInt(Mathf.Lerp(1, 4, perlinWidth));

            List<Tile> neighbouringTiles = GetTilesInProximity(tile.x, tile.y, 0, UnityEngine.Random.Range(0, 5));

            tile.Type = Tile.TileType.Water;

            foreach(Tile neighbour in neighbouringTiles)
            {
                neighbour.Type = Tile.TileType.Water;
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
