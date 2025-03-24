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

    [Header("World Gen Adjustments")]
    public float noiseScale;
    public float dirtThreshold;
    public float treeThreshold;
    public int treeProximity;

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

    [Header("Plant Sprites")]
    public Sprite oakTreeLog;

    public WorldMap World { get; protected set; }

    public GameObject[] layers;
    public GameObject[,,] tileObjects;

    public int currentZLevel = 0;

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        World = new WorldMap();

        layers = new GameObject[World.worldZLevels];
        tileObjects = new GameObject[World.worldWidth, World.worldHeight, World.worldZLevels];

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

                    tileObjects[x, y, z] = tileObject;

                    tileData.TileTypeChangedCallback((tile) => { OnTileChange(tileData, tileObject); });
                }
            }
        }

        World.RandomizeTiles(noiseScale, dirtThreshold);
        World.RandomizeTrees(noiseScale, treeThreshold, treeProximity);


        for (int z = 0; z < World.worldZLevels; z++)
        {
            for (int y = 0; y < World.worldWidth; y++)
            {
                for (int x = 0; x < World.worldHeight; x++)
                {
                    Tile tileData = World.GetTile(x, y, z);

                    GameObject tileObject = tileObjects[x, y, z];

                    OnTileChange(tileData, tileObject);
                }
            }
        }

        for (int z = 0; z < layers.Length; z++)
        {
            if (z < 0 || z > 0)
            {
                for (int y = 0; y < World.worldWidth; y++)
                {
                    for (int x = 0; x < World.worldHeight; x++)
                    {
                        tileObjects[x, y, z].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
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
                spriteRenderer.sprite = null;
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

            case Tile.TileType.Tree:
                spriteRenderer.sprite = oakTreeLog;
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

        SpriteRenderer sprite;
        Color color;

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


        for (int layer = 0; layer <= layers.Length - 1; layer++)
        {
            if (layer == currentZLevel)
            {
                layers[layer].SetActive(true);

                for (int y = 0; y < World.worldWidth; y++)
                {
                    for (int x = 0; x < World.worldHeight; x++)
                    {
                        sprite = tileObjects[x, y, layer].GetComponent<SpriteRenderer>();
                        sprite.enabled = true;

                        color = sprite.color;
                        color.a = 255f;

                        sprite.color = color;

                    }
                }

            }
            else if (layer > currentZLevel)
            {
                for (int y = 0; y < World.worldWidth; y++)
                {
                    for (int x = 0; x < World.worldHeight; x++)
                    {
                        tileObjects[x, y, layer].GetComponent<SpriteRenderer>().enabled = false;
                    }
                }
            }
            else if (layer < currentZLevel)
            {
                if(currentZLevel - layer > 3)
                {
                    for (int y = 0; y < World.worldWidth; y++)
                    {
                        for (int x = 0; x < World.worldHeight; x++)
                        {
                            tileObjects[x, y, layer].GetComponent<SpriteRenderer>().enabled = false;
                        }
                    }
                }
                else if (currentZLevel - layer <= 3)
                {

                    for (int y = 0; y < World.worldWidth; y++)
                    {
                        for (int x = 0; x < World.worldHeight; x++)
                        {
                            sprite = tileObjects[x, y, layer].GetComponent<SpriteRenderer>();

                            if (!sprite.enabled)
                            {
                                sprite.enabled = true;
                            }

                            color = sprite.color;

                            if (currentZLevel > previousZLevel)
                            {
                                color.a = Mathf.Max(0, color.a / 2);
                            }
                            else if (currentZLevel < previousZLevel)
                            {
                                color.a = Mathf.Min(255, color.a * 2);
                            }

                            sprite.color = color;
                        }
                    }
                }
                
                
                //else if (currentZLevel > layer)
                //{
                //    for (int y = 0; y < World.worldWidth; y++)
                //    {
                //        for (int x = 0; x < World.worldHeight; x++)
                //        {
                //            SpriteRenderer sprite = tileObjects[x, y, layer].GetComponent<SpriteRenderer>();

                //            Color color = sprite.color;
                //            color.a /= 2;

                //            sprite.color = color;
                //        }
                //    }
                //}
                //else if (currentZLevel < previousZLevel)
                //{
                //    for (int y = 0; y < World.worldWidth; y++)
                //    {
                //        for (int x = 0; x < World.worldHeight; x++)
                //        {
                //            SpriteRenderer sprite = tileObjects[x, y, layer].GetComponent<SpriteRenderer>();

                //            Color color = sprite.color;
                //            color.a *= 2;

                //            sprite.color = color;
                //        }
                //    }
                //}

            }
            //else
            //{
            //    layers[layer].SetActive(false);
            //}
        }
    }
}
