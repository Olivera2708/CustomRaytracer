using UnityEngine;

public class CameraRays : MonoBehaviour
{
    // A MonoBehaviour that visualizes camera rays projected through a virtual screen grid.
    //
    // This class calculates a 2D grid in camera space at the near clipping plane and visualizes
    // rays from the camera through each point using Debug.DrawRay. Useful for debugging ray 
    // directions or understanding camera projection.
    //
    // Fields:
    //   _cam: Reference to the main camera.
    //   _transformCam: Cached transform of the main camera.
    //   _screenHeight: Height of the virtual screen at the near clipping plane.
    //   _screenWidth: Width of the virtual screen at the near clipping plane.

    private Camera _cam;
    private Transform _transformCam;
    private float _screenHeight;
    private float _screenWidth;

    private void DrawArrow(Vector3 from, Vector3 to)
    {
        // Draws a debug ray with a small arrowhead from a starting point to a target point.
        //
        // Args:
        //   from: The origin of the ray.
        //   to: The target point to which the ray points.
        //
        // This function uses `Debug.DrawRay` to draw the main ray and additional lines
        // to create a simple arrowhead for visual aid.

        Debug.DrawRay(from ,to, Color.black);
        Vector3 dir = to - from;
        
        Vector3 right = Quaternion.Euler(0, 30, 0) * -dir;
        Vector3 left = Quaternion.Euler(0, -30, 0) * -dir;
        
        Debug.DrawRay(to + from, right * 0.01f, Color.black);
        Debug.DrawRay(to + from, left * 0.01f, Color.black);
    }
    
    private void CameraPoints()
    {
        // Computes and visualizes a grid of rays from the camera through its screen.
        //
        // This method calculates a grid of points across the near clipping plane in camera space,
        // transforms them into world space, and draws rays from the camera to each of these points.

        Vector3 startPoint = new Vector3(-_screenWidth/2, -_screenHeight/2, _cam.nearClipPlane); //bottom left point (0,0) in our 2D grid
        for (float x = 0; x < _screenWidth; x += 0.3f)
        {
            for (float y = 0; y < _screenHeight; y += 0.3f)
            {
                Vector3 point = startPoint + new Vector3(x, y, 0);
                DrawArrow(_transformCam.position, _transformCam.rotation * point);
            }
        }
    }

    private void SetCamera()
    {
        // Initializes the camera and calculates screen dimensions at the near clipping plane.
        //
        // This method retrieves the main camera, caches its transform, and calculates the
        // screen width and height at the near clip plane based on field of view and aspect ratio.

        _cam = Camera.main;
        _transformCam = _cam.transform;
        _screenHeight = 2.0f * _cam.nearClipPlane * Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad); //nearClipPlane - distance from camera to screen
        _screenWidth = _screenHeight * _cam.aspect;
    }

    private void UpdateCamera()
    {
        // Updates the cached camera transform reference.
        //
        // This ensures the transform is current in case the camera moves or changes dynamically.

        _transformCam = _cam.transform;
    }
    
    void Start()
    {
        // Unity Start method. Called before the first frame update.
        //
        // Initializes the camera setup and calculates screen dimensions.

        SetCamera();
    }

    void Update()
    {
        // Unity Update method. Called once per frame.
        //
        // Updates the camera transform and visualizes the debug rays from the camera through the screen grid.

        UpdateCamera();
        CameraPoints();
    }
}
