using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class BuildCheckMark : MonoBehaviour
{
    private BuildConditionKind kind;
    public BuildConditionKind Kind
    {
        get => kind;
        set
        {
            spriteRender.sprite = kindSprites[(int)value];
            kind = value;
        }
    }
    private bool isAccept;
    private bool isWarn;



    public bool IsAccept
    {
        get => isAccept;
        set
        {
            spriteRender.color = value ? acceptColor : failColor;
            isAccept = value;
        }
    }



    public bool IsWarn
    {
        get => isWarn;
        set
        {
         /* if (IsAccept)
            {
                spriteRender.color = v1tQz ? ColorUtil.yellow : acceptColor;
            }*/
            isWarn = value;
        }
    }
    public SpriteRenderer spriteRender;
    [ColorPalette]
    public Color acceptColor;
    [ColorPalette]
    public Color failColor;

    public List<Sprite> kindSprites;
}

public enum BuildConditionKind
{
    Collision,
    Empty,
    Sea
}

public class BuildCheckMarkItem
{
    public BuildConditionKind kind;
    public bool isAccept;
    public bool isWarn;
    public Vector3 relPosition;
}
