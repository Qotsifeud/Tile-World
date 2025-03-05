using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static List<PooledObjectInfo> ObjectPools = new List<PooledObjectInfo>();

    public static GameObject TileDragParent;

    public enum PoolType
    {
        TileDragOverlay,
        Tile
    }

    public void Start()
    {
        TileDragParent = GameObject.Find("Tile Drag Overlays");
    }

    public static GameObject SpawnObject(GameObject obj, Vector3 spawnPos, Quaternion spawnRot, PoolType poolType)
    {
        PooledObjectInfo pool = ObjectPools.Find(p => p.LookupString == obj.name);

        if(pool == null)
        {
            pool = new PooledObjectInfo() { LookupString = obj.name };
            ObjectPools.Add(pool);
        }

        GameObject spawnableObj = null;

        foreach (GameObject inactiveObj in pool.InactiveObjects)
        {
            if (inactiveObj != null)
            {
                spawnableObj = inactiveObj;
                break;
            }
        }

        if(spawnableObj == null)
        {

            GameObject parentObject = SetParent(poolType);

            spawnableObj = Instantiate(obj, spawnPos, spawnRot);

            if(parentObject != null)
            {
                spawnableObj.transform.SetParent(parentObject.transform, true);
            }
        }
        else
        {
            spawnableObj.transform.position = spawnPos;
            spawnableObj.transform.rotation = spawnRot;
            pool.InactiveObjects.Remove(spawnableObj);
            spawnableObj.SetActive(true);
        }

        return spawnableObj;
    }

    public static void ReturnToPool(GameObject obj)
    {
        string goName = obj.name.Substring(0, obj.name.Length - 7); // Removes the (clone) at the end of instantiated game objects.

        PooledObjectInfo pool = ObjectPools.Find(p => p.LookupString == goName);

        if(pool == null)
        {
            Debug.LogWarning("Trying to release an object that has not been pooled: " + obj.name);
        }
        else
        {
            obj.SetActive(false);
            pool.InactiveObjects.Add(obj);
        }
    }

    private static GameObject SetParent(PoolType poolType)
    {
        switch(poolType)
        {
            case PoolType.TileDragOverlay:
                return TileDragParent;
        }

        return null;
    }
}

public class PooledObjectInfo
{
    public string LookupString;
    public List<GameObject> InactiveObjects = new List<GameObject>();
}
