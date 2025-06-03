using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineCollider : MonoBehaviour
{
    LineRenderer lineRenderer;
    private Draw draw;

    // Start is called before the first frame update
    void Start()
    {
        draw = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Draw>();
        GenerateMesh();
    }

    // Update is called once per frame
    void GenerateMesh()
    {
        lineRenderer = GetComponent<LineRenderer>();
        MeshCollider collider = GetComponent<MeshCollider>();
        if (collider == null )
        {
            collider = gameObject.AddComponent<MeshCollider>();
        }
        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);
        collider.sharedMesh = mesh;
        lineRenderer.startColor = draw.drawingColors[draw.prevColour];
        draw.prevColour++;
        if (draw.prevColour >= draw.drawingColors.Count)
        {
            draw.prevColour = 0;
        }
        lineRenderer.endColor = draw.drawingColors[draw.prevColour];
    }
}
