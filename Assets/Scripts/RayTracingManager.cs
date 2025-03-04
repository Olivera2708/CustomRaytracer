using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

[ExecuteAlways]
public class RayTracingManager : MonoBehaviour
{
    private Camera _cam;
    private Transform _transformCam;
    private float _screenHeight;
    private float _screenWidth;
    [SerializeField] private Material customShader;
    private List<SceneObject> sceneData = new List<SceneObject>();

    private void DrawTriangles()
    {
        foreach (SceneObject obj in this.sceneData)
        {
            foreach (var triangle in obj.triangles)
            {
                Debug.DrawRay(triangle.v0, triangle.v1 - triangle.v0, Color.black);
                Debug.DrawRay(triangle.v1, triangle.v2 - triangle.v1, Color.black);
                Debug.DrawRay(triangle.v2, triangle.v0 - triangle.v2, Color.black);
            }
        }
    }

    private void GetSceneObjects()
    {
        sceneData.Clear();
        MeshRenderer[] meshes = FindObjectsOfType<MeshRenderer>();
        foreach (var mesh in meshes)
        {
            MeshFilter meshFilter = mesh.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.sharedMesh != null)
                sceneData.Add(new SceneObject(mesh.transform, meshFilter.sharedMesh));
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, customShader);
    }
    
    private void DrawArrow(Vector3 from, Vector3 to)
    {
        Debug.DrawRay(from ,to, Color.black);
        Vector3 dir = to - from;
        
        Vector3 right = Quaternion.Euler(0, 30, 0) * -dir;
        Vector3 left = Quaternion.Euler(0, -30, 0) * -dir;
        
        Debug.DrawRay(to + from, right * 0.01f, Color.black);
        Debug.DrawRay(to + from, left * 0.01f, Color.black);
    }
    
    private void RayTrace()
    {
        Vector3 startPoint = new Vector3(-_screenWidth/2, -_screenHeight/2, _cam.farClipPlane);
        float x_part = _screenWidth / Screen.width;
        float y_part = _screenHeight / Screen.height;
        for (float x = 0; x < _screenWidth; x += x_part)
        {
            for (float y = 0; y < _screenHeight; y += y_part)
            {
                Vector3 point = startPoint + new Vector3(x, y, 0);
                // DrawArrow(_transformCam.position, _transformCam.rotation * point);
            }
        }
    }

    private void SetCamera()
    {
        _cam = Camera.main;
        _transformCam = _cam.transform;
        _screenHeight = 2.0f * _cam.farClipPlane * Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        _screenWidth = _screenHeight * _cam.aspect;
    }

    private void UpdateCamera()
    {
        _transformCam = _cam.transform;
    }
    
    void Start()
    {
        SetCamera();
        GetSceneObjects();
    }

    void Update()
    {
        UpdateCamera();
        RayTrace();
        DrawTriangles();
    }
}
