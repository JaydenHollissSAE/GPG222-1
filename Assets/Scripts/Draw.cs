using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using Unity.Netcode;
using UnityEditor.Tilemaps;
using System.Collections;
using System;
using Unity.Collections;
using TMPro;
using UnityEngine.UI;


public class Draw : NetworkBehaviour
{
    public Camera m_camera;
    public GameObject brush;
    private int platform = 1;
    public GameObject drawings;
    public GameObject drawingGroup;
    private GameObject activeDrawingGroup;
    public Color playerColour = Color.white;
    private float inkMultiplier = 50f;


    public float currentInk;
    public float maxInk = 100f;

    public LineRenderer currentLineRenderer;
    public GameObject currentDrawing;
    public List<Color> drawingColours;
    public int prevColour = 0;
    public int selectedColour;
    private TextMeshProUGUI inkCounter;
    private bool freeDrawActive = false;

    Vector2 lastPos;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (GameManager.instance == null) GameManager.instance = FindFirstObjectByType<GameManager>();
        drawingColours = GameManager.instance.drawingColours;
        maxInk = GameManager.instance.maxInk;
        currentInk = GameManager.instance.maxInk;
        freeDrawActive = GameManager.instance.freeDrawActive;
        m_camera = Camera.main;

        if (IsServer)
        {
            
            while (true)
            {
                selectedColour = UnityEngine.Random.Range(0, GameManager.instance.drawingColours.Count);
                if (!GameManager.instance.colours.Value.ToString().Contains(selectedColour.ToString() + "|") && !GameManager.instance.colours.Value.ToString().Contains("|" + selectedColour.ToString() + "|")) break;
            }
            GameManager.instance.colours.Value = GameManager.instance.colours.Value + selectedColour.ToString() + "|";
            GameManager.instance.drawList.Add(this);
            SetColourSprites(drawingColours[selectedColour]);
            //playerColour = GameManager.instance.drawingColours[selectedColour];
            //SetColour_Rpc(playerColour, OwnerClientId);
        }
        else StartCoroutine(SetColour());
        if (!freeDrawActive) inkCounter = GetInkText();
        if (IsOwner) GameManager.instance.localDraw = this;


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
        selectedColour = GameManager.instance.coloursList[id - 1];
        SetColourSprites(drawingColours[selectedColour]);
    }

    public void SetColourSprites(Color inputColour)
    {
        playerColour = inputColour;
        if (IsOwner) GetInkImage().color = playerColour;
        SetColourSpritesEveryone_Rpc(GetComponent<NetworkObject>().NetworkObjectId, inputColour);

    }

    [Rpc(SendTo.Everyone, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    public void SetColourSpritesEveryone_Rpc(ulong playerId, Color inputColour)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out NetworkObject netObj))
        {
            transform.GetChild(0).GetComponent<MouseControl>().SetMouseColour(inputColour);
        }
    }



    Image GetInkImage()
    {
        foreach (var item in FindObjectsByType<Image>(FindObjectsSortMode.None)) 
        { 
            if (item.gameObject.tag == "InkCount") return item;
        }
        return FindFirstObjectByType<Image>();

    }
    TextMeshProUGUI GetInkText()
    {
        foreach (var item in FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))
        {
            if (item.gameObject.tag == "InkCount") return item;
        }
        return FindFirstObjectByType<TextMeshProUGUI>();

    }


    private void UpdateInkCounter()
    {
        if (!freeDrawActive)
        {
            if (inkCounter == null) inkCounter = GetInkText();
            if (currentInk < 0) currentInk = 0;
            inkCounter.text = Mathf.FloorToInt(currentInk).ToString();
        }

    }


    private void Update()
    {   
        if (freeDrawActive)
        {
            currentInk = maxInk;
        }
        if (IsClient && IsLocalPlayer)
        {
            Drawing();
        }

    }

    void Drawing()
    {
        if (currentInk > 0 || freeDrawActive)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                ToServerSetup(true);
            }
            else if (Input.GetKey(KeyCode.Mouse0))
            {
                ToServerSetup();
            }
            else if (freeDrawActive && (Input.GetKeyDown(KeyCode.Mouse1) || Input.GetKey(KeyCode.Mouse1)))
            {
                Erase();
            }
        }
        if (currentInk < maxInk)
        {
            currentInk += Time.deltaTime / 2 * inkMultiplier;
            UpdateInkCounter();
        }
        else
        {
            currentInk = maxInk;
            UpdateInkCounter();
        }

    }

    Vector3 GetControl()
    {
        return Input.mousePosition;
    }






    [Rpc(SendTo.Server, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    void ServerProcessing_Rpc(Vector2 mousePos, Vector2 lastPos, Color playerColour, int selectedColour, bool newItem = false, float width = 0.10f)
    {
        if (newItem) return;
        GameObject brushInstance = Instantiate(brush);
        NetworkObject brushNetObj = brushInstance.GetComponent<NetworkObject>();
        brushInstance.GetComponent<DataStorage>().selectedColour = selectedColour;
        brushNetObj.Spawn();
        ApplyLineVisuals_Rpc(brushNetObj.NetworkObjectId, mousePos, lastPos, playerColour, width);
    }


    [Rpc(SendTo.Everyone, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    void ApplyLineVisuals_Rpc(ulong playerId, Vector2 mousePos, Vector2 lastPos, Color colour, float width)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out NetworkObject netObj))
        {
            LineRenderer line = netObj.GetComponent<LineRenderer>();
            if (line != null)
            {
                line.SetPosition(0, lastPos);
                line.SetPosition(1, mousePos);
                line.startColor = colour;
                line.endColor = colour;
                line.widthMultiplier = width;
            }
            netObj.GetComponent<LineCollider>().enabled = true;
        }
    }





    void ToServerSetup(bool newItem = false, float width = 0.10f)
    {
        currentInk -= Time.deltaTime * 2 * inkMultiplier;
        UpdateInkCounter();
        if (currentInk > 0)
        {
            Vector2 mousePos = m_camera.ScreenToWorldPoint(GetControl());
            ServerProcessing_Rpc(mousePos, lastPos, playerColour, selectedColour, newItem, width);
            lastPos = mousePos;
        }


    }

    void Erase()
    {
        Ray ray = Camera.main.ScreenPointToRay(GetControl());
        RaycastHit hitData_for_the_ray;
        if (Physics.Raycast(ray, out hitData_for_the_ray))
        {
            //Debug.Log(hitData_for_the_ray);
            GameObject theGameObjectHitByRay = hitData_for_the_ray.collider.gameObject;
            if (theGameObjectHitByRay.tag == "Drawing")
            {
                NetworkObject netObj = theGameObjectHitByRay.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    //netObj.Despawn();
                    ServerErace_Rpc(netObj.NetworkObjectId);
                }
            }
            //Destroy(theGameObjectHitByRay);
        }

    }


    [Rpc(SendTo.Server, RequireOwnership = true, Delivery = RpcDelivery.Reliable)]
    void ServerErace_Rpc(ulong playerId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out NetworkObject netObj))
        {
            netObj.Despawn();
        }
    }



}
