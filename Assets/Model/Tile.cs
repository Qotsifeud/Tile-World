using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public enum TileType
    {
        Dirt,
        Stone,
        Empty,
        Floor
    }

    TileType type = TileType.Empty;

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
    InstalledObject installedObject;

    WorldMap world;
    public int x { get; set; }
    public int y { get; set; }

    public Tile(WorldMap world, int x, int y)
    {
        this.world = world;
        this.x = x;
        this.y = y;
    }

    public void TileTypeChangedCallback (Action<Tile> callback)
    {
        cbTileChanged += callback;
    }
}
