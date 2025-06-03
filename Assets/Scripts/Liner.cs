using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Liner : MonoBehaviour
{
    LineRenderer lr;
    Color[] colors = { Color.red, Color.black };
    int curColor = 0;
    void Start()
    {
        lr = this.gameObject.GetComponent<LineRenderer>();
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
    void Update()
    {
        // when you click on the line, the line color should change between red and black

        // be aware that I am using the new input system
        // if using the old input system, i think you replace the the next line with "if(Input.GetMouseButtonDown(0))"
        if (Input.GetMouseButtonDown(1))
        {
            // if using the old input system, i think you replace the the next line with "Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);"
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitData_for_the_ray;
            if (Physics.Raycast(ray, out hitData_for_the_ray))
            {
                Debug.Log(hitData_for_the_ray);
                GameObject theGameObjectHitByRay = hitData_for_the_ray.collider.gameObject;
                Destroy(theGameObjectHitByRay);
            }
        }
    }
}