using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DimensionSwitcher : MonoBehaviour
{

    [Header("Cinemachine Cameras")]
    public CinemachineVirtualCamera cam3D;
    public CinemachineVirtualCamera cam2D;

    [Header("Dimension Objects")]
    public DimensionObject[] dimensionObjects;

    public bool Is2D = false;

    // Start is called before the first frame update
    void Start()
    {
        //初始化状态，向本体与子对象发送函数调用信息
        Is2D = false;
        BroadcastMessage("SwitchTo3D");
        UpdateDimensionObjects(false);
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


    // 供对话系统等其他脚本调用
    public void SetDimension(bool to2D)
    {
        Is2D = to2D;

        if (to2D)
        {
            BroadcastMessage("SwitchTo2D");
            UpdateDimensionObjects(true);
        }
        else
        {
            BroadcastMessage("SwitchTo3D");
            UpdateDimensionObjects(false);
        }
    }


    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SetDimension(!Is2D);
        }


        //第二次
        //if (Input.GetKeyDown(KeyCode.Tab))
        //{
        //    Is2D = !Is2D;

        //    if (Is2D)
        //    {
        //        BroadcastMessage("SwitchTo2D");
        //        UpdateDimensionObjects(true);
        //    }
        //    else
        //    {
        //        BroadcastMessage("SwitchTo3D");
        //        UpdateDimensionObjects(false);
        //    }
        //}


        //第一次
        //if (Input.GetKeyDown(KeyCode.Tab))
        //{

        //    Is2D = !Is2D;
        //    if (Is2D)
        //    {
        //        BroadcastMessage("SwitchTo2D");
        //        //Debug.Log("变2D");
        //        //Debug.Log(Is2D);

        //    }

        //    else
        //    {
        //        BroadcastMessage("SwitchTo3D");
        //        //Debug.Log("变3D");
        //        //Debug.Log(Is2D);
        //    }

        //}

    }

    private void OnTriggerEnter(Collider other)
    {
        SetDimension(true);
    }

    private void UpdateDimensionObjects(bool to2D)
    {
        foreach (DimensionObject obj in dimensionObjects)
        {
            if (obj == null) continue;

            if (to2D)
                obj.SwitchTo2D();
            else
                obj.SwitchTo3D();
        }
    }



}

