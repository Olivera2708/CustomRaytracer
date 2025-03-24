using UnityEngine;

public struct LightObject
{
    // Represents a light source in the scene.
    //
    // Fields:
    //   type: The type of light (e.g., 0 = point, 1 = directional, 2 = spot).
    //   position: The position of the light in world space.
    //   direction: The direction the light is pointing (used for directional/spot lights).
    //   color: The RGBA color of the light.
    //   intensity: The brightness of the light.
    //   radius: The effective radius of the light (used for point/spot lights).

    public int type;
    public Vector3 position;
    public Vector3 direction;
    public Vector4 color;
    public float intensity;
    public float radius;
}

public struct Triangle
{
    // Represents a triangle in 3D space using its three vertex positions.
    //
    // Fields:
    //   v0: The first vertex of the triangle.
    //   v1: The second vertex of the triangle.
    //   v2: The third vertex of the triangle.

    public Vector3 v0, v1, v2;
}

public struct MaterialObject
{
    // Represents the material properties of a surface for shading.
    //
    // Fields:
    //   color: The base color (RGBA) of the material.
    //   shininess: The smoothness or glossiness of the surface (used in specular highlights).
    //   specularColor: The color of the specular reflection.

    public Vector4 color;
    public float shininess;
    public Vector4 specularColor;
}