using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class WorldMap
{
    Tile[,] tiles;
    public int worldWidth { get; private set; }
    public int worldHeight { get; private set; }

    public WorldMap(int width = 100, int height = 100)
    {
        this.worldWidth = width;
        this.worldHeight = height;

        tiles = new Tile[worldWidth, worldHeight];

        for (int x = 0; x < worldWidth; x++)
        {
            for(int y = 0;  y < worldHeight; y++)
            {
                tiles[x,y] = new Tile(this, x, y);
            }
        }

        Debug.Log("World created with " + (worldWidth *  worldHeight) + " tiles.");
    }
    
    public void RandomizeTiles()
    {
        for(int x = 0; x < worldWidth; x++)
        {
            for (int y = 0; y < worldHeight; y++)
            {
                if(Random.Range(0, 2) == 0)
                {
                    tiles[x, y].Type = Tile.TileType.Empty;
                }
                else
                {
                    tiles[x, y].Type = Tile.TileType.Dirt;
                }
            }
        }
    }

    public Tile GetTile(int x, int y)
    {
        if (tiles[x, y] == null)
        {
            tiles[x,y] = new Tile(this, x, y);
        }

        return tiles[x, y];
    }



}
