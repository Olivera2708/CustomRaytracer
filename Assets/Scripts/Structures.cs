using UnityEngine;

public struct LightObject
{
    public int type;
    public Vector3 position;
    public Vector3 direction;
    public Vector4 color;
    public float intensity;
    public float radius;
}

public struct Triangle
{
    public Vector3 v0, v1, v2;
}

public struct MaterialObject
{
    public Vector4 color;
    public float shininess;
    public Vector4 specularColor;
}