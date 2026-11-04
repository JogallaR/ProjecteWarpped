using System.Collections.Generic;
using UnityEngine;

public class LowPolyMountainGenerator : MonoBehaviour
{
    [Header("Generation")]
    public int mountainCount = 30;
    public int seed = 12345;

    [Header("Area")]
    public float areaSize = 300f;

    [Header("Mountain Size")]
    public float minHeight = 25f;
    public float maxHeight = 80f;

    public float minRadius = 8f;
    public float maxRadius = 22f;

    [Header("Geometry")]
    [Range(5, 16)]
    public int sides = 8;

    [Range(3, 12)]
    public int rings = 6;

    [Range(0f, 0.6f)]
    public float jaggedness = 0.18f;

    [Range(0.05f, 0.8f)]
    public float topRadiusFactor = 0.15f;

    [Header("Shape")]
    [Range(0f, 1f)]
    public float bendAmount = 0.15f;

    [Range(0f, 0.5f)]
    public float ringOffset = 0.10f;

    [Header("Appearance")]
    public Material mountainMaterial;

    public bool generateColliders = false;
    public bool randomRotation = true;

    [ContextMenu("Generate Mountains")]
    public void GenerateMountains()
    {
        ClearMountains();

        Random.InitState(seed);

        GameObject parent = new GameObject("Generated Mountains");
        parent.transform.SetParent(transform);
        parent.transform.localPosition = Vector3.zero;

        for (int i = 0; i < mountainCount; i++)
        {
            CreateMountain(parent.transform, i);
        }

        Debug.Log("Generated " + mountainCount + " mountains");
    }

    void CreateMountain(Transform parent, int index)
    {
        GameObject mountain = new GameObject("Mountain_" + index.ToString("000"));
        mountain.transform.SetParent(parent);

        float x = Random.Range(-areaSize * 0.5f, areaSize * 0.5f);
        float z = Random.Range(-areaSize * 0.5f, areaSize * 0.5f);

        mountain.transform.localPosition = new Vector3(x, 0f, z);

        float height = Random.Range(minHeight, maxHeight);
        float radius = Random.Range(minRadius, maxRadius);

        if (randomRotation)
        {
            mountain.transform.localRotation =
                Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        }

        Mesh mesh = GenerateMountainMesh(height, radius);

        MeshFilter mf = mountain.AddComponent<MeshFilter>();
        MeshRenderer mr = mountain.AddComponent<MeshRenderer>();

        mf.sharedMesh = mesh;

        if (mountainMaterial != null)
        {
            mr.sharedMaterial = mountainMaterial;
        }

        if (generateColliders)
        {
            MeshCollider mc = mountain.AddComponent<MeshCollider>();
            mc.sharedMesh = mesh;
        }
    }

    Mesh GenerateMountainMesh(float height, float baseRadius)
    {
        Mesh mesh = new Mesh();
        mesh.name = "LowPolyMountain";

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        Vector3[,] ringPoints = new Vector3[rings, sides];

        float bendX = Random.Range(-bendAmount, bendAmount) * baseRadius;
        float bendZ = Random.Range(-bendAmount, bendAmount) * baseRadius;

        for (int r = 0; r < rings; r++)
        {
            float t = r / (float)(rings - 1);

            float y = t * height;

            float radiusCurve = Mathf.Lerp(
                baseRadius,
                baseRadius * topRadiusFactor,
                t
            );

            float centerX = bendX * t;
            float centerZ = bendZ * t;

            centerX += Random.Range(-ringOffset, ringOffset) * baseRadius;
            centerZ += Random.Range(-ringOffset, ringOffset) * baseRadius;

            for (int s = 0; s < sides; s++)
            {
                float angle =
                    ((float)s / sides) * Mathf.PI * 2f;

                float variation =
                    Random.Range(
                        1f - jaggedness,
                        1f + jaggedness
                    );

                float currentRadius =
                    radiusCurve * variation;

                float px =
                    Mathf.Cos(angle) * currentRadius + centerX;

                float pz =
                    Mathf.Sin(angle) * currentRadius + centerZ;

                ringPoints[r, s] =
                    new Vector3(px, y, pz);
            }
        }

        // Costats
        for (int r = 0; r < rings - 1; r++)
        {
            for (int s = 0; s < sides; s++)
            {
                int next = (s + 1) % sides;

                Vector3 a = ringPoints[r, s];
                Vector3 b = ringPoints[r, next];
                Vector3 c = ringPoints[r + 1, next];
                Vector3 d = ringPoints[r + 1, s];

                AddFlatTriangle(verts, tris, a, c, b);
                AddFlatTriangle(verts, tris, a, d, c);
            }
        }

        // Tapa superior
        Vector3 topCenter = Vector3.zero;

        for (int s = 0; s < sides; s++)
        {
            topCenter += ringPoints[rings - 1, s];
        }

        topCenter /= sides;

        topCenter.y += height * 0.04f;

        for (int s = 0; s < sides; s++)
        {
            int next = (s + 1) % sides;

            AddFlatTriangle(
                verts,
                tris,
                ringPoints[rings - 1, s],
                topCenter,
                ringPoints[rings - 1, next]
            );
        }

        // Base
        Vector3 bottomCenter = Vector3.zero;

        for (int s = 0; s < sides; s++)
        {
            bottomCenter += ringPoints[0, s];
        }

        bottomCenter /= sides;

        for (int s = 0; s < sides; s++)
        {
            int next = (s + 1) % sides;

            AddFlatTriangle(
                verts,
                tris,
                bottomCenter,
                ringPoints[0, s],
                ringPoints[0, next]
            );
        }

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    void AddFlatTriangle(
        List<Vector3> verts,
        List<int> tris,
        Vector3 a,
        Vector3 b,
        Vector3 c)
    {
        int index = verts.Count;

        verts.Add(a);
        verts.Add(b);
        verts.Add(c);

        tris.Add(index);
        tris.Add(index + 1);
        tris.Add(index + 2);
    }

    [ContextMenu("Clear Mountains")]
    public void ClearMountains()
    {
        Transform old =
            transform.Find("Generated Mountains");

        if (old == null)
            return;

        if (Application.isPlaying)
            Destroy(old.gameObject);
        else
            DestroyImmediate(old.gameObject);
    }
}