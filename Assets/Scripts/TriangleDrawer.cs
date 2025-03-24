using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

[ExecuteAlways]
public class TriangleDrawer : MonoBehaviour
{
    // A MonoBehaviour that collects mesh data from the scene, extracts triangle geometry,
    // and visualizes it using Debug rays. Also applies a custom shader via OnRenderImage.
    //
    // This script runs both in Play mode and Edit mode (due to [ExecuteAlways]).
    //
    // Fields:
    //   _cam: Reference to the main camera.
    //   _transformCam: Cached transform of the main camera.
    //   _screenHeight: Height of the virtual screen at the near clipping plane.
    //   _screenWidth: Width of the virtual screen at the near clipping plane.
    //   customShader: Material containing the custom image effect shader.
    //   sceneData: List of SceneObjects representing geometry extracted from scene meshes.

    private Camera _cam;
    private Transform _transformCam;
    private float _screenHeight;
    private float _screenWidth;
    [SerializeField] private Material customShader;
    private List<SceneObject> sceneData = new List<SceneObject>();

    private void DrawTriangles()
    {
        // Draws all extracted triangles from scene meshes using Debug rays.
        //
        // This method loops through all triangles in `sceneData` and visualizes
        // their edges by drawing rays between the triangle's vertices.

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
        // Collects all mesh renderers in the scene and extracts their geometry.
        //
        // This method finds all MeshRenderer components, retrieves their associated
        // meshes, and creates corresponding SceneObjects for triangle extraction.

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
        // Applies a custom image effect shader using Graphics.Blit.
        //
        // Args:
        //   source: The source render texture from the camera.
        //   destination: The destination render texture where the result is drawn.
        //
        // This method allows the use of a full-screen shader for post-processing.

        Graphics.Blit(source, destination, customShader);
    }
    
    private void RayTrace()
    {
        // Iterates over a virtual screen grid in front of the camera for ray tracing.
        //
        // This method sets up a 2D grid in camera space and prepares points for future
        // ray tracing operations. Currently it only computes the points, without casting rays.

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
        // Initializes the camera and calculates screen dimensions at the near clipping plane.
        //
        // This method caches the main camera and its transform, and computes the virtual
        // screen size in world units at the near plane based on FOV and aspect ratio.

        _cam = Camera.main;
        _transformCam = _cam.transform;
        _screenHeight = 2.0f * _cam.nearClipPlane * Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad); //nearClipPlane - distance from camera to screen
        _screenWidth = _screenHeight * _cam.aspect;
    }

    private void UpdateCamera()
    {
        // Updates the cached camera transform reference.
        //
        // Ensures that the camera's current position and orientation are tracked each frame.

        _transformCam = _cam.transform;
    }
    
    void Start()
    {
        // Unity Start method. Called before the first frame update.
        //
        // Sets up camera information and gathers mesh data from the scene.

        SetCamera();
        GetSceneObjects();
    }

    void Update()
    {
        // Unity Update method. Called once per frame.
        //
        // Updates the camera state, traces a grid of ray directions, and visualizes triangle edges.

        UpdateCamera();
        RayTrace();
        DrawTriangles();
    }
}