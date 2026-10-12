using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnEnable : MonoBehaviour
{
    public string sceneName;

    void OnEnable()
    {
        SceneManager.LoadScene(sceneName);
    }
}
