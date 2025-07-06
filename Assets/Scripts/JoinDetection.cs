using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Editor;
using UnityEngine;

public class JoinDetection : NetworkBehaviour
{
    private Draw[] playersOld;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        base.OnNetworkSpawn();
        playersOld = GameObject.FindObjectsByType<Draw>(FindObjectsSortMode.None);


    }

    void Update()
    {
        if (IsServer)
        {

            Debug.Log(playersOld.Length);
            Draw[] players = GameObject.FindObjectsByType<Draw>(FindObjectsSortMode.None);
            if (playersOld.Length == 0 || playersOld.Length > players.Length) playersOld = players;

            if (playersOld.Length < players.Length)
            {
                playersOld = players;
                //foreach (NetworkObject networkObject in GameObject.FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
                foreach (LineRenderer networkObject in GameObject.FindObjectsByType<LineRenderer>(FindObjectsSortMode.None))
                {
                    try
                    {
                        networkObject.GetComponent<NetworkObject>().Spawn();
                    }
                    finally
                    {
                        try
                        {
                            LineRenderer lineRenderer = networkObject.GetComponent<LineRenderer>();
                            if (lineRenderer != null)
                            {
                                SetDrawData_Rpc(networkObject.GetComponent<NetworkObject>().NetworkObjectId, lineRenderer.GetPosition(1), lineRenderer.GetPosition(0), lineRenderer.startColor, lineRenderer.widthMultiplier);
                            }
                        }
                        finally { }

                    }


                }
            }
            

        }
    }


    

    [Rpc(SendTo.Everyone, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    void SetDrawData_Rpc(ulong playerId, Vector2 pos1, Vector2 pos0, Color colour, float width)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out NetworkObject netObj))
        {
            LineRenderer line = netObj.GetComponent<LineRenderer>();
            if (line != null)
            {
                line.SetPosition(0, pos0);
                line.SetPosition(1, pos1);
                line.startColor = colour;
                line.endColor = colour;
                line.widthMultiplier = width;
            }
            netObj.GetComponent<LineCollider>().enabled = true;
        }
        //return;
    }


}
