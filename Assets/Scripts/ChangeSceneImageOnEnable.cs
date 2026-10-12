using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeSceneImageOnEnable : MonoBehaviour
{
    public Image targetImage;
    public Sprite newSprite;

    void OnEnable()
    {
        if (targetImage != null && newSprite != null)
        {
            targetImage.sprite = newSprite;
        }
    }
}