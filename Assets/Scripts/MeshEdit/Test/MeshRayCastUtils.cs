using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GetampedPaint.Utils;

public static class MeshRayCastUtils
{
    #region Raycast

   
    public enum Culling
    {
        Back = 0x1,
        Front = 0x2,
        FrontBack = 0x4
    }

    public static bool MeshRaycast(Ray InWorldRay, ModifyMesh mesh, out MeshRaycastHit hit)
    {
        return MeshRaycast(InWorldRay, mesh, out hit, Mathf.Infinity, Culling.Front);
    }

 
    public static bool MeshRaycast(Ray InWorldRay, ModifyMesh mesh, out MeshRaycastHit hit, float distance, Culling cullingMode)
    {

        InWorldRay.origin -= mesh.transform.position;  // Why doesn't worldToLocalMatrix apply translation?
        InWorldRay.origin = mesh.transform.worldToLocalMatrix * InWorldRay.origin;
        InWorldRay.direction = mesh.transform.worldToLocalMatrix * InWorldRay.direction;

        Vector3[] vertices = mesh.vertices;

        float dist = 0f;
        Vector3 point = Vector3.zero;

        float OutHitPoint = Mathf.Infinity;
        float dot; // vars used in loop
        Vector3 nrm;    // vars used in loop
        int OutHitFace = -1;
        Vector3 OutNrm = Vector3.zero;

        /**
         * Iterate faces, testing for nearest hit to ray origin.  Optionally ignores backfaces.
         */
        for (int CurFace = 0; CurFace < mesh.faces.Length; ++CurFace)
        {
            int[] Indices = mesh.faces[CurFace].indices;

            for (int CurTriangle = 0; CurTriangle < Indices.Length; CurTriangle += 3)
            {
                Vector3 a = vertices[Indices[CurTriangle + 0]];
                Vector3 b = vertices[Indices[CurTriangle + 1]];
                Vector3 c = vertices[Indices[CurTriangle + 2]];

                nrm = Vector3.Cross(b - a, c - a);
                dot = Vector3.Dot(InWorldRay.direction, nrm);

                bool ignore = false;

                switch (cullingMode)
                {
                    case Culling.Front:
                        if (dot > 0f) ignore = true;
                        break;

                    case Culling.Back:
                        if (dot < 0f) ignore = true;
                        break;
                }

                if (!ignore && MathHelper.RayIntersectsTriangle(InWorldRay, a, b, c, out dist, out point))
                {
                    if (dist > OutHitPoint || dist > distance)
                        continue;

                    OutNrm = nrm;
                    OutHitFace = CurFace;
                    OutHitPoint = dist;

                    continue;
                }
            }
        }

        hit = new MeshRaycastHit(OutHitPoint,
                                InWorldRay.GetPoint(OutHitPoint),
                                OutNrm,
                                OutHitFace);

        return OutHitFace > -1;
    }


    public static bool MeshRaycast(Ray InWorldRay, ModifyMesh qe, out List<MeshRaycastHit> hits, float distance, Culling cullingMode)
    {

        InWorldRay.origin -= qe.transform.position;  

        InWorldRay.origin = qe.transform.worldToLocalMatrix * InWorldRay.origin;
        InWorldRay.direction = qe.transform.worldToLocalMatrix * InWorldRay.direction;

        Vector3[] vertices = qe.vertices;

        float dist = 0f;
        Vector3 point = Vector3.zero;

        float dot;
        Vector3 nrm;   
        hits = new List<MeshRaycastHit>();

        for (int CurFace = 0; CurFace < qe.faces.Length; ++CurFace)
        {
            int[] Indices = qe.faces[CurFace].indices;

            for (int CurTriangle = 0; CurTriangle < Indices.Length; CurTriangle += 3)
            {
                Vector3 a = vertices[Indices[CurTriangle + 0]];
                Vector3 b = vertices[Indices[CurTriangle + 1]];
                Vector3 c = vertices[Indices[CurTriangle + 2]];

                if (MathHelper.RayIntersectsTriangle(InWorldRay, a, b, c, out dist, out point))
                {
                    nrm = Vector3.Cross(b - a, c - a);

                    switch (cullingMode)
                    {
                        case Culling.Front:
                            dot = Vector3.Dot(InWorldRay.direction, -nrm);

                            if (dot > 0f)
                                goto case Culling.FrontBack;
                            break;

                        case Culling.Back:
                            dot = Vector3.Dot(InWorldRay.direction, nrm);

                            if (dot > 0f)
                                goto case Culling.FrontBack;
                            break;

                        case Culling.FrontBack:
                            hits.Add(new MeshRaycastHit(dist,
                                                        InWorldRay.GetPoint(dist),
                                                        nrm,
                                                        CurFace));
                            break;
                    }

                    continue;
                }
            }
        }

        return hits.Count > 0;
    }

    const float MAX_EDGE_SELECT_DISTANCE = 20f;

