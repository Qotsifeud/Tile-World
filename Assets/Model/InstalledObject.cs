using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstalledObject
{
    public enum ObjectType
    {
        None,
        Tree,
        Leaves,
        Bush,
        Wall,
        Floor
    }

    ObjectType type = ObjectType.None;

    public ObjectType Type
    {
        get { return type; }
        set { type = value; }
    }

    WorldMap world;

    public int x { get; set; }
    public int y { get; set; }
    public int z { get; set; }

    public InstalledObject(WorldMap world, int x, int y, int z)
    {
        this.world = world;
        this.x = x;
        this.y = y;
        this.z = z;
    }
}
