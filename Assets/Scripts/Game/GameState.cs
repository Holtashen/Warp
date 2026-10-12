using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    private HashSet<string> flags = new HashSet<string>();
    private Dictionary<string, int> intVariables =
        new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Flag

    public void SetFlag(string flag)
    {
        flags.Add(flag);
    }

    public bool HasFlag(string flag)
    {
        return flags.Contains(flag);
    }

    // Integer

    public void SetInt(string key, int value)
    {
        intVariables[key] = value;
    }

    public int GetInt(string key)
    {
        if (intVariables.TryGetValue(key, out int value))
        {
            return value;
        }

        return 0;
    }

    public void AddInt(string key, int amount)
    {
        int currentValue = GetInt(key);
        intVariables[key] = currentValue + amount;
    }
}