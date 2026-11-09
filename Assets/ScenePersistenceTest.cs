using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenePersistenceTest : MonoBehaviour
{
    private void Update()
    {
        // ∞¥ F5 «–ªªµΩ≤‚ ‘≥°æ∞
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SceneManager.LoadScene("Level-2");
        }
    }
}