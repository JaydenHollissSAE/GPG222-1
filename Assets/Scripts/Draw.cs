using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using Unity.Netcode;


public class Draw : NetworkBehaviour
{
    public Camera m_camera;
    public GameObject brush;
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
        m_camera = Camera.main;

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
            CreateNewGroup_Rpc(drawingGroup);
            CreateBrush_Rpc(m_camera.ScreenToWorldPoint(control));
        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            PointToMousePos(control);
        }
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Erase(control);
        }
        else if (Input.GetKey(KeyCode.Mouse1))
        {
            Erase(control);
        }
        else
        {
            currentLineRenderer = null;
        }

    }


    void SetNewGroup(GameObject group)
    {
        activeDrawingGroup = group;
        
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    void CreateNewGroup_Rpc(GameObject drawingGroup)
    {
        //GameObject localActiveDrawingGroup = Instantiate(drawingGroup);
        //SetNewGroup(localActiveDrawingGroup);
        //activeDrawingGroup = localActiveDrawingGroup;
        activeDrawingGroup = Instantiate(drawingGroup);
    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    void CreateBrush_Rpc(Vector2 oldMousepos)
    {
        GameObject brushInstance = Instantiate(brush);
        brushInstance.transform.SetParent(activeDrawingGroup.transform);
        if (drawings != null) activeDrawingGroup.transform.SetParent(drawings.transform);

        currentDrawing = brushInstance;
        currentLineRenderer = brushInstance.GetComponent<LineRenderer>();

        //Debug.Log(currentLineRenderer);

        //because you gotta have 2 points to start a line renderer, 
        Vector2 mousePos = m_camera.ScreenToWorldPoint(control);
        if (oldMousepos == null)
        {
            oldMousepos = mousePos;
        }

        currentLineRenderer.SetPosition(0, oldMousepos);
        currentLineRenderer.SetPosition(1, mousePos);

    }



    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    void AddAPoint_Rpc(Vector2 pointPos, LineRenderer localCurrentLineRenderer)
    {
        //Debug.Log(currentLineRenderer);
        //Debug.Log(currentLineRenderer.positionCount);
        //currentLineRenderer.positionCount++;

        if (localCurrentLineRenderer.positionCount >= 2)
        {
            //CreateNewGroup();
            CreateBrush_Rpc(localCurrentLineRenderer.GetPosition(1));
        }


        int positionIndex = localCurrentLineRenderer.positionCount - 1;
        localCurrentLineRenderer.SetPosition(positionIndex, pointPos);
    }


    void PointToMousePos(Vector3 mouseInput)
    {
        Vector2 mousePos = m_camera.ScreenToWorldPoint(mouseInput);
        //Debug.Log(mousePos);
        if (lastPos != mousePos)
        {
            //Debug.Log("Add pos");
            AddAPoint_Rpc(mousePos, currentLineRenderer);
            lastPos = mousePos;
        }
    }

    void Erase(Vector3 mousePos)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        RaycastHit hitData_for_the_ray;
        if (Physics.Raycast(ray, out hitData_for_the_ray))
        {
            Debug.Log(hitData_for_the_ray);
            GameObject theGameObjectHitByRay = hitData_for_the_ray.collider.gameObject;
            DestroyHit_Rpc(theGameObjectHitByRay);
        }

    }

    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    void DestroyHit_Rpc(GameObject hitObject)
    {
        Destroy(hitObject);
    }


}