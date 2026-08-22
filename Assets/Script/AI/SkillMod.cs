using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Constants;

public partial class Person
{
    [Title("技能等级")]
    [SerializeField]
    private SkillUnit skillUnit;
    public SkillUnit SkillUnit { get => skillUnit; private set => skillUnit = value; }




    public void OnSkillMod()
    {
/*      var stress = SkillUnit.RemainingStress;
        if (stress > 0)
        {
            MoodDegree += Time.deltaTime * SkillUnit.stressMoodSpeed * stress;
            VigorDegree += Time.deltaTime * SkillUnit.stressMoodSpeed * stress;
        }*/
    }
}

[System.Serializable]
public class SkillUnit
{
    [SerializeField]
    private int skillCount;



/*  public int SkillCount
    {
        get => skillCount;
        set
        {
            skillCount = v1tQz;
            if (skillCount >= 5)
            {
                SkillPanel.OnShowTip();
            }
        }
    }*/




    public int buildLevel;



    public int takeLevel;



    public int operateLevel;



    public int farmerLevel;



    public int attackLevel;



    public int medicLevel;



    public int studyLevel;



    public int shipLevel;




    public bool isSpaceman;



    public bool isDiver;

    #region 效果加成
    public const float BuildRate = 0.2f;
    public const float TakeRate = 0.2f;
    public const float OperateRate = 0.2f;
    public const float AttackRate = 2f;
    public const float StudyRate = 0.2f;
    #endregion

    #region 技能偏好
    public WorkKind likeWorkKind;
    public WorkKind unlikeWorkKind;



    public void RandomLike()
    {
        likeWorkKind = (WorkKind)Random.Range(0, (int)WorkKind.None);
        unlikeWorkKind = (WorkKind)Random.Range(0, (int)WorkKind.None);
        if (likeWorkKind == unlikeWorkKind)
        {
            likeWorkKind = WorkKind.None;
            unlikeWorkKind = WorkKind.None;
        }
    }
    #endregion

    #region 压力



    public float stressDegree;



/*    public float WillpowerDegree => RoomManager.Instance.morale;



    public float RemainingStress => stressDegree - WillpowerDegree;
    public const float stressMoodSpeed = -0.01f;
    public const float stressVigorSpeed = -0.01f;*/
    #endregion
}