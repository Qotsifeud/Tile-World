using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Experimental.GraphView.GraphView;

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
    
    // Arrays for various things.
    public GameObject[] layers;
    public GameObject[,,] tileObjects;
    public Dictionary<InstalledObject, GameObject> installedObjects = new Dictionary<InstalledObject, GameObject>();

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
                    TreeCreation(tileData, tileObject);
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
                    else if (neighbour == Vector3Int.left && neighbourTile.Type == Tile.TileType.Dirt)
                    {
                        hasDirtLeft = true;
                    }
                    else if (neighbour == Vector3Int.right && neighbourTile.Type == Tile.TileType.Dirt)
                    {
                        hasDirtRight = true;
                    }
                    else if (neighbour == Vector3Int.down && neighbourTile.Type == Tile.TileType.Dirt)
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
                else if (hasDirtRight && !hasDirtLeft)
                {
                    if (hasDirtAbove && hasDirtBelow)
                    {
                        spriteRenderer.sprite = rightIsolatedGrass;
                        return;
                    }
                    else
                    {
                        spriteRenderer.sprite = rightGrass;
                        return;
                    }
                }
                else
                {
                    // Default to normal grass
                    spriteRenderer.sprite = grassSprite;
                    return;
                }
            
        }
    }

    public void TreeCreation(Tile tileData, GameObject tile)
    {
        int treeHeight = 2;

        if (tileData.installedObject != null)
        {
            if (tileData.installedObject.Type == InstalledObject.ObjectType.Tree)
            {
                for (int i = 0; i <= treeHeight; i++)
                {
                    GameObject treeObject = new GameObject();
                    treeObject.transform.position = new Vector3(tile.transform.position.x, tile.transform.position.y, tile.transform.position.z + i);
                    treeObject.name = "Tree_" + tile.transform.position.x + "_" + tile.transform.position.y + "_" + tile.transform.position.z + i;
                    treeObject.AddComponent<SpriteRenderer>().sprite = oakTreeLog;

                    treeObject.transform.parent = tileObjects[(int)tile.transform.position.x, (int)tile.transform.position.y, (int)tile.transform.position.z + i].transform;

                    installedObjects.Add(tileData.installedObject, treeObject);
                }
            }
        }
        else
        {
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

        // Ensure currentZLevel is within valid range
        currentZLevel = Mathf.Clamp(currentZLevel, 0, layers.Length);

        for (int layer = 0; layer < layers.Length; layer++)
        {
            if (layer == currentZLevel)
            {
                layers[layer].SetActive(true);

                for (int y = 0; y < World.worldWidth; y++)
                {
                    for (int x = 0; x < World.worldHeight; x++)
                    {
                        SpriteRenderer tileSprite = GetTileSpriteRenderer(x, y, layer);
                        tileSprite.enabled = true;

                        Color color = tileSprite.color;
                        color.a = 1f; // Set alpha to fully opaque

                        tileSprite.color = color;

                        SpriteRenderer treeSprite = GetObjSpriteRenderer(x, y, layer, World.GetTile(x, y, layer).installedObject);

                        if (treeSprite != null)
                        {
                            treeSprite.enabled = true;
                            Color treeColor = treeSprite.color;
                            treeColor.a = 1f; // Ensure it's fully visible at the current Z-level
                            treeSprite.color = treeColor;
                        }
                    }
                }
            }
            else if (layer > currentZLevel)
            {
                for (int y = 0; y < World.worldWidth; y++)
                {
                    for (int x = 0; x < World.worldHeight; x++)
                    {
                        GetTileSpriteRenderer(x, y, layer).enabled = false;

                        GetObjSpriteRenderer(x, y, layer, World.GetTile(x, y, layer).installedObject).enabled = false;
                    }
                }
            }
            else if (layer < currentZLevel)
            {
                if (currentZLevel - layer > 3)
                {
                    for (int y = 0; y < World.worldWidth; y++)
                    {
                        for (int x = 0; x < World.worldHeight; x++)
                        {
                            GetTileSpriteRenderer(x, y, layer).enabled = false;


                            InstalledObject temp = World.GetTile(x, y, layer).installedObject;

                            if (temp != null)
                            {
                                GetObjSpriteRenderer(x, y, layer, temp).enabled = false;
                            }
                        }
                    }
                }
                else if (currentZLevel - layer <= 3)
                {
                    for (int y = 0; y < World.worldWidth; y++)
                    {
                        for (int x = 0; x < World.worldHeight; x++)
                        {
                            SpriteRenderer tileSprite = GetTileSpriteRenderer(x, y, layer);
                            SpriteRenderer treeSprite = new SpriteRenderer();

                            InstalledObject temp = World.GetTile(x, y, layer).installedObject;

                            if (temp != null && temp.Type == InstalledObject.ObjectType.Tree)
                            {
                                treeSprite = GetObjSpriteRenderer(x, y, layer, temp);

                                if (!treeSprite.enabled)
                                {
                                    treeSprite.enabled = true;
                                }
                            }

                            if (!tileSprite.enabled)
                            {
                                tileSprite.enabled = true;
                            }
                            

                            Color color = tileSprite.color;

                            if (currentZLevel > previousZLevel)
                            {
                                color.a = Mathf.Max(0, color.a / 2f);
                            }
                            else if (currentZLevel < previousZLevel)
                            {
                                color.a = Mathf.Min(1, color.a * 2f);
                            }

                            treeSprite.color = color;
                            tileSprite.color = color;
                        }
                    }
                }
            }
        }
    }

    public SpriteRenderer GetTileSpriteRenderer(int x, int y, int z)
    {
        return tileObjects[x, y, z].GetComponent<SpriteRenderer>();
    }

    public SpriteRenderer GetObjSpriteRenderer(int x, int y, int z, InstalledObject obj)
    {
        return installedObjects[obj].GetComponent<SpriteRenderer>();
    }
}
