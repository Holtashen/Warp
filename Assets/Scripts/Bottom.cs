using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Bottom : MonoBehaviour
{
    public List<GameObject> texts;
    private int index = 0;

    void Start()
    {
        for (int i = 0; i < texts.Count; i++)
        {
            texts[i].SetActive(i == 0);
        }
    }

    public void Next()
    {
        Debug.Log("xxxxx");
        if (index >= texts.Count - 1)
            return;

        texts[index].SetActive(false);
        index++;
        texts[index].SetActive(true);
    }
}
    

