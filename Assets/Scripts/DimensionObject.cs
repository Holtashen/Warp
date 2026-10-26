using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DimensionObject : MonoBehaviour
{

    [Header("Colliders")]
    public Collider collider_2D;
    public Collider collider_3D;

   
    public void SwitchTo2D()
    {
        if (collider_2D != null)
            collider_2D.enabled = true;

        if (collider_3D != null)
            collider_3D.enabled = false;

    }

    public void SwitchTo3D()
    {
        if (collider_2D != null)
            collider_2D.enabled = false;

        if (collider_3D != null)
            collider_3D.enabled = true;

    }

    void OnCollisionEnter(Collision collision)
    {
        var player = collision.gameObject.GetComponent<DimensionPlayerController>();

        if (player && player.is2D)
        {
            var contact = collision.contacts[0];



            var hitNormal = -contact.normal;


            if (Vector3.Dot(hitNormal, Vector3.up) > 0.9f)
            {
                var rb = player.GetComponent<Rigidbody>();
                rb.position = new Vector3(player.transform.position.x,player.transform.position.y,transform.position.z);
            }
        }
    }
    
}
