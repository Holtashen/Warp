using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DimensionPlayerController : MonoBehaviour
{

    public bool is2D;
    private void SwitchTo2D()
    { is2D = true; }

    private void SwitchTo3D()
    { is2D= false; }

    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    private Rigidbody rb;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    // Update is called once per frame
    void Update()
    {

        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        var movement = Vector3.zero;

        if (is2D)
        {
            movement = new Vector3(h, 0, 0) * moveSpeed;

        }
        else
        {
            movement = new Vector3(v, 0, -h) * moveSpeed;
        }

        Vector3 velocity = rb.velocity;
        velocity.x = movement.x;
        if (!is2D)
        {
            velocity.z = movement.z;
        }
        rb.velocity = velocity;


        if(Input.GetButtonDown("Jump")&&IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f);

    }

}
