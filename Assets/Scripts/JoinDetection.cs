using NUnit.Framework;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Netcode.Editor;
using UnityEngine;

public class JoinDetection : NetworkBehaviour
{
    private List<Draw> players = new List<Draw>();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        base.OnNetworkSpawn();
        
    }

    void Update()
    {
        if (IsServer)
        {
            //players = GameManager.instance.drawList;
            Debug.Log("Test");
            Draw[] draws = GameObject.FindObjectsByType<Draw>(FindObjectsSortMode.None);
            Debug.Log("players count: "+players.Count);
            Debug.Log("draws length: "+draws.Length);

               
            if (players.Count < draws.Length)
            {
                Debug.Log("Count Changed");
                players = GameManager.instance.drawList;
                foreach (NetworkObject networkObject in GameObject.FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
                {
                    try
                    {
                        networkObject.Spawn();
                    }
                    finally
                    {
                        LineRenderer lineRenderer = networkObject.GetComponent<LineRenderer>();
                        Debug.Log("Run Spawn");
                        if (lineRenderer != null)
                        {
                            Debug.Log("Set Data");
                            SetDrawData_Rpc(networkObject.NetworkObjectId, lineRenderer.GetPosition(1), lineRenderer.GetPosition(0), lineRenderer.startColor, lineRenderer.widthMultiplier);
                        }
                    }

                    
                }
            }
        }
    }


    

    [Rpc(SendTo.Everyone, RequireOwnership = false, Delivery = RpcDelivery.Reliable)]
    void SetDrawData_Rpc(ulong playerId, Vector2 pos1, Vector2 pos0, Color color, float width)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerId, out NetworkObject netObj))
        {
            LineRenderer line = netObj.GetComponent<LineRenderer>();
            if (line != null)
            {
                line.SetPosition(0, pos0);
                line.SetPosition(1, pos1);
                line.startColor = color;
                line.endColor = color;
                line.widthMultiplier = width;
            }
            netObj.GetComponent<LineCollider>().enabled = true;
        }
        //return;
    }


}
