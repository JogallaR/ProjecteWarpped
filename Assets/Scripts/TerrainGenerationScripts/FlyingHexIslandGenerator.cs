using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class FlyingHexIslandGenerator : MonoBehaviour
{
    [Header("GENERATE")]
    public bool generateNow = false;
    public bool clearNow = false;

    [Header("HEX GRID")]
    [Range(3, 15)]
    public int gridRadius = 7;

    public float hexSize = 2f;

    [Header("TERRAIN HEIGHT")]
    public float minHeight = 0f;
    public float maxHeight = 2.5f;

    [Header("ISLAND SHAPE")]
    [Range(0f, 0.5f)]
    public float edgeVariation = 0.15f;

    [Header("FLOATING ROCK")]
    public float islandDepth = 14f;

    [Range(0.3f, 0.9f)]
    public float middleRadius = 0.72f;

    [Range(0.15f, 0.6f)]
    public float bottomRadius = 0.38f;

    [Header("RANDOM")]
    public int seed = 12345;

    [Header("MATERIAL")]
    public Material terrainMaterial;
    public Material rockMaterial;

    [Header("COLLIDER")]
    public bool generateCollider = true;

    private class Cell
    {
        public int q;
        public int r;
        public Vector3 position;
        public float height;
    }

    private void Update()
    {
        if (generateNow)
        {
            generateNow = false;
            GenerateIsland();
        }

        if (clearNow)
        {
            clearNow = false;
            ClearIsland();
        }
    }

    // =========================================================
    // GENERATE
    // =========================================================

    public void GenerateIsland()
    {
        ClearIsland();

        Random.InitState(seed);

        GameObject root =
            new GameObject("Generated Island");

        root.transform.SetParent(transform);
        root.transform.localPosition = Vector3.zero;

        List<Cell> cells = CreateCells();

        CreateHexTerrain(cells, root.transform);
        CreateFloatingRock(cells, root.transform);

        Debug.Log(
            "Flying Island generated: " +
            cells.Count +
            " hexagons."
        );
    }

    // =========================================================
    // CELLS
    // =========================================================

    private List<Cell> CreateCells()
    {
        List<Cell> cells =
            new List<Cell>();

        for (int q = -gridRadius;
             q <= gridRadius;
             q++)
        {
            for (int r = -gridRadius;
                 r <= gridRadius;
                 r++)
            {
                int s = -q - r;

                int distance =
                    Mathf.Max(
                        Mathf.Abs(q),
                        Mathf.Abs(r),
                        Mathf.Abs(s)
                    );

                if (distance > gridRadius)
                    continue;

                // Només fem irregular la vora.
                if (distance >= gridRadius)
                {
                    if (Random.value < edgeVariation)
                        continue;
                }

                Vector3 position =
                    AxialToWorld(q, r);

                float noise =
                    Mathf.PerlinNoise(
                        (q + seed) * 0.18f,
                        (r + seed) * 0.18f
                    );

                float center =
                    1f -
                    distance /
                    (float)gridRadius;

                float height =
                    Mathf.Lerp(
                        minHeight,
                        maxHeight,
                        noise
                    );

                // Centre lleugerament més alt.
                height += center * 1.2f;

                Cell cell =
                    new Cell();

                cell.q = q;
                cell.r = r;
                cell.position = position;
                cell.height = height;

                cells.Add(cell);
            }
        }

        return cells;
    }

    // =========================================================
    // HEX POSITION
    // =========================================================

    private Vector3 AxialToWorld(
        int q,
        int r)
    {
        float x =
            hexSize *
            Mathf.Sqrt(3f) *
            (q + r * 0.5f);

        float z =
            hexSize *
            1.5f *
            r;

        return new Vector3(
            x,
            0f,
            z
        );
    }

    // =========================================================
    // TOP TERRAIN
    // =========================================================

    private void CreateHexTerrain(
        List<Cell> cells,
        Transform parent)
    {
        GameObject obj =
            new GameObject("Hex Terrain");

        obj.transform.SetParent(parent);

        Mesh mesh =
            new Mesh();

        mesh.name =
            "Low Poly Hex Terrain";

        List<Vector3> vertices =
            new List<Vector3>();

        List<int> triangles =
            new List<int>();

        foreach (Cell cell in cells)
        {
            int centerIndex =
                vertices.Count;

            vertices.Add(
                new Vector3(
                    cell.position.x,
                    cell.height,
                    cell.position.z
                )
            );

            Vector3[] corners =
                GetCorners(
                    cell.position
                );

            for (int i = 0; i < 6; i++)
            {
                corners[i].y =
                    cell.height;

                vertices.Add(
                    corners[i]
                );
            }

            for (int i = 0; i < 6; i++)
            {
                int next =
                    (i + 1) % 6;

                triangles.Add(
                    centerIndex
                );

                triangles.Add(
                    centerIndex + 1 + next
                );

                triangles.Add(
                    centerIndex + 1 + i
                );
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(
            triangles,
            0
        );

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshFilter filter =
            obj.AddComponent<MeshFilter>();

        MeshRenderer renderer =
            obj.AddComponent<MeshRenderer>();

        filter.sharedMesh =
            mesh;

        renderer.sharedMaterial =
            terrainMaterial != null
                ? terrainMaterial
                : CreateMaterial(
                    "Island Terrain",
                    new Color(
                        0.32f,
                        0.48f,
                        0.32f
                    )
                );

        if (generateCollider)
        {
            MeshCollider collider =
                obj.AddComponent<MeshCollider>();

            collider.sharedMesh =
                mesh;
        }
    }

    // =========================================================
    // FLOATING ROCK
    // =========================================================

    private void CreateFloatingRock(
        List<Cell> cells,
        Transform parent)
    {
        GameObject obj =
            new GameObject(
                "Floating Rock"
            );

        obj.transform.SetParent(parent);

        Mesh mesh =
            new Mesh();

        mesh.name =
            "Low Poly Floating Rock";

        List<Vector3> vertices =
            new List<Vector3>();

        List<int> triangles =
            new List<int>();

        /*
         * En comptes de fer una punta per cada hexagon,
         * fem 3 nivells:
         *
         * TOP
         *   ↓
         * MIDDLE
         *   ↓
         * BOTTOM
         *
         * Això crea una massa de roca.
         */

        List<Vector3> boundary =
            GetBoundary(cells);

        if (boundary.Count < 3)
            return;

        int count =
            boundary.Count;

        // TOP RING
        for (int i = 0; i < count; i++)
        {
            Vector3 p =
                boundary[i];

            p.y =
                GetBoundaryHeight(
                    p,
                    cells
                );

            vertices.Add(p);
        }

        // MIDDLE RING
        for (int i = 0; i < count; i++)
        {
            Vector3 p =
                boundary[i];

            p *= middleRadius;

            p.y =
                -islandDepth * 0.45f;

            vertices.Add(p);
        }

        // BOTTOM RING
        for (int i = 0; i < count; i++)
        {
            Vector3 p =
                boundary[i];

            p *= bottomRadius;

            p.y =
                -islandDepth;

            vertices.Add(p);
        }

        // TOP -> MIDDLE
        for (int i = 0; i < count; i++)
        {
            int next =
                (i + 1) % count;

            AddQuad(
                triangles,
                i,
                next,
                count + next,
                count + i
            );
        }

        // MIDDLE -> BOTTOM
        for (int i = 0; i < count; i++)
        {
            int next =
                (i + 1) % count;

            AddQuad(
                triangles,
                count + i,
                count + next,
                count * 2 + next,
                count * 2 + i
            );
        }

        // BOTTOM
        int bottomCenter =
            vertices.Count;

        vertices.Add(
            new Vector3(
                0f,
                -islandDepth - 1f,
                0f
            )
        );

        int bottomStart =
            count * 2;

        for (int i = 0; i < count; i++)
        {
            int next =
                (i + 1) % count;

            triangles.Add(
                bottomCenter
            );

            triangles.Add(
                bottomStart + i
            );

            triangles.Add(
                bottomStart + next
            );
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(
            triangles,
            0
        );

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshFilter filter =
            obj.AddComponent<MeshFilter>();

        MeshRenderer renderer =
            obj.AddComponent<MeshRenderer>();

        filter.sharedMesh =
            mesh;

        renderer.sharedMaterial =
            rockMaterial != null
                ? rockMaterial
                : CreateMaterial(
                    "Floating Rock",
                    new Color(
                        0.20f,
                        0.23f,
                        0.22f
                    )
                );
    }

    // =========================================================
    // BOUNDARY
    // =========================================================

    private List<Vector3> GetBoundary(
        List<Cell> cells)
    {
        List<Vector3> points =
            new List<Vector3>();

        foreach (Cell cell in cells)
        {
            Vector3[] corners =
                GetCorners(
                    cell.position
                );

            for (int side = 0;
                 side < 6;
                 side++)
            {
                int nq;
                int nr;

                GetNeighbour(
                    cell.q,
                    cell.r,
                    side,
                    out nq,
                    out nr
                );

                if (FindCell(
                    cells,
                    nq,
                    nr
                ))
                {
                    continue;
                }

                points.Add(
                    corners[side]
                );
            }
        }

        // Ordenem els punts al voltant de l'illa.
        Vector3 center =
            Vector3.zero;

        foreach (Vector3 p in points)
            center += p;

        center /= points.Count;

        points.Sort(
            (a, b) =>
            {
                float angleA =
                    Mathf.Atan2(
                        a.z - center.z,
                        a.x - center.x
                    );

                float angleB =
                    Mathf.Atan2(
                        b.z - center.z,
                        b.x - center.x
                    );

                return angleA.CompareTo(
                    angleB
                );
            }
        );

        return points;
    }

    // =========================================================
    // HEIGHT
    // =========================================================

    private float GetBoundaryHeight(
        Vector3 position,
        List<Cell> cells)
    {
        float closest =
            float.MaxValue;

        float height = 0f;

        foreach (Cell cell in cells)
        {
            float distance =
                Vector3.Distance(
                    new Vector3(
                        position.x,
                        0f,
                        position.z
                    ),
                    new Vector3(
                        cell.position.x,
                        0f,
                        cell.position.z
                    )
                );

            if (distance < closest)
            {
                closest = distance;
                height = cell.height;
            }
        }

        return height;
    }

    // =========================================================
    // QUAD
    // =========================================================

    private void AddQuad(
        List<int> triangles,
        int a,
        int b,
        int c,
        int d)
    {
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c);

        triangles.Add(a);
        triangles.Add(c);
        triangles.Add(d);
    }

    // =========================================================
    // HEX CORNERS
    // =========================================================

    private Vector3[] GetCorners(
        Vector3 center)
    {
        Vector3[] corners =
            new Vector3[6];

        for (int i = 0; i < 6; i++)
        {
            float angle =
                Mathf.Deg2Rad *
                (60f * i + 30f);

            corners[i] =
                center +
                new Vector3(
                    Mathf.Cos(angle) *
                    hexSize,

                    0f,

                    Mathf.Sin(angle) *
                    hexSize
                );
        }

        return corners;
    }

    // =========================================================
    // NEIGHBOUR
    // =========================================================

    private void GetNeighbour(
        int q,
        int r,
        int side,
        out int nq,
        out int nr)
    {
        int[] dq =
        {
            1, 0, -1,
            -1, 0, 1
        };

        int[] dr =
        {
            0, 1, 1,
            0, -1, -1
        };

        nq =
            q + dq[side];

        nr =
            r + dr[side];
    }

    // =========================================================
    // FIND CELL
    // =========================================================

    private bool FindCell(
        List<Cell> cells,
        int q,
        int r)
    {
        foreach (Cell cell in cells)
        {
            if (cell.q == q &&
                cell.r == r)
            {
                return true;
            }
        }

        return false;
    }

    // =========================================================
    // MATERIAL
    // =========================================================

    private Material CreateMaterial(
        string materialName,
        Color color)
    {
        Shader shader =
            Shader.Find(
                "Universal Render Pipeline/Lit"
            );

        if (shader == null)
            shader =
                Shader.Find("Standard");

        Material material =
            new Material(shader);

        material.name =
            materialName;

        if (material.HasProperty(
            "_BaseColor"))
        {
            material.SetColor(
                "_BaseColor",
                color
            );
        }

        if (material.HasProperty(
            "_Color"))
        {
            material.SetColor(
                "_Color",
                color
            );
        }

        return material;
    }

    // =========================================================
    // CLEAR
    // =========================================================

    public void ClearIsland()
    {
        Transform old =
            transform.Find(
                "Generated Island"
            );

        if (old == null)
            return;

        if (Application.isPlaying)
            Destroy(old.gameObject);
        else
            DestroyImmediate(
                old.gameObject
            );
    }
}