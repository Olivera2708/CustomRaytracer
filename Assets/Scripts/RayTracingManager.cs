using System;
using System.Collections.Generic;
using UnityEngine;

public class RayTracingManager : MonoBehaviour
{
    public ComputeShader rayTracingShader;
    public Material displayMaterial;
    private Camera rayTracingCamera;
    
    private ComputeBuffer vertexBuffer;
    private ComputeBuffer indexBuffer;
    private ComputeBuffer materialBuffer;
    private ComputeBuffer materialIndexBuffer;
    private ComputeBuffer bvhBuffer;
    private RenderTexture accumTexture;
    
    private List<int> materialIndices = new List<int>();
    private List<Vector3> materialColors = new List<Vector3>();
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> indices = new List<int>();
    private int[] hitResults;
    private int triangleCount;
    
    private ComputeBuffer frustumBuffer;
    private Vector3[] frustumCorners = new Vector3[8];
    private RenderTexture renderTexture;
    private BVH bvh;

    private int sampleCount = 0;

    void Start()
    {
        rayTracingCamera = Camera.main;
        GetSceneObjects();
        GetMaterials();
        
        bvh = new BVH(vertices, indices);
        
        InitComputeBuffers();
        InitRenderTexture();
    }

    void GetMaterials()
    {
        materialColors.Clear();
        MeshRenderer[] meshes = FindObjectsOfType<MeshRenderer>();

        foreach (var mesh in meshes)
        {
            Material mat = mesh.sharedMaterial;
            Color color = mat.color.linear;
            materialColors.Add(new Vector3(color.r, color.g, color.b));
        }
    }

    void GetSceneObjects()
    {
        vertices.Clear();
        indices.Clear();
        materialIndices.Clear();
        int objectIndex = 0;

        MeshRenderer[] meshes = FindObjectsOfType<MeshRenderer>();
        foreach (var mesh in meshes)
        {
            MeshFilter meshFilter = mesh.GetComponent<MeshFilter>();
            if (meshFilter == null || meshFilter.sharedMesh == null) continue;

            Mesh meshData = meshFilter.sharedMesh;
            int vertexOffset = vertices.Count;

            foreach (Vector3 vertex in meshData.vertices)
                vertices.Add(mesh.transform.TransformPoint(vertex));

            foreach (int index in meshData.triangles)
            {
                indices.Add(index + vertexOffset);
                materialIndices.Add(objectIndex);
            }

            objectIndex++;
        }
    }

    void InitRenderTexture()
    {
        if (renderTexture != null) renderTexture.Release();
        if (accumTexture != null) accumTexture.Release();

        renderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();
        
        accumTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
        accumTexture.enableRandomWrite = true;
        accumTexture.Create();

        rayTracingShader.SetTexture(0, "colorBuffer", renderTexture);
        rayTracingShader.SetTexture(0, "accumBuffer", accumTexture);
        displayMaterial.SetTexture("_MainTex", renderTexture);
    }

    void InitComputeBuffers()
    {
        if (vertexBuffer != null) vertexBuffer.Release();
        if (indexBuffer != null) indexBuffer.Release();
        if (materialBuffer != null) materialBuffer.Release();
        if (materialIndexBuffer != null) materialIndexBuffer.Release();
        if (bvhBuffer != null) bvhBuffer.Release();
    
        vertexBuffer = new ComputeBuffer(vertices.Count, sizeof(float) * 3);
        indexBuffer = new ComputeBuffer(indices.Count, sizeof(int));
        materialBuffer = new ComputeBuffer(materialColors.Count, sizeof(float) * 3);
        materialIndexBuffer  = new ComputeBuffer(materialIndices.Count, sizeof(int));
        bvhBuffer = new ComputeBuffer(bvh.nodes.Count, sizeof(int) * 6 + sizeof(float) * 6);
    
        int totalPixels = Screen.width * Screen.height;
        triangleCount = indices.Count / 3;
    
        vertexBuffer.SetData(vertices);
        indexBuffer.SetData(indices);
        materialBuffer.SetData(materialColors);
        materialIndexBuffer.SetData(materialIndices);
        bvhBuffer.SetData(bvh.nodes);
        hitResults = new int[totalPixels];
        
        if (frustumBuffer != null) frustumBuffer.Release();
    
        // just help for frustum
        frustumBuffer = new ComputeBuffer(8, sizeof(float) * 3);
        rayTracingShader.SetBuffer(0, "frustumBuffer", frustumBuffer);
    }
    
