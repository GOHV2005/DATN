using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    EdgeCollider2D TerrainCollider;
    MeshFilter TerrainMeshFilter;
    float horCount = 500, horStep = 0.0125f, vertStep = 0.3f, noiseAmpl = 0.25f, noiseFreq = 3f, colliderMargin = -0.02f, impactWidth = 0.1f, impactDepth = 0.32f, initialImpactWidth, initialImpactDepth;
    bool isDragableObject = false;
    Transform dragableObject = null;
    Vector3 startMousePos = new Vector3();
    public Slider widthSlider, depthSlider;

    void Start()
    {
        TerrainCollider = gameObject.GetComponent<EdgeCollider2D>();
        TerrainMeshFilter = gameObject.GetComponent<MeshFilter>();
        initialImpactWidth = impactWidth;
        initialImpactDepth = impactDepth;
        BuildTestTerrain();
        BuildTerrainCollider();
    }

    private void BuildTestTerrain()
    {
        var mesh = new Mesh();
        var verticies = new List<Vector3>();
        var UVs = new List<Vector2>();

        var length = 2 * (horCount + 1);
        for (int i = 0; i < length; i++)
        {
            var x = (i / 2) * horStep;
            var y = (i % 2) * vertStep;
            y *= 1 + noiseAmpl * (Mathf.PerlinNoise(x * noiseFreq, 0) + Mathf.PerlinNoise(x * 2 * noiseFreq, 0) / 2 + Mathf.PerlinNoise(x * 4 * noiseFreq, 0) / 4);
            verticies.Add(new Vector3(x, y));
            UVs.Add(new Vector2(x / (horStep * horCount) , i % 2 == 0 ? 0 : 1));
        }
        var triangles = new List<int>();
        for (int i = 0; i < verticies.Count - 2; i += 2)
        {
            triangles.Add(i);
            triangles.Add(i + 1);
            triangles.Add(i + 3);
            triangles.Add(i);
            triangles.Add(i + 3);
            triangles.Add(i + 2);
        }
        mesh.vertices = verticies.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = UVs.ToArray();
        TerrainMeshFilter.sharedMesh = mesh;
    }

    void BuildTerrainCollider()
    {
        var verticies = TerrainMeshFilter.sharedMesh.vertices;
        var points = new List<Vector2>();
        points.Add(verticies[0]);
        for (int i = 1; i < verticies.Length; i += 2)
        {
            var p = verticies[i];
            p.y += colliderMargin;
            points.Add(p);
        }
        points.Add(verticies[verticies.Length - 2]);
        TerrainCollider.points = points.ToArray();
    }

    public void DoImpact(float x, float force)
    {
        var verticies = TerrainMeshFilter.sharedMesh.vertices;
        for (int i = 1; i < verticies.Length; i += 2)
        {
            var dist = (verticies[i].x - x) / impactWidth;
            var gauss = GaussApproximation(dist) * impactDepth;
            verticies[i].y -= force * gauss;
        }
        TerrainMeshFilter.sharedMesh.vertices = verticies;
        BuildTerrainCollider();
    }

    float GaussApproximation(float x)
    {
        return Mathf.Cos(x) / Mathf.Exp(1 + Mathf.Pow(x / 1.5f, 2));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        var contact = collision.contacts[0];
        var velocityY = collision.relativeVelocity.y;
        if (velocityY > 0) return;
        DoImpact(contact.point.x, -velocityY / 10);
    }

    // Update is called once per frame
    void Update()
    {
        #region Raycast
        if (Input.GetMouseButtonDown(0))
        {
            {
                RaycastHit2D rayHit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
                if (rayHit.transform != null)
                {
                    if (rayHit.transform.GetComponent<PolygonCollider2D>() != null)
                    {
                        isDragableObject = true;
                        dragableObject = rayHit.transform;
                        startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    }
                    else
                    {
                        isDragableObject = false;
                        dragableObject = null;
                    }
                }
            }
        }
        if (Input.GetMouseButton(0) && isDragableObject)
        {
            dragableObject.position += new Vector3(Camera.main.ScreenToWorldPoint(Input.mousePosition).x - startMousePos.x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y - startMousePos.y, /*dragableObject.position.z*/0);
            dragableObject.GetComponent<Rigidbody2D>().gravityScale = 0;
            startMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
        if (Input.GetMouseButtonUp(0) && dragableObject!=null)
        {
            dragableObject.GetComponent<Rigidbody2D>().gravityScale = 1;
            isDragableObject = false;
            dragableObject = null;
        }
        #endregion
    }

    public void ChangeImpactDepth()
    {
        impactDepth = initialImpactDepth * depthSlider.value;
        
    }
    public void ChangeImpactWidth()
    {
        impactWidth = initialImpactWidth * widthSlider.value;
    }
}
