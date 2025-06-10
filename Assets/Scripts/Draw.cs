using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using Unity.Netcode;
using UnityEditor.Tilemaps;


public class Draw : NetworkBehaviour
{
    public Camera m_camera;
    public GameObject brush;
    private int platform = 1;
    private Vector3 control;
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
        m_camera = GetComponent<Camera>();

        if (drawings == null) drawings = DrawingsFolder.instance.gameObject;

    }

    private void Update()
    {
        if (IsClient)
        {
            if (currentDrawing != null)
            {
                currentLineRenderer = currentDrawing.GetComponent<LineRenderer>();

            }
            Drawing();

        }
    }

    void Drawing()
    {
        control = Input.mousePosition;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            CreateNewGroup();
            ServerProcessing_Rpc(brush, drawings, activeDrawingGroup, m_camera.ScreenToWorldPoint(control));
        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            PointToMousePos_Rpc();
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Erase_Rpc();
        }
        else if (Input.GetKey(KeyCode.Mouse1))
        {
            Erase_Rpc();
        }
        else
        {
            currentLineRenderer = null;
        }
    }





    [Rpc(SendTo.Server, RequireOwnership = true)]
    void ServerProcessing_Rpc(GameObject brush, GameObject drawings, GameObject activeDrawingGroup, Vector2 oldMousePos)
    {
        GameObject brushInstance = Instantiate(brush);
        brushInstance.transform.SetParent(activeDrawingGroup.transform);
        if (drawings != null) activeDrawingGroup.transform.SetParent(drawings.transform);

        //currentDrawing = brushInstance;
        LineRenderer currentLineRenderer = brushInstance.GetComponent<LineRenderer>();

        //Debug.Log(currentLineRenderer);

        //because you gotta have 2 points to start a line renderer, 
        Vector2 mousePos = m_camera.ScreenToWorldPoint(control);
        if (oldMousePos == null)
        {
            oldMousePos = mousePos;
        }

        currentLineRenderer.SetPosition(0, oldMousePos);
        currentLineRenderer.SetPosition(1, mousePos);
        PlayerDrawSpawn_Rpc(brushInstance);
    }


    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    void PlayerDrawSpawn_Rpc(GameObject fedObject)
    {
        Instantiate(fedObject);
        currentDrawing = fedObject;
        CreateNewGroup();
    }


    void CreateNewGroup()
    {
        activeDrawingGroup = Instantiate(drawingGroup);
    }


    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    void AddAPoint_Rpc(Vector2 pointPos)
    {
        //Debug.Log(currentLineRenderer);
        //Debug.Log(currentLineRenderer.positionCount);
        //currentLineRenderer.positionCount++;

        if (currentLineRenderer.positionCount >= 2)
        {
            //CreateNewGroup();
            ServerProcessing_Rpc(brush, drawings, activeDrawingGroup, currentLineRenderer.GetPosition(1));
        }


        int positionIndex = currentLineRenderer.positionCount - 1;
        currentLineRenderer.SetPosition(positionIndex, pointPos);
    }


    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    void PointToMousePos_Rpc()
    {
        Vector2 mousePos = m_camera.ScreenToWorldPoint(control);
        //Debug.Log(mousePos);
        if (lastPos != mousePos)
        {
            //Debug.Log("Add pos");
            AddAPoint_Rpc(mousePos);
            lastPos = mousePos;
        }
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    void Erase_Rpc()
    {
        Ray ray = Camera.main.ScreenPointToRay(control);
        RaycastHit hitData_for_the_ray;
        if (Physics.Raycast(ray, out hitData_for_the_ray))
        {
            Debug.Log(hitData_for_the_ray);
            GameObject theGameObjectHitByRay = hitData_for_the_ray.collider.gameObject;
            Destroy(theGameObjectHitByRay);
        }

    }


}