    void DebugFrustum()
    {
        Vector3 camPos = rayTracingCamera.transform.position;
        
        // Far Plane (Green)
        Debug.DrawRay(camPos, frustumCorners[4] - camPos, Color.green); // Bottom Left
        Debug.DrawRay(camPos, frustumCorners[5] - camPos, Color.green); // Bottom Right
        Debug.DrawRay(camPos, frustumCorners[6] - camPos, Color.green); // Top Left
        Debug.DrawRay(camPos, frustumCorners[7] - camPos, Color.green); // Top Right
        
        // Near Plane (Red)
        Debug.DrawRay(camPos, frustumCorners[0] - camPos, Color.red); // Bottom Left
        Debug.DrawRay(camPos, frustumCorners[1] - camPos, Color.red); // Bottom Right
        Debug.DrawRay(camPos, frustumCorners[2] - camPos, Color.red); // Top Left
        Debug.DrawRay(camPos, frustumCorners[3] - camPos, Color.red); // Top Right
    }

    void DispatchComputeShader()
    {
        rayTracingShader.SetInt("width", Screen.width);
        rayTracingShader.SetInt("height", Screen.height);
        rayTracingShader.SetInt("triangleCount", triangleCount);
        rayTracingShader.SetBuffer(0, "vertices", vertexBuffer);
        rayTracingShader.SetBuffer(0, "indices", indexBuffer);
        rayTracingShader.SetBuffer(0, "materialColors", materialBuffer);
        rayTracingShader.SetBuffer(0, "materialIndices", materialIndexBuffer);
        rayTracingShader.SetBuffer(0, "bvhNodes", bvhBuffer);
        
        rayTracingShader.SetVector("cameraPosition", rayTracingCamera.transform.position);
        rayTracingShader.SetVector("cameraRight", rayTracingCamera.transform.right);
        rayTracingShader.SetVector("cameraUp", rayTracingCamera.transform.up);
        rayTracingShader.SetVector("cameraForward", rayTracingCamera.transform.forward);
        rayTracingShader.SetFloat("fov", rayTracingCamera.fieldOfView);
        rayTracingShader.SetFloat("aspectRatio", (float)Screen.width / Screen.height);
        rayTracingShader.SetFloat("nearPlane", rayTracingCamera.nearClipPlane);
        rayTracingShader.SetFloat("farPlane", rayTracingCamera.farClipPlane);
        
        rayTracingShader.SetInt("sampleCount", sampleCount);

        int threadGroupsX = Mathf.CeilToInt(Screen.width / 16.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 16.0f);
        rayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);
        
        sampleCount++;

        // frustumBuffer.GetData(frustumCorners);
        // DebugFrustum();
        // DebugHits();
    }

    void DebugHits()
    {
        int hitCount = 0;
        for (int i = 0; i < hitResults.Length; i++)
        {
            if (hitResults[i] == 1) hitCount++;
        }
        Debug.Log("Hit Pixels: " + hitCount);
    }

    void FixedUpdate()
    {
        if (sampleCount < 1000)
            DispatchComputeShader();
    }
    
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(renderTexture, dest, displayMaterial);
    }

    void OnDestroy()
    {
        if (vertexBuffer != null) vertexBuffer.Release();
        if (indexBuffer != null) indexBuffer.Release();
        if (renderTexture != null) renderTexture.Release();
        if (frustumBuffer != null) frustumBuffer.Release();
        if (materialBuffer != null) materialBuffer.Release();
        if (materialIndexBuffer != null) materialIndexBuffer.Release();
        if (bvhBuffer != null) bvhBuffer.Release();
    }
}
