using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LineCollider : NetworkBehaviour
{
    LineRenderer lineRenderer;
    private Draw draw;

    /*// Start is called before the first frame update
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        //draw = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Draw>();
        GenerateMesh_Rpc();
    }*/


    //[Rpc(SendTo.ClientsAndHost, RequireOwnership = false)]

    private void Start()
    {
        GenerateMesh();
    }

    public void GenerateMesh()
    {
        lineRenderer = GetComponent<LineRenderer>();
        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<MeshCollider>();
        }
        lineRenderer.useWorldSpace = false;
        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);
        collider.sharedMesh = mesh;
        /*lineRenderer.startColor = draw.drawingColors[draw.prevColour];
        draw.prevColour++;
        if (draw.prevColour >= draw.drawingColors.Count)
        {
            draw.prevColour = 0;
        }
        lineRenderer.endColor = draw.drawingColors[draw.prevColour];*/
    }
}
