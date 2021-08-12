using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class TexDrawPlane : MonoBehaviour
{
    [SerializeField] 
    private Camera targetCamera;

    private Mesh mesh;
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;

    private Vector3[] outCorners = new Vector3[4];
    
    void Start()
    {
        var filter = GetComponent<MeshFilter>();

        mesh = new Mesh();
        filter.mesh = mesh;

        vertices = new Vector3[4];
        triangles = new[] {0, 1, 2, 2, 1, 3};
        uvs = new[] {new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f)};

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
    }

    void Update()
    {
        transform.localPosition = Vector3.zero;
        transform.rotation = Quaternion.identity;

        targetCamera.CalculateFrustumCorners(new Rect(0, 0, 1, 1), targetCamera.farClipPlane - 0.1f, Camera.MonoOrStereoscopicEye.Mono, outCorners);
        var cameraTransform = targetCamera.transform;
        
        vertices[0] = cameraTransform.TransformVector(outCorners[3]);
        vertices[1] = cameraTransform.TransformVector(outCorners[0]);
        vertices[2] = cameraTransform.TransformVector(outCorners[2]);
        vertices[3] = cameraTransform.TransformVector(outCorners[1]);

        mesh.vertices = vertices;
        mesh.RecalculateBounds();
    }

}
