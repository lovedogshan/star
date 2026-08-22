using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using static Constants;

public class ConstructionConfigItem : ScriptableObject
{
    [ReadOnly] 
    public string guid;
    [ReadOnly] 
    public int id;

    public string explain;
    
    [Tooltip("标准实体预制ID")]
// [ValueDropdown(nameof(GetEnitityDropdown))]
    public int construcitionPrefabID;
    
    public TechTreeNodeKind kind;
    
    [PreviewField]
    public Sprite showSorite;
    
    [PreviewField]
    public Sprite buildSprite;
    public List<Vector2Int> collisions = new List<Vector2Int>();
}
    

    
