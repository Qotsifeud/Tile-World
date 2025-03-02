using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    public GameObject normalTileOverlay;
    public GameObject DragParent;

    Vector3 previousFramePosition;

    Vector3 dragStartPos;

    Vector3 currentFramePos;

    List<GameObject> dragTilePreview;

    // Start is called before the first frame update
    void Start()
    {
        dragTilePreview = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        currentFramePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentFramePos.z = 0;

        SelectTile();
        DragTiles();
        DragCamera();

        previousFramePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        previousFramePosition.z = 0;
    }

    void SelectTile()
    {
        Tile currentTile = WorldController.Instance.GetTileAtWorldCoord(currentFramePos);

        if(Input.GetMouseButtonUp(0))
        {
            if (currentTile != null)
            {
                normalTileOverlay.SetActive(true);
                normalTileOverlay.transform.position = new Vector3(currentTile.x, currentTile.y, -1);
            }
            else
            {
                normalTileOverlay.SetActive(false);
            }
        }
    }

    void DragCamera()
    {
        if (Input.GetMouseButton(1) || Input.GetMouseButton(2))
        {
            Vector3 diff = previousFramePosition - currentFramePos;
            Camera.main.transform.Translate(diff);
        }
    }

    void DragTiles()
    {   
        // Start Dragging
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = currentFramePos;
        }

        int dragStartX = Mathf.FloorToInt(dragStartPos.x);
        int dragEndX = Mathf.FloorToInt(currentFramePos.x);

        if (dragEndX < dragStartX)
        {
            int tmp = dragEndX;
            dragEndX = dragStartX;
            dragStartX = tmp;
        }

        int dragStartY = Mathf.FloorToInt(dragStartPos.y);
        int dragEndY = Mathf.FloorToInt(currentFramePos.y);

        if (dragEndY < dragStartY)
        {
            int tmp = dragEndY;
            dragEndY = dragStartY;
            dragStartY = tmp;
        }

        while(dragTilePreview.Count > 0)
        {
            GameObject go = dragTilePreview[0];
            dragTilePreview.RemoveAt(0);
            Destroy(go);
        }

        if (Input.GetMouseButton(0))
        {
            for (int x = dragStartX; x <= dragEndX; x++)
            {
                for (int y = dragStartY; y <= dragEndY; y++)
                {
                    Tile tile = WorldController.Instance.World.GetTile(x, y);
                    if (tile != null)
                    {
                        // Display the overlay of dragged tiles.
                        GameObject go = (GameObject)Instantiate(normalTileOverlay, new Vector3(x, y, -1), Quaternion.identity);
                        go.transform.SetParent(DragParent.transform, true);
                        dragTilePreview.Add(go);
                    }
                }
            }
        }

        // End Dragging
        if (Input.GetMouseButtonUp(0))
        {
            
        }
    }
}
