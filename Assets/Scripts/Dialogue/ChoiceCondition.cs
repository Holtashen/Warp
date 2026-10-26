using System;
using UnityEngine;

[Serializable]
public class ChoiceCondition
{
    public ConditionType type;

    public string key;

    public int value;
}

public enum ConditionType
{
    HasFlag,
    IntGreaterOrEqual,
    IntLessOrEqual,
    NotHasFlag
}