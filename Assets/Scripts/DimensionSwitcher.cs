using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DimensionSwitcher : MonoBehaviour
{

    [Header("Cinemachine Cameras")]
    public CinemachineVirtualCamera cam3D;
    public CinemachineVirtualCamera cam2D;

   public bool Is2D = false;

    // Start is called before the first frame update
    void Start()
    {

        //初始化状态，向本体与子对象发送函数调用信息
        BroadcastMessage("SwitchTo3D");
    }

    void SwitchTo2D()
    {

        //逻辑：优先级高的相机会取得控制权
        cam2D.Priority = 11;
        cam3D.Priority = 10;
    }

    

    private void SwitchTo3D()
    {

        
        cam3D.Priority = 11;
        cam2D.Priority = 10;
    }
    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Is2D = !Is2D;
            if (Is2D)

                BroadcastMessage("SwitchTo2D");
            else
                BroadcastMessage("SwitchTo3D");
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        BroadcastMessage("SwitchTo2D");
    }
}
