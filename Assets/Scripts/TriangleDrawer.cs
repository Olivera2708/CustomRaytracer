using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

[ExecuteAlways]
public class TriangleDrawer : MonoBehaviour
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
    
    private void RayTrace()
    {
        Vector3 startPoint = new Vector3(-_screenWidth/2, -_screenHeight/2, _cam.nearClipPlane); //bottom left point (0,0) in our 2D grid
        for (float x = 0; x < _screenWidth; x += 0.3f)
        {
            for (float y = 0; y < _screenHeight; y += 0.3f)
            {
                Vector3 point = startPoint + new Vector3(x, y, 0);
            }
        }
    }

    private void SetCamera()
    {
        _cam = Camera.main;
        _transformCam = _cam.transform;
        _screenHeight = 2.0f * _cam.nearClipPlane * Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad); //nearClipPlane - distance from camera to screen
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