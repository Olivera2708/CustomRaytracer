using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public struct BVHNode
{
    public Vector3 minBounds;
    public Vector3 maxBounds;
    public int leftChild;
    public int rightChild;
    public int firstTriangle;
    public int triangleCount;
    public int parent;
    public int escapeIndex;
}

public class BVH
{
    public List<BVHNode> nodes = new List<BVHNode>();
    private List<int> triangles;
    private List<Vector3> vertices;
    
    public BVH(List<Vector3> vertices, List<int> indices)
    {
        this.vertices = vertices;
        this.triangles = new List<int>(indices);
        Build(0, triangles.Count / 3);
    }
    
    private int Build(int start, int count, int parent = -1)
    {
        BVHNode node = new BVHNode();
        node.firstTriangle = start;
        node.triangleCount = count;
        node.parent = parent;
        node.escapeIndex = -1;

        ComputeBounds(ref node, vertices, triangles);
        int nodeIndex = nodes.Count;
        nodes.Add(node);

        if (count <= 4)
            return nodeIndex;

        int splitAxis = ChooseSplitAxis(node);
        int mid = PartitionTrianglesMedian(vertices, triangles, start, count, splitAxis);
        
        int leftChild = Build(start, mid - start, nodeIndex);
        int rightChild = Build(mid, count - (mid - start), nodeIndex);

        BVHNode updatedNode = nodes[nodeIndex];
        updatedNode.leftChild = leftChild;
        updatedNode.rightChild = rightChild;
        nodes[nodeIndex] = updatedNode;
        
        BVHNode leftNode = nodes[leftChild];
        leftNode.escapeIndex = rightChild != -1 ? rightChild : nodeIndex + 1;
        nodes[leftChild] = leftNode;

        if (rightChild != -1)
        {
            BVHNode rightNode = nodes[rightChild];
            rightNode.escapeIndex = nodeIndex + 1;
            nodes[rightChild] = rightNode;
        }

        return nodeIndex;
    }
    
    void ComputeBounds(ref BVHNode node, List<Vector3> vertices, List<int> indices)
    {
        node.minBounds = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        node.maxBounds = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        for (int i = node.firstTriangle * 3; i < (node.firstTriangle + node.triangleCount) * 3; i++)
        {
            Vector3 v = vertices[indices[i]];
            node.minBounds = Vector3.Min(node.minBounds, v);
            node.maxBounds = Vector3.Max(node.maxBounds, v);
        }
    }

    int ChooseSplitAxis(BVHNode node)
    {
        Vector3 extents = node.maxBounds - node.minBounds;
        return (extents.x > extents.y && extents.x > extents.z) ? 0 :
            (extents.y > extents.z) ? 1 : 2;
    }

    int PartitionTrianglesMedian(List<Vector3> vertices, List<int> indices, int start, int count, int axis)
    {
        List<Tuple<int, float>> centroids = new List<Tuple<int, float>>();

        for (int i = start; i < start + count; i++)
        {
            Vector3 centroid = (vertices[indices[i * 3]] +
                                vertices[indices[i * 3 + 1]] +
                                vertices[indices[i * 3 + 2]]) / 3.0f;
            centroids.Add(new Tuple<int, float>(i, centroid[axis]));
        }

        centroids.Sort((a, b) => a.Item2.CompareTo(b.Item2));
        int median = centroids.Count / 2;

        List<int> sortedIndices = new List<int>();
        foreach (var c in centroids) sortedIndices.Add(c.Item1);
        
        for (int i = 0; i < sortedIndices.Count; i++)
        {
            int fromIndex = sortedIndices[i] * 3;
            int toIndex = (start + i) * 3;

            (indices[toIndex], indices[fromIndex]) = (indices[fromIndex], indices[toIndex]);
            (indices[toIndex + 1], indices[fromIndex + 1]) = (indices[fromIndex + 1], indices[toIndex + 1]);
            (indices[toIndex + 2], indices[fromIndex + 2]) = (indices[fromIndex + 2], indices[toIndex + 2]);
        }

        return start + median;
    }
}
