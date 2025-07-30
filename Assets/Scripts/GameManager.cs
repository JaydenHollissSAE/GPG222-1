using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<FixedString128Bytes> colours = null;
    private string localColours;
    public List<Draw> drawList = new List<Draw>();
    public Draw localDraw;
    public List<int> coloursList;
    public List<Color> drawingColours = new List<Color>();
    public static GameManager instance;
    public bool freeDrawActive = false;
    public bool endlessActive = false;
    [SerializeField] GameObject colourSelection;

    public float maxInk = 100f;

    //public NetworkVariable<GameManager> networkInstance = null;


    private void Awake()
    {
        instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //if (networkInstance == null) networkInstance.Value = this;
        if (instance == null) instance = this;
        if (colours == null)
        {
            colours = new NetworkVariable<FixedString128Bytes>();
            //colours.Value = "";
        }
        //Debug.Log(colours.Value);

    }


    private void Update()
    {
        if (!freeDrawActive)
        {
            if (localColours != colours.Value.ToString())
            {
                //Debug.Log(colours.Value);
                localColours = colours.Value.ToString();
                //Debug.Log(localColours);
                coloursList = new List<int>();
                string[] tmpColours = localColours.Split('|');
                for (int i = 0; i < tmpColours.Length; i++)
                {
                    //Debug.Log(tmpColours[i]);
                    coloursList.Add(int.Parse(tmpColours[i]));
                }
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                colourSelection.SetActive(!colourSelection.activeSelf);
            }
        }

    }

    public void NewList()
    {
        colours.Value = string.Join("|", coloursList);
        localColours = colours.Value.ToString();
    }



}
