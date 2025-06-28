using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using Unity.Netcode;
using UnityEditor.Tilemaps;
using System.Collections;
using System;
using Unity.Collections;


public class Draw : NetworkBehaviour
{
    public Camera m_camera;
    public GameObject brush;
    private int platform = 1;
    public GameObject drawings;
    public GameObject drawingGroup;
    private GameObject activeDrawingGroup;
    public Color playerColour = Color.white;


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

        if (IsServer)
        {
            int selectedColour;
            while (true)
            {
                selectedColour = UnityEngine.Random.Range(0, GameManager.instance.drawingColours.Count);
                if (!GameManager.instance.colours.Value.ToString().Contains(selectedColour.ToString() + "|") && !GameManager.instance.colours.Value.ToString().Contains("|" + selectedColour.ToString() + "|")) break;
            }
            GameManager.instance.colours.Value = GameManager.instance.colours.Value + selectedColour.ToString() + "|";
            playerColour = drawingColors[selectedColour];
            //playerColour = GameManager.instance.drawingColours[selectedColour];
            //SetColour_Rpc(playerColour, OwnerClientId);
        }
        else StartCoroutine(SetColour());


    }


    IEnumerator SetColour()
    {
        int id = GameObject.FindObjectsByType<Draw>(FindObjectsSortMode.None).Length;
        //int id = Convert.ToInt32(OwnerClientId);
        Debug.Log(GameManager.instance.coloursList.Count.ToString() + " , " + id.ToString());
        while (GameManager.instance.coloursList.Count < id)
        {
            Debug.Log(GameManager.instance.coloursList.Count.ToString() + " , " + id.ToString());
            yield return null;
        }
        playerColour = drawingColors[GameManager.instance.coloursList[id-1]];
    }


    private void Update()
    {
        if (IsClient && IsLocalPlayer)
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






    [Rpc(SendTo.Everyone, RequireOwnership = false)]
    void ServerProcessing_Rpc(Vector2 mousePos, Vector2 lastPos, Color playerColour, bool newItem = false)
    {
        if (newItem) return;
        GameObject brushInstance = Instantiate(brush);

        LineRenderer currentLineRenderer = brushInstance.GetComponent<LineRenderer>();

        

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
