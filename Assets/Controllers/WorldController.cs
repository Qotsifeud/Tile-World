using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    public static WorldController Instance
    {
        get; protected set;
    }

    public Sprite dirtSprite;

    public WorldMap World { get; protected set; }

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        World = new WorldMap();

        // Creating a GameObject for each of our tiles.

        for (int x = 0; x < World.worldWidth; x++)
        {
            for (int y = 0; y < World.worldHeight; y++)
            {
                Tile tileData = World.GetTile(x, y);

                GameObject tileObject = new GameObject();
                tileObject.name = "Tile_" + x + "_" + y;
                tileObject.transform.position = new Vector3(tileData.x, tileData.y);
                tileObject.transform.SetParent(this.transform, true);

                tileObject.gameObject.AddComponent<SpriteRenderer>();

                tileData.TileTypeChangedCallback((tile) => { OnTileChange(tileData, tileObject); });
            }
        }

        World.RandomizeTiles();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTileChange(Tile tileData, GameObject tile)
    {
        if(tileData.Type == Tile.TileType.Dirt)
        {
            tile.GetComponent<SpriteRenderer>().sprite = dirtSprite;
        }
        else if(tileData.Type == Tile.TileType.Empty)
        {
            tile.GetComponent<SpriteRenderer>().sprite = null;
        }
    }

    public Tile GetTileAtWorldCoord(Vector3 mousePos)
    {
        int x = Mathf.FloorToInt(mousePos.x);
        int y = Mathf.FloorToInt(mousePos.y);

        return World.GetTile(x, y);
    }
}
