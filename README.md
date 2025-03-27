# Unity Ray Tracer

This project is a custom ray tracer implemented in Unity using compute shaders. It simulates the physics of light to render realistic scenes from scratch, featuring various lighting models, shading techniques, and performance optimizations such as spatial acceleration structures.

The ray tracer serves both as a renderer and as an educational tool, featuring visual debugging utilities to help understand the behavior of rays, meshes, and the camera.

---

## Features

### Shading Models and Materials

- **Phong Shading**: Classic shading model that includes ambient, diffuse, and specular components for smooth shading across surfaces.
- **Lambertian Shading**: Used for diffuse materials, producing realistic matte surfaces based on surface normals and light direction.
- **Support for smoothness and metallic parameters**: Allows nuanced material behavior based on physical values.

### Lighting and Shadows

- **Direct Lighting**: Calculates illumination from one or more direct light sources.
- **Hard Shadows**: Ray-based shadow determination with sharp, well-defined edges.
- **Soft Shadows**: Uses multiple shadow rays and area light approximation for natural-looking penumbras and gradient shadows.

### Ray Casting and Sampling

- **Möller–Trumbore Triangle Intersection**: Efficient ray-triangle intersection algorithm used for determining ray-object collisions.
- **Multiple Rays per Pixel (Supersampling)**: Anti-aliasing technique that casts several rays per pixel to produce smoother edges and reduce noise.

### Performance and Acceleration

- **Bounding Volume Hierarchy (BVH)**: Spatial data structure used to reduce the number of intersection tests, greatly improving rendering performance for complex scenes.
- **Efficient memory layout for compute shader compatibility**: Scene data is structured to optimize GPU traversal.

### Debugging and Visualization Tools (Bonus)

- **Camera Frustum Visualizer**: Renders the frustum of the virtual camera in the scene view to help visualize the ray generation volume.
- **Ray Visualizer**: Draws rays cast from the camera help debug sampling and intersections.
- **Triangle Mesh Visualizer**: Displays the individual triangles of scene objects to verify geometry and intersection accuracy.

---

## How to Use

1. Clone or download the project.
2. Open the project using Unity Hub or directly in the Unity Editor.
3. Load the main scene.
4. Enter Play Mode to start rendering.
