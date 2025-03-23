using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class SceneObject
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Mesh mesh;
        public List<Triangle> triangles = new List<Triangle>();

        public SceneObject(Transform transform, Mesh mesh)
        {
            this.position = transform.position;
            this.rotation = transform.rotation;
            this.scale = transform.lossyScale;
            this.mesh = mesh;

            if (mesh != null)
                ExtractTriangles(mesh);
        }

        public void ExtractTriangles(Mesh mesh)
        {
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
            return position + rotation * Vector3.Scale(localPoint, scale);
        }
    }
}