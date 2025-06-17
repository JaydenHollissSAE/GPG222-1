using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Liner : NetworkBehaviour
{
    LineRenderer lr;
    Color[] colors = { Color.red, Color.black };
    int curColor = 0;
    public void SetPos()
    {
        base.OnNetworkSpawn();
        lr = GetComponent<LineRenderer>();
        //lr.material = new Material(Shader.Find("Sprites/Default"));
        //lr.material.color = colors[curColor];
        lr.startWidth = 1f;
        lr.endWidth = 1f;
        lr.positionCount = 2;

        Vector3[] poses = new Vector3[2];
        poses[0] = new Vector3(0, 10f, 100f);
        poses[1] = new Vector3(5f, -3f, -4f);
        lr.SetPositions(poses);


        // making a meshcolllider and attach to the lineRenderer

        //beware this makes lineRenderer using local space
        //lr.useWorldSpace = false;

        MeshCollider meshCollider = this.gameObject.AddComponent<MeshCollider>();
        Mesh mesh = new Mesh();
        lr.BakeMesh(mesh, false);
        meshCollider.sharedMesh = mesh;


    }
    
}