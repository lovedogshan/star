using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Cabin :Construction
{

    public Tilemap Tilemap
    {
        get
        {
            if (tilemap == null)
            {
                gameObject = Resources.Load<GameObject>($"Prefab/{tileName}");
                tilemap = gameObject.GetComponentInChildren<Tilemap>();
            }
            return tilemap;
        }
    }

    private GameObject gameObject;
    private Tilemap tilemap;

    public string tileName;

}