    public static bool EdgeRaycast(Vector2 mousePosition, ElementCache selection, out Edge edge)
    {
        ModifyMesh mesh = selection.mesh;

        Vector3 v0, v1;
        float bestDistance = Mathf.Infinity;
        float distance = 0f;
        edge = null;

        GameObject go = HandleUtility.PickGameObject(mousePosition, false);

        if (go == null || go != selection.transform.gameObject)
        {
            Edge[] edges = mesh.userEdges;

            int width = Screen.width;
            int height = Screen.height;

            for (int i = 0; i < edges.Length; i++)
            {
                v0 = selection.verticesInWorldSpace[edges[i].x];
                v1 = selection.verticesInWorldSpace[edges[i].y];

                distance = HandleUtility.DistanceToLine(v0, v1);

                if (distance < bestDistance && distance < MAX_EDGE_SELECT_DISTANCE)
                {
                    Vector3 vs0 = SceneView.lastActiveSceneView.camera.WorldToScreenPoint(v0);

                    if (vs0.z <= 0 || vs0.x < 0 || vs0.y < 0 || vs0.x > width || vs0.y > height)
                        continue;

                    Vector3 vs1 = SceneView.lastActiveSceneView.camera.WorldToScreenPoint(v1);

                    if (vs1.z <= 0 || vs1.x < 0 || vs1.y < 0 || vs1.x > width || vs1.y > height)
                        continue;


                    bestDistance = distance;
                    edge = edges[i];
                }
            }
        }
        else
        {
            // Test culling
            List<MeshRaycastHit> hits;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

            if (MeshRaycast(ray, mesh, out hits, Mathf.Infinity, Culling.FrontBack))
            {
                hits.Sort((x, y) => x.Distance.CompareTo(y.Distance));

                Vector3[] v = mesh.vertices;

                for (int i = 0; i < hits.Count; i++)
                {
                    if (PointIsOccluded(mesh, mesh.transform.TransformPoint(hits[i].Point)))
                        continue;

                    foreach (Edge e in mesh.faces[hits[i].FaceIndex].GetEdges())
                    {
                        float d = HandleUtility.DistancePointLine(hits[i].Point, v[e.x], v[e.y]);

                        if (d < bestDistance)
                        {
                            bestDistance = d;
                            edge = e;
                        }
                    }

                    if (Vector3.Dot(ray.direction, mesh.transform.TransformDirection(hits[i].Normal)) < 0f)
                        break;
                }

                if (edge != null && HandleUtility.DistanceToLine(mesh.transform.TransformPoint(v[edge.x]), mesh.transform.TransformPoint(v[edge.y])) > MAX_EDGE_SELECT_DISTANCE)
                {
                    edge = null;
                }
                else
                {
                    edge.x = mesh.ToUserIndex(edge.x);
                    edge.y = mesh.ToUserIndex(edge.y);
                }
            }
        }

        return edge != null;
    }

    public static bool VertexRaycast(Vector2 mousePosition, int rectSize, ElementCache selection, out int index)
    {
        ModifyMesh mesh = selection.mesh;

        float bestDistance = Mathf.Infinity;
        float distance = 0f;
        index = -1;

        GameObject go = HandleUtility.PickGameObject(mousePosition, false);

        if (go == null || go != selection.transform.gameObject)
        {
            Camera cam = SceneView.lastActiveSceneView.camera;
            int width = Screen.width;
            int height = Screen.height;

            Rect mouseRect = new Rect(mousePosition.x - (rectSize / 2f), mousePosition.y - (rectSize / 2f), rectSize, rectSize);
            List<int> user = (List<int>)selection.mesh.GetUserIndices();

            for (int i = 0; i < user.Count; i++)
            {
                if (mouseRect.Contains(HandleUtility.WorldToGUIPoint(selection.verticesInWorldSpace[user[i]])))
                {
                    Vector3 v = cam.WorldToScreenPoint(selection.verticesInWorldSpace[user[i]]);

                    distance = Vector2.Distance(mousePosition, v);

                    if (distance < bestDistance)
                    {
                        if (v.z <= 0 || v.x < 0 || v.y < 0 || v.x > width || v.y > height)
                            continue;

                        if (PointIsOccluded(mesh, selection.verticesInWorldSpace[user[i]]))
                            continue;

                        index = user[i];
                        bestDistance = Vector2.Distance(v, mousePosition);
                    }
                }
            }
        }
        else
        {
            // Test culling
            List<MeshRaycastHit> hits;
            Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

            if (MeshRaycast(ray, mesh, out hits, Mathf.Infinity, Culling.FrontBack))
            {
                hits.Sort((x, y) => x.Distance.CompareTo(y.Distance));

                Vector3[] v = mesh.vertices;

                for (int i = 0; i < hits.Count; i++)
                {
                    if (PointIsOccluded(mesh, mesh.transform.TransformPoint(hits[i].Point)))
                        continue;

                    foreach (int tri in mesh.faces[hits[i].FaceIndex].indices)
                    {
                        float d = Vector3.Distance(hits[i].Point, v[tri]);

                        if (d < bestDistance)
                        {
                            bestDistance = d;
                            index = tri;
                        }
                    }

                    if (Vector3.Dot(ray.direction, mesh.transform.TransformDirection(hits[i].Normal)) < 0f)
                        break;
                }

                if (index > -1 && Vector2.Distance(mousePosition, HandleUtility.WorldToGUIPoint(selection.verticesInWorldSpace[index])) > rectSize * 1.3f)
                {
                    index = -1;
                }
            }
        }

        if (index > -1)
            index = mesh.ToUserIndex(index);

        return index > -1;
    }

    public static bool PointIsOccluded(ModifyMesh mesh, Vector3 worldPoint)
    {
        Camera cam = SceneView.lastActiveSceneView.camera;
        Vector3 dir = (cam.transform.position - worldPoint).normalized;

        Ray ray = new Ray(worldPoint + dir * .0001f, dir);

        MeshRaycastHit hit;

        return MeshRaycast(ray, mesh, out hit, Vector3.Distance(cam.transform.position, worldPoint), Culling.Back);
    }
    #endregion
}
