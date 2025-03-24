using System;
using System.Collections.Generic;
using UnityEngine;

public struct BVHNode
{
    // Represents a node in the Bounding Volume Hierarchy (BVH).
    //
    // Fields:
    //   minBounds: The minimum point of the node’s axis-aligned bounding box (AABB).
    //   maxBounds: The maximum point of the node’s axis-aligned bounding box (AABB).
    //   leftChild: Index of the left child node in the BVH tree. -1 if there is no child.
    //   rightChild: Index of the right child node in the BVH tree. -1 if there is no child.
    //   firstTriangle: Index of the first triangle associated with this node.
    //   triangleCount: Number of triangles associated with this node.
    //   escapeIndex: Index used to quickly skip over the subtree when traversing the BVH.

    public Vector3 minBounds;
    public Vector3 maxBounds;
    public int leftChild;
    public int rightChild;
    public int firstTriangle;
    public int triangleCount;
    public int escapeIndex;
}

public class BVH
{
    // Constructs and manages a Bounding Volume Hierarchy (BVH) for efficient spatial queries.
    //
    // The BVH is built from a list of mesh vertices and triangle indices. It recursively partitions
    // the geometry into a binary tree, where each node contains a bounding box and either triangle
    // references or child nodes. This structure enables faster ray tracing and collision detection.
    //
    // Fields:
    //   nodes: The list of BVH nodes forming the hierarchy.
    //   triangles: Internal list of triangle indices used for building and partitioning.
    //   vertices: Reference to the mesh vertices used to compute bounding volumes.
    //
    // The BVH constructor immediately starts building the tree upon creation.

    public List<BVHNode> nodes = new List<BVHNode>();
    private List<int> triangles;
    private List<Vector3> vertices;
    
    public BVH(List<Vector3> vertices, List<int> indices)
    {
        this.vertices = vertices;
        this.triangles = new List<int>(indices);
        Build(0, triangles.Count / 3);
    }
    
    private int Build(int start, int count)
    {
        // Recursively builds the BVH (Bounding Volume Hierarchy) starting from a given triangle range.
        //
        // Args:
        //   start: The starting index of the triangle list to include in this node.
        //   count: The number of triangles to include in this node.
        //
        // Returns:
        //   The index of the newly created BVH node in the nodes list.
        //
        // This method calculates bounds for the current node, selects a splitting axis, 
        // partitions triangles along that axis, and recursively builds left and right child nodes.

        BVHNode node = new BVHNode();
        node.firstTriangle = start;
        node.triangleCount = count;
        node.escapeIndex = -1;

        ComputeBounds(ref node, vertices, triangles);
        int nodeIndex = nodes.Count;
        nodes.Add(node);

        if (count <= 4)
            return nodeIndex;

        int splitAxis = ChooseSplitAxis(node);
        int mid = PartitionTrianglesMedian(vertices, triangles, start, count, splitAxis);
        
        int leftChild = Build(start, mid - start);
        int rightChild = Build(mid, count - (mid - start));

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
        // Computes the axis-aligned bounding box (AABB) for the triangles in the given BVH node.
        //
        // Args:
        //   node: Reference to the BVH node for which to compute bounds.
        //   vertices: List of mesh vertex positions.
        //   indices: List of triangle indices corresponding to vertices.
        //
        // This function updates the minBounds and maxBounds of the node by iterating
        // over all triangles assigned to it.

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
        // Chooses the axis with the greatest extent of the bounding box for splitting the BVH node.
        //
        // Args:
        //   node: The BVH node whose bounds are evaluated.
        //
        // Returns:
        //   An integer representing the axis index:
        //   0 for X, 1 for Y, 2 for Z.
        //
        // The axis is chosen based on the difference between maxBounds and minBounds.

        Vector3 extents = node.maxBounds - node.minBounds;
        return (extents.x > extents.y && extents.x > extents.z) ? 0 :
            (extents.y > extents.z) ? 1 : 2;
    }

    int PartitionTrianglesMedian(List<Vector3> vertices, List<int> indices, int start, int count, int axis)
    {
        // Partitions triangles in the given range around the median centroid along the specified axis.
        //
        // Args:
        //   vertices: List of mesh vertex positions.
        //   indices: List of triangle indices (modified in-place).
        //   start: The starting index of the triangle range to partition.
        //   count: The number of triangles to partition.
        //   axis: The axis (0 = X, 1 = Y, 2 = Z) to use for computing centroids.
        //
        // Returns:
        //   The index that splits the triangle list into left and right partitions.
        //
        // This method sorts triangles by the centroid position along the given axis,
        // then rearranges triangle indices to group triangles spatially.

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
