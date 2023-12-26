using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Edge : System.IEquatable<Edge>
{
    public int x, y;

    public Edge(int _x, int _y)
    {
        x = _x;
        y = _y;
    }

    public Edge(Edge edge)
    {
        x = edge.x;
        y = edge.y;
    }

    public override int GetHashCode()
    {
        int hashX;
        int hashY;

        if (x < y)
        {
            hashX = x.GetHashCode();
            hashY = y.GetHashCode();
        }
        else
        {
            hashX = y.GetHashCode();
            hashY = x.GetHashCode();
        }

        //Calculate the hash code for the product. 
        return hashX ^ hashY;
    }

    public bool Equals(Edge edge)
    {
        return (this.x == edge.x && this.y == edge.y) || (this.x == edge.y && this.y == edge.x);
    }
}

public static class qe_Edge_Ext
{
    public static IEnumerable<Edge> ToSharedIndex(this IEnumerable<Edge> arr, Dictionary<int, int> lookup)
    {
        List<Edge> vals = new List<Edge>();

        foreach (Edge edge in arr)
        {
            vals.Add(new Edge(
                lookup[edge.x],
                lookup[edge.y]));
        }

        return vals;
    }

    public static IEnumerable<Edge> ToTriangleIndex(this IEnumerable<Edge> arr, List<List<int>> shared)
    {
        List<Edge> keys = new List<Edge>();

        foreach (Edge edge in arr)
        {
            keys.Add(new Edge(shared[edge.x][0], shared[edge.y][0]));
        }

        return keys;
    }

    public static IList<int> ToIndices(this IList<Edge> arr)
    {
        int[] ind = new int[arr.Count * 2];
        int n = 0;

        for (int i = 0; i < arr.Count; i++)
        {
            ind[n++] = arr[i].x;
            ind[n++] = arr[i].y;
        }

        return ind;
    }
}
