using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public enum TileType
    {
        Water,
        Dirt,
        Grass,
        Stone,
        Empty,
        Floor
    }

    TileType type = TileType.Empty;

    public bool hasTreeOn;

    Action<Tile> cbTileChanged;

    public TileType Type
    {
        get { return type; }
        set 
        { 
            type = value;
            
            if(cbTileChanged != null)
                cbTileChanged(this);
        }
    }

    LooseObject looseObject;
    public InstalledObject installedObject;

    WorldMap world;
    public int x { get; set; }
    public int y { get; set; }
    public int z { get; set; }

    public Tile(WorldMap world, int x, int y, int z)
    {
        this.world = world;
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public void TileTypeChangedCallback (Action<Tile> callback)
    {
        cbTileChanged += callback;
    }
}
