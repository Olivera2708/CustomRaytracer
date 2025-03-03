using UnityEngine;

public class CameraRays : MonoBehaviour
{
    private Camera _cam;
    private Transform _transformCam;
    private float _screenHeight;
    private float _screenWidth;

    private void DrawArrow(Vector3 from, Vector3 to)
    {
        Debug.DrawRay(from ,to, Color.black);
        Vector3 dir = to - from;
        
        Vector3 right = Quaternion.Euler(0, 30, 0) * -dir;
        Vector3 left = Quaternion.Euler(0, -30, 0) * -dir;
        
        Debug.DrawRay(to + from, right * 0.01f, Color.black);
        Debug.DrawRay(to + from, left * 0.01f, Color.black);
    }
    
    private void CameraPoints()
    {
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
    
    void Start()
    {
        _cam = Camera.main;
        _transformCam = _cam.transform;
        _screenHeight = 2.0f * _cam.nearClipPlane * Mathf.Tan(_cam.fieldOfView * 0.5f * Mathf.Deg2Rad); //nearClipPlane - distance from camera to screen
        _screenWidth = _screenHeight * _cam.aspect;
    }

    void Update()
    {
        _transformCam = _cam.transform;
        CameraPoints();
    }
}
