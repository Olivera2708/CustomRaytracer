using System.Collections.Generic;
using UnityEngine;

public class RayTracingManager : MonoBehaviour
{
    public ComputeShader rayTracingShader;
    public Material displayMaterial;
    private Camera camera;
    
    private ComputeBuffer vertexBuffer;
    private ComputeBuffer indexBuffer;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> indices = new List<int>();
    private int[] hitResults;
    private int triangleCount;
    
    private ComputeBuffer frustumBuffer;
    private Vector3[] frustumCorners = new Vector3[8];
    private RenderTexture renderTexture;

    void Start()
    {
        camera = Camera.main;
        GetSceneObjects();
        InitComputeBuffers();
        InitRenderTexture();
    }

    void GetSceneObjects()
    {
        vertices.Clear();
        indices.Clear();

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
                indices.Add(index + vertexOffset);
        }
    }

    void InitRenderTexture()
    {
        if (renderTexture != null) renderTexture.Release();

        renderTexture = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBFloat);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        rayTracingShader.SetTexture(0, "colorBuffer", renderTexture);
        displayMaterial.SetTexture("_MainTex", renderTexture);
    }

    void InitComputeBuffers()
    {
        if (vertexBuffer != null) vertexBuffer.Release();
        if (indexBuffer != null) indexBuffer.Release();
    
        vertexBuffer = new ComputeBuffer(vertices.Count, sizeof(float) * 3);
        indexBuffer = new ComputeBuffer(indices.Count, sizeof(int));
    
        int totalPixels = Screen.width * Screen.height;
        triangleCount = indices.Count / 3;
    
        vertexBuffer.SetData(vertices);
        indexBuffer.SetData(indices);
        hitResults = new int[totalPixels];
        
        if (frustumBuffer != null) frustumBuffer.Release();
    
        // just help for frustum
        frustumBuffer = new ComputeBuffer(8, sizeof(float) * 3);
        rayTracingShader.SetBuffer(0, "frustumBuffer", frustumBuffer);
    }
    
    void DebugFrustum()
    {
        Vector3 camPos = camera.transform.position;
        
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
        
        rayTracingShader.SetVector("cameraPosition", camera.transform.position);
        rayTracingShader.SetVector("cameraRight", camera.transform.right);
        rayTracingShader.SetVector("cameraUp", camera.transform.up);
        rayTracingShader.SetVector("cameraForward", camera.transform.forward);
        rayTracingShader.SetFloat("fov", camera.fieldOfView);
        rayTracingShader.SetFloat("aspectRatio", (float)Screen.width / Screen.height);
        rayTracingShader.SetFloat("nearPlane", camera.nearClipPlane);
        rayTracingShader.SetFloat("farPlane", camera.farClipPlane);

        int threadGroupsX = Mathf.CeilToInt(Screen.width / 16.0f);
        int threadGroupsY = Mathf.CeilToInt(Screen.height / 16.0f);
        rayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);

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
    }
}
