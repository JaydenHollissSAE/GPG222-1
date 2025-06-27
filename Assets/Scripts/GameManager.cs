using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<FixedString128Bytes> colours = null;
    private string localColours;
    public List<int> coloursList;
    public List<Color> drawingColours = new List<Color>();
    public static GameManager instance;
    //public NetworkVariable<GameManager> networkInstance = null;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //if (networkInstance == null) networkInstance.Value = this;
        instance = this;
        if (colours == null)
        {
            colours = new NetworkVariable<FixedString128Bytes>();
            //colours.Value = "";
        } 
        //Debug.Log(colours.Value);
           
    }


    private void Update()
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


}
