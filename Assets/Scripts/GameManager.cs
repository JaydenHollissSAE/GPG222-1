using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<int> colours = null;
    private int localColours;
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
            colours = new NetworkVariable<int>();
            colours.Value = 0;
        } 
           
    }


    private void Update()
    {
        if (localColours != colours.Value)
        {
            localColours = colours.Value;
            coloursList = new List<int>();
            foreach (string colourId in localColours.ToString().Split("")) 
            {
                coloursList.Add(int.Parse(colourId));
            }
        }
    }


}
