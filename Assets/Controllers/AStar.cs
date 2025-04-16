using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class Node
{
    public Tile tile { get; private set; }
    public bool IsTraversable { get; set; }
    public float G { get; private set; }
    public float H { get; private set; }
    public float F { get { return this.G + this.H; } }
    public NodeState State { get; set; }
    public Node ParentNode;

    public Node(Tile tile)
    {
        this.tile = tile;
        this.G = float.MaxValue;
    }

    public void SetG(float gValue)
    {
        this.G = gValue;
    }

    public void SetHeuristic(Tile current, Tile target)
    {
        this.H = Mathf.Abs(current.x - target.x) + Mathf.Abs(current.y - target.y);
    }
}

public enum NodeState { Untested, Open, Closed };

public class AStar
{
    public WorldMap map;
    
    List<Node> openSet = new List<Node>();
    List<Node> closedSet = new List<Node>();

    Dictionary<Tile, Node> nodeMap = new Dictionary<Tile, Node>();

    Vector2Int[] neighbouringNodes;

    public List<Tile> AStarPath(Tile startingTile, Tile endingTile, WorldMap worldMap)
    {
        Debug.Log("Ending Tile: x = " + endingTile.x + " / y = " + endingTile.y + " | Starting Tile: x = " + startingTile.x + " / y = " + startingTile.y);

        List <Tile> finalPath = new List<Tile>();

        map = worldMap;

        neighbouringNodes = map.neighbourPositions;

        int movementCost = 1;

        Node startingNode = new Node(startingTile);

        startingNode.SetG(0);
        startingNode.SetHeuristic(startingTile, endingTile);
        startingNode.ParentNode = null;

        openSet.Add(startingNode);

        nodeMap.Add(startingTile, startingNode);

        Node current = null;

        while (openSet.Count > 0)
        {
            current = GetLowestFNode(openSet);

            if (current.tile == endingTile)
            {
                break;
            }

            Debug.Log("A* opened tile: (" + current.tile.x + ", " + current.tile.y + ")");

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var dir in neighbouringNodes)
            {
                int x = current.tile.x + dir.x;
                int y = current.tile.y + dir.y;

                if (x >= 0 && x < map.worldWidth && y >= 0 && y < map.worldHeight)
                {

                    Node neighbourNode = new Node(null);
                    Tile neighbour = map.GetTile(x, y);


                    Debug.Log("Trying neighbour: (" + neighbour.x + ", " + y + ")");

                    //if (!IsTraversable(neighbour))
                    //{
                    //    continue;
                    //}

                    if(!nodeMap.ContainsKey(neighbour))
                    {
                        neighbourNode = new Node(neighbour);
                        nodeMap.Add(neighbour, neighbourNode);
                    }
                    else if (nodeMap.ContainsKey(neighbour))
                    {
                        neighbourNode = nodeMap[neighbour];
                    }

                    if (!closedSet.Contains(neighbourNode))
                    {
                        float tempG = current.G + movementCost;


                        Debug.Log("Checking G: tempG = " + tempG + ", current G = " + neighbourNode.G + " for tile: " + neighbour.x + "," + neighbour.y);

                        if (tempG < neighbourNode.G)
                        {
                            neighbourNode.SetG(tempG);
                            neighbourNode.ParentNode = current;
                        }

                        Debug.Log("Updated G to " + tempG + ", Parent set to " + current.tile.x + "," + current.tile.y);

                        neighbourNode.SetHeuristic(neighbour, endingTile);
                    }

                    if (!openSet.Contains(neighbourNode))
                    {
                        openSet.Add(neighbourNode);
                    }
                }
            }
        }

        if(!nodeMap.ContainsKey(endingTile) || nodeMap[endingTile].ParentNode == null)
        {
            Debug.LogWarning("A* Failed to reach the target.");
            return new List<Tile>();
        }

        try
        {
            Node pathNode = nodeMap[endingTile];

            while (pathNode != null)
            {
                finalPath.Add(pathNode.tile);
                pathNode = pathNode.ParentNode;
            }

            finalPath.Reverse();
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }

        return finalPath;
    }

    public bool IsTraversable(Tile tile)
    {
        if (tile.Type != Tile.TileType.Dirt || tile.Type != Tile.TileType.Grass || tile.installedObject != null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public Node GetLowestFNode(List<Node> set)
    {
        Node lowestNode;

        lowestNode = set[0];

        foreach (var node in set)
        {
            if(node.F <  lowestNode.F)
            {
                lowestNode = node;
            }
        }

        return lowestNode;
    }
}
