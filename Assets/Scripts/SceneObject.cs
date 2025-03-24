using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class SceneObject
    {
        // Represents a 3D object in a scene with transform data and extracted triangle geometry.
        //
        // Fields:
        //   position: The world position of the object.
        //   rotation: The world rotation of the object.
        //   scale: The world scale (lossy scale) of the object.
        //   mesh: The Mesh associated with the object.
        //   triangles: A list of world-space triangles extracted from the mesh.

        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Mesh mesh;
        public List<Triangle> triangles = new List<Triangle>();

        public SceneObject(Transform transform, Mesh mesh)
        {
            // Initializes a new SceneObject using a Transform and associated Mesh.
            //
            // Args:
            //   transform: The Transform from which to extract position, rotation, and scale.
            //   mesh: The Mesh to associate with this object.
            //
            // If the mesh is not null, its triangles are immediately extracted and transformed
            // into world space.

            this.position = transform.position;
            this.rotation = transform.rotation;
            this.scale = transform.lossyScale;
            this.mesh = mesh;

            if (mesh != null)
                ExtractTriangles(mesh);
        }

        public void ExtractTriangles(Mesh mesh)
        {
            // Extracts triangles from the given mesh and transforms them into world space.
            //
            // Args:
            //   mesh: The Mesh from which to extract triangle vertex data.
            //
            // For each triangle in the mesh, this method computes its world-space vertex positions
            // using the object's transform and stores the result in the triangles list.

            Vector3[] vertices = mesh.vertices;
            int[] triangle = mesh.triangles;

            for (int i = 0; i < triangle.Length; i += 3)
            {
                Vector3 v0 = TransformToWorld(vertices[triangle[i]]);
                Vector3 v1 = TransformToWorld(vertices[triangle[i + 1]]);
                Vector3 v2 = TransformToWorld(vertices[triangle[i + 2]]);
                
                triangles.Add(new Triangle{v0=v0, v1=v1, v2=v2});
            }
        }
        
        private Vector3 TransformToWorld(Vector3 localPoint)
        {
            // Transforms a local-space point to world-space using the object's transform.
            //
            // Args:
            //   localPoint: A point in the local space of the object.
            //
            // Returns:
            //   The corresponding point in world space.
            //
            // The transformation applies scale, rotation, and then translation.

            return position + rotation * Vector3.Scale(localPoint, scale);
        }
    }
}