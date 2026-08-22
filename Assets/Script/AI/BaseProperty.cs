using System;

[Serializable]
public class BaseProperty
{
    public float Value
    {
        get
        {
            return this.baseValue + this.tempValue;
        }
    }

    public float baseValue;

    [NonSerialized]
    public float tempValue;
}

