using System;
using UnityEngine;

[Serializable]
public class ChoiceEffect
{
    public EffectType type;

    public string key;

    public int value;
}

public enum EffectType
{
    SetFlag,
    AddInt
}