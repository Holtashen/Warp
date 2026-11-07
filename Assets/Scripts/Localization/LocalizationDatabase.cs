using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "LocalizationDatabase",
    menuName = "Game/Localization Database"
)]
public class LocalizationDatabase : ScriptableObject
{
    public List<LocalizedEntry> entries = new List<LocalizedEntry>();
}