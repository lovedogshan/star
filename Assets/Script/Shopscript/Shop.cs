using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName= "New Item",menuName ="Shop/New Item")]
public class Shop : ScriptableObject
{
    public List<item>  itemList = new List<item>();
}
