using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstalledObject
{
    public enum ObjectType
    {
        None,
        Tree,
        Wall,
        Floor
    }

    ObjectType type = ObjectType.None;

    public ObjectType Type
    {
        get { return type; }
        set { type = value; }
    }
}
