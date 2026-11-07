using System;
using UnityEngine;

[Serializable]
public class LocalizedEntry
{
    [Tooltip("唯一文本 Key，例如 chapter01.room001.text")]
    public string key;

    [TextArea(2, 5)]
    public string chinese;

    [TextArea(2, 5)]
    public string japanese;

    [TextArea(2, 5)]
    public string english;
}