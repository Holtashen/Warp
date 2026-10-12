using UnityEngine;
using UnityEngine.SceneManagement;

public class Pool : MonoBehaviour
{
   
       public Vector3 respawnPosition = new Vector3(-6.3f, 1.6f, 4.4f);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = respawnPosition;
        }
    }
}
