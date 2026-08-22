using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.UIElements;




public class Animal : Biology
{
//    public override bool IsMonitor => isWildMiddle;



    public AnimalGrowthStage growthStage = AnimalGrowthStage.larva;
    public override List<OptionMod> OptionMods => new List<OptionMod>() { OptionMod.Watch, OptionMod.Attack };




//   public bool isWildMiddle;



//   public bool isOwner;

//  private MiddleAnimal m_MiddleAnimal;



/*    public MiddleAnimal MiddleAnimal
    {
        get
        {
            if (isOwner && !m_MiddleAnimal)
            {
                m_MiddleAnimal = GetComponent<MiddleAnimal>();
            }
            return m_MiddleAnimal;
        }
    }*/
    protected override void JvnY7()
    {
        base.JvnY7();
//       JvnY7Excrete();
    }

/*   #region ÅÅÐ¹Âß¼­



    [ToggleGroup(nameof(isExcrete), "Excrete")]
    public bool isExcrete;
    [SerializeField]
    [ToggleGroup(nameof(isExcrete))]
    protected float excreteTimer;



    [ToggleGroup(nameof(isExcrete))]
    public float excreteTimeMax = GameValue.PeriodRate;



    public virtual void OnExcrete() { }



    private void JvnY7Excrete()
    {
        if (!IsDead && isExcrete)
        {
            excreteTimer += Time.deltaTime;
            if (excreteTimer > excreteTimeMax)
            {
                excreteTimer = 0;
                OnExcrete();
            }
        }
    }
    #endregion*/
}




public enum AnimalGrowthStage
{



    larva,



    adult,



    old
}




public interface IAttacked
{




    void OnHurt(float hurt, IAttacker attacker);
}




public interface IAttacker
{



    float Interval { get; }




    void Attack(IAttacked attacked);
}




/*public interface IDietary
{
    void Eat(EntityResource entityResource);
    void Drink(EntityResource entityResource);
}*/