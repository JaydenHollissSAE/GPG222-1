using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using Unity.Netcode;
using UnityEditor.Tilemaps;
using System.Collections;


public class Draw : NetworkBehaviour
{
    public Camera m_camera;
    public GameObject brush;
    private int platform = 1;
    //private Vector3 GetControl();
    public GameObject drawings;
    public GameObject drawingGroup;
    private GameObject activeDrawingGroup;


    public LineRenderer currentLineRenderer;
    public GameObject currentDrawing;
    public List<Color> drawingColors;
    public int prevColour = 0;

    Vector2 lastPos;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_camera = Camera.main;

        //if (drawings == null) drawings = DrawingsFolder.instance.gameObject;

    }

    private void Update()
    {
        if (IsClient)
        {
            //if (currentDrawing != null)
            //{
            //    currentLineRenderer = currentDrawing.GetComponent<LineRenderer>();

            //}
            Drawing();

        }
    }

    void Drawing()
    {
        
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //CreateNewGroup_Rpc();
            //ServerProcessing_Rpc(m_camera.ScreenToWorldPoint(GetControl()), true);
            ToServerSetup(true);

        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            //ServerProcessing_Rpc(m_camera.ScreenToWorldPoint(GetControl()));
            ToServerSetup();
        }
        //else
        //{
        //    currentLineRenderer = null;
        //}
    }

    Vector3 GetControl()
    {
        return Input.mousePosition;
    }






    [Rpc(SendTo.Server, RequireOwnership = false)]
    void ServerProcessing_Rpc(Vector2 mousePos, Vector2 lastPos, bool newItem = false)
    {
        GameObject brushInstance = Instantiate(brush);
        //brushInstance.transform.SetParent(activeDrawingGroup.transform);
        //if (drawings != null) activeDrawingGroup.transform.SetParent(drawings.transform);

        //currentDrawing = brushInstance;
        LineRenderer currentLineRenderer = brushInstance.GetComponent<LineRenderer>();

        //Debug.Log(currentLineRenderer);

        //because you gotta have 2 points to start a line renderer, 
        //Vector2 mousePos = m_camera.ScreenToWorldPoint(GetControl());

        //if (newItem) lastPos = mousePos;
        if (newItem) return;

        currentLineRenderer.SetPosition(0, lastPos);
        currentLineRenderer.SetPosition(1, mousePos);
        brushInstance.GetComponent<LineCollider>().GenerateMesh_Rpc();
        //currentDrawing = brushInstance;
        //lastPos = mousePos;
        brushInstance.GetComponent<NetworkObject>().Spawn();
        //CreateNewGroup_Rpc();
        //PlayerDrawSpawn_Rpc(brushInstance.GetComponent<NetworkObject>());
        return;
    }

    void ToServerSetup(bool newItem = false)
    {
        //StartCoroutine(ServerAwaitLoop(, newItem));
        //ServerProcessing_Rpc();
        Vector2 mousePos = m_camera.ScreenToWorldPoint(GetControl());
        ServerProcessing_Rpc(mousePos, lastPos, newItem);
        lastPos = mousePos;

    }

    //IEnumerator ServerAwaitLoop(Vector2 mousePos, bool newItem)
    //{
    //    bool waitForFunc = ServerProcessing_Rpc(mousePos, newItem);
    //    while (waitForFunc)
    //    {
    //        yield return null;
    //    }
    //    lastPos = mousePos;

    //}



    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    void CreateNewGroup_Rpc()
    {
        activeDrawingGroup = Instantiate(drawingGroup);
    }


    /*[Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    void AddAPoint_Rpc(Vector2 pointPos)
    {
        //Debug.Log(currentLineRenderer);
        //Debug.Log(currentLineRenderer.positionCount);
        //currentLineRenderer.positionCount++;

        if (currentLineRenderer.positionCount >= 2)
        {
            //CreateNewGroup();
            ServerProcessing_Rpc(m_camera.ScreenToWorldPoint(GetControl())currentLineRenderer.GetPosition(1));
        }


        currentLineRenderer.SetPosition(0, lastPos);
        int positionIndex = currentLineRenderer.positionCount - 1;
        currentLineRenderer.SetPosition(positionIndex, pointPos);
    }*/


    /*[Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    void PointToMousePos_Rpc()
    {
        Vector2 mousePos = m_camera.ScreenToWorldPoint(GetControl());
        //Debug.Log(mousePos);
        if (lastPos != mousePos)
        {
            //Debug.Log("Add pos");
            //AddAPoint_Rpc(mousePos);
            
        }
    }*/

    //[Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    //void Erase_Rpc()
    //{
    //    Ray ray = Camera.main.ScreenPointToRay(GetControl());
    //    RaycastHit hitData_for_the_ray;
    //    if (Physics.Raycast(ray, out hitData_for_the_ray))
    //    {
    //        Debug.Log(hitData_for_the_ray);
    //        GameObject theGameObjectHitByRay = hitData_for_the_ray.collider.gameObject;
    //        Destroy(theGameObjectHitByRay);
    //    }

    //}


}