using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WorldController : MonoBehaviour
{
    private readonly Vector3Int[] neighbourPositions =
    {
        Vector3Int.up,
        Vector3Int.left,
        Vector3Int.right,
        Vector3Int.down
    
        // if you also wanted to get diagonal neighbours
        //Vector3Int.up + Vector3Int.right,
        //Vector3Int.up + Vector3Int.left,
        //Vector3Int.down + Vector3Int.right,
        //Vector3Int.down + Vector3Int.left
    };

    public static WorldController Instance
    {
        get; protected set;
    }

    [Header("Noise Adjustments")]
    public float noiseScale;
    public float dirtThreshold;

    [Header("Testing Sprites")]
    public Sprite air;

    [Header("Grass Tile Sprites")]
    public Sprite dirtSprite;
    public Sprite grassSprite;
    public Sprite topGrass;
    public Sprite leftGrass;
    public Sprite rightGrass;
    public Sprite bottomGrass;
    public Sprite topIsolatedGrass;
    public Sprite leftIsolatedGrass;
    public Sprite rightIsolatedGrass;
    public Sprite bottomIsolatedGrass;
    public Sprite leftTopCornerGrass;
    public Sprite rightTopCornerGrass;
    public Sprite leftBottomCornerCrass;
    public Sprite rightBottomCornerCrass;

    public WorldMap World { get; protected set; }

    public GameObject[] layers;

    public int currentZLevel = 0;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        World = new WorldMap();

        layers = new GameObject[World.worldZLevels];

        // Creating a GameObject for each of our tiles.


        for (int z = 0; z < World.worldZLevels; z++)
        {
            GameObject layerParent = new GameObject();
            layerParent.name = "Layer " + z;
            layerParent.transform.SetParent(this.transform, true);

            layers[z] = layerParent;

            for (int y = 0; y < World.worldWidth; y++)
            {
                for (int x = 0; x < World.worldHeight; x++)
                {
                    Tile tileData = World.GetTile(x, y, z);

                    GameObject tileObject = new GameObject();
                    tileObject.name = "Tile_" + x + "_" + y + "_" + z;
                    tileObject.transform.position = new Vector3(tileData.x, tileData.y, tileData.z);

                    tileObject.transform.SetParent(layerParent.transform, true);

                    tileObject.gameObject.AddComponent<SpriteRenderer>();

                    tileData.TileTypeChangedCallback((tile) => { OnTileChange(tileData, tileObject); });
                }
            }
        }

        World.RandomizeTiles(noiseScale, dirtThreshold);


        for (int z = 0; z < World.worldZLevels; z++)
        {
            for (int y = 0; y < World.worldWidth; y++)
            {
                for (int x = 0; x < World.worldHeight; x++)
                {
                    Tile tileData = World.GetTile(x, y, z);

                    GameObject tileObject = GameObject.Find("Tile_" + x + "_" + y + "_" + z);

                    OnTileChange(tileData, tileObject);
                }
            }
        }

        for (int i = 0; i < layers.Length; i++)
        {
            if (i > 0)
            {
                layers[i].SetActive(false);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTileChange(Tile tileData, GameObject tile)
    {
        SpriteRenderer spriteRenderer = tile.GetComponent<SpriteRenderer>();

        switch (tileData.Type)
        {
            case Tile.TileType.Empty:
                spriteRenderer.sprite = air;
                return;

            case Tile.TileType.Dirt:
                spriteRenderer.sprite = dirtSprite;
                return;

            case Tile.TileType.Grass:
                bool hasDirtAbove = false;
                bool hasDirtLeft = false;
                bool hasDirtRight = false;
                bool hasDirtBelow = false;

                foreach (var neighbour in neighbourPositions)
                {
                    int neighbourX = (int)tile.transform.position.x + neighbour.x;
                    int neighbourY = (int)tile.transform.position.y + neighbour.y;

                    if (neighbourX < 0 || neighbourX >= World.worldWidth || neighbourY < 0 || neighbourY >= World.worldHeight)
                        continue;

                    Tile neighbourTile = World.GetTile(neighbourX, neighbourY);
                    // Ensure we don't access out-of-bounds tiles

                    if (neighbour == Vector3Int.up && neighbourTile.Type == Tile.TileType.Dirt)
                    {
                        hasDirtAbove = true;
                    }
                    else if(neighbour == Vector3Int.left && neighbourTile.Type == Tile.TileType.Dirt)
                    {
                        hasDirtLeft = true;
                    }
                    else if(neighbour == Vector3Int.right && neighbourTile.Type == Tile.TileType.Dirt)
                    {
                        hasDirtRight = true;
                    }
                    else if(neighbour == Vector3Int.down && neighbourTile.Type == Tile.TileType.Dirt)
                    {
                        hasDirtBelow = true;
                    }
                }

                if (hasDirtAbove && !hasDirtBelow)
                {
                    if (hasDirtRight && hasDirtLeft)
                    {
                        spriteRenderer.sprite = topIsolatedGrass;
                        return;
                    }
                    else if (hasDirtLeft & !hasDirtRight)
                    {
                        spriteRenderer.sprite = leftTopCornerGrass;
                        return;
                    }
                    else if (hasDirtRight & !hasDirtLeft)
                    {
                        spriteRenderer.sprite = rightTopCornerGrass;
                        return;
                    }
                    else
                    {
                        spriteRenderer.sprite = topGrass;
                        return;
                    }
                }
                else if (hasDirtBelow && !hasDirtAbove)
                {
                    if (hasDirtRight && hasDirtLeft)
                    {
                        spriteRenderer.sprite = bottomIsolatedGrass;
                        return;
                    }
                    else if (hasDirtLeft && !hasDirtRight)
                    {
                        spriteRenderer.sprite = leftBottomCornerCrass;
                        return;
                    }
                    else if (hasDirtRight && !hasDirtLeft)
                    {
                        spriteRenderer.sprite = rightBottomCornerCrass;
                        return;
                    }
                    else
                    {
                        spriteRenderer.sprite = bottomGrass;
                        return;
                    }
                }
                else if (hasDirtLeft && !hasDirtRight)
                {
                    if (hasDirtAbove && hasDirtBelow)
                    {
                        spriteRenderer.sprite = leftIsolatedGrass;
                        return;
                    }
                    else
                    {
                        spriteRenderer.sprite = leftGrass;
                        return;
                    }
                    
                }
                else if (hasDirtRight && hasDirtAbove && hasDirtBelow)
                {
                    spriteRenderer.sprite = rightIsolatedGrass;
                    return;
                }
                else
                {
                    // Default to normal grass
                    spriteRenderer.sprite = grassSprite;
                    return;
                }

                return;
        }
    }

    public Tile GetTileAtWorldCoord(Vector3 mousePos)
    {
        int x = Mathf.FloorToInt(mousePos.x);
        int y = Mathf.FloorToInt(mousePos.y);

        return World.GetTile(x, y);
    }

    public void ChangeZLayer(float direction)
    {
        int previousZLevel = currentZLevel;

        if (direction > 0f)
        {
            currentZLevel -= 1;
            Debug.Log("current z level is " + currentZLevel);
        }
        else
        {
            currentZLevel += 1;
            Debug.Log("current z level is " + currentZLevel);
        }

        for (int layer = 0;  layer < layers.Length; layer++)
        {
            if (layer == currentZLevel)
            {
                layers[layer].SetActive(true);
            }
            else
            {
                layers[layer].SetActive(false);
            }
        }
    }
}
