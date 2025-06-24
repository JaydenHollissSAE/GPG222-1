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
    public GameObject drawings;
    public GameObject drawingGroup;
    private GameObject activeDrawingGroup;
    private Color playerColour = Color.white;


    public LineRenderer currentLineRenderer;
    public GameObject currentDrawing;
    public List<Color> drawingColors;
    public int prevColour = 0;

    Vector2 lastPos;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        m_camera = Camera.main;
        if (GameManager.instance == null) GameManager.instance = FindFirstObjectByType<GameManager>();
        int selectedColour = Random.Range(0, GameManager.instance.drawingColours.Count);
        GameManager.instance.colours.Value = int.Parse(GameManager.instance.colours.Value.ToString() + selectedColour.ToString());
        playerColour = GameManager.instance.drawingColours[selectedColour];
    }

    private void Update()
    {
        if (IsClient)
        {
            Drawing();
        }
    }

    void Drawing()
    {
        
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            ToServerSetup(true);
        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            ToServerSetup();
        }
    }

    Vector3 GetControl()
    {
        return Input.mousePosition;
    }






    [Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]
    void ServerProcessing_Rpc(Vector2 mousePos, Vector2 lastPos, Color playerColour, bool newItem = false)
    {
        GameObject brushInstance = Instantiate(brush);

        LineRenderer currentLineRenderer = brushInstance.GetComponent<LineRenderer>();

        if (newItem) return;

        currentLineRenderer.SetPosition(0, lastPos);
        currentLineRenderer.SetPosition(1, mousePos);
        currentLineRenderer.startColor = playerColour;
        currentLineRenderer.endColor = playerColour;
        //Debug.Log(lastPos);
        //Debug.Log(mousePos);
        brushInstance.GetComponent<LineCollider>().enabled = true;
        //brushInstance.GetComponent<NetworkObject>().Spawn();
        return;
    }

    void ToServerSetup(bool newItem = false)
    {
        Vector2 mousePos = m_camera.ScreenToWorldPoint(GetControl());
        ServerProcessing_Rpc(mousePos, lastPos, playerColour, newItem);
        lastPos = mousePos;

    }
}