using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BrushStroke{

    //Classes
    private Controller controller;
    private Tools[] tools;
    private Color[] colors;
    private Sound[] sounds; 
    
    private bool isDouble;

    //Mesh
    private Mesh m = new Mesh();
    private List<Vector3> origin = new List<Vector3>();
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector3> normals = new List<Vector3>();
    private List<Vector2> uV = new List<Vector2>();
    private List<float> rotation = new List<float>();
    private Material brushMaterial;

    //Indexes
    private Vector4 vIndex;
    private int segmentIndex;
    Vector3 currentPosition;

    //Mesures
    private float width = 0.08f;
    private float vertexDistance = 0.01f;
    private int smoothingFactor = 15;
    private int velocityRange = 3;
    private List<float> distance = new List<float>();
    private float velocity = new float();

    //Rotations
    private float angle = new float();
    private float lastAngle = new float();

    private GameObject container;

    //Constructor
    public BrushStroke(Controller _controller, Tools[] _tools, Color[] _colors, Sound[] _sounds, Material _brushMaterial, GameObject _container, bool _isDouble)
    {
        //Control Manager's Properties
        controller = _controller;
        tools = _tools;
        colors = _colors;
        sounds = _sounds;
        brushMaterial = _brushMaterial;
        container = _container;
        isDouble = _isDouble;

        GameObject paintStroke = new GameObject((isDouble == true) ? "face" : "backface");
        paintStroke.AddComponent<MeshRenderer>();
        paintStroke.AddComponent<MeshFilter>();
        paintStroke.transform.parent = container.transform;
        paintStroke.layer = 12;

        MeshFilter meshFilter = (MeshFilter)paintStroke.GetComponent<MeshFilter>();
        MeshRenderer renderer = paintStroke.GetComponent(typeof(MeshRenderer)) as MeshRenderer;

        meshFilter.mesh = m;
        renderer.material = brushMaterial;
        renderer.material.color = colors[controller.currentColor];
        renderer.material.SetColor("_EmissionColor", colors[controller.currentColor]);

        origin.Add(tools[controller.currentTool].head.transform.position);

        segmentIndex = 0;
        vIndex.Set(0, 1, 2, 3);
        currentPosition = tools[controller.currentTool].head.transform.GetChild(0).transform.position;

        
    }

    public void GenerateStroke()
    {
        currentPosition = tools[controller.currentTool].head.transform.GetChild(0).transform.GetChild(0).transform.position;

        distance.Add(Vector3.Distance(origin[origin.Count - 1], tools[controller.currentTool].head.transform.position));

        if (distance.Count >= velocityRange)
            distance.RemoveAt(0);

        velocity = distance.Average();

        if (distance.Count - 1 > 0 && distance.Count - 1 > vertexDistance/* && velocity >= 0.097f*/)
        {
            origin.Add(currentPosition);
            AddSegment();
        }

        
    }

    private Mesh AddSegment()
    {
        int verticiesCount = vertices.Count;

        Vector3 lastOrigin = new Vector3();

        if (segmentIndex > 0)
            lastOrigin = origin[origin.Count - 2];
        else
            lastOrigin = currentPosition;

        

        if (segmentIndex > smoothingFactor - 9)
            angle = Rotation((segmentIndex > 6) ? origin[origin.Count - 1] : origin[origin.Count - 2], origin[origin.Count - 2], origin[origin.Count - 3]);

        tools[controller.currentTool].head.transform.GetChild(0).transform.Rotate(0, angle,0);

        for (int i = 0; i < 2; i++)
        {
            vertices.Add(VerticiesCoordinates(i, angle));
            uV.Add(UVCoordinatesStretch((i % 2 == 0) ? true : false, verticiesCount));
        }

        if (segmentIndex > smoothingFactor - 10)
            vertices = Smoothen(vertices, smoothingFactor);
        m.vertices = vertices.ToArray();
        m.uv = uV.ToArray();

        if (vertices.Count >= 4)
        {
            if (vertices.Count == 4)
            {
                normals.Add(Normal(vertices[(int)vIndex[2]], vertices[(int)vIndex[1]], vertices[(int)vIndex[3]]));
                normals.Add(Normal(vertices[(int)vIndex[3]], vertices[(int)vIndex[2]], vertices[(int)vIndex[0]]));
            }
            normals.Add(Normal(vertices[(int)vIndex[2]], vertices[(int)vIndex[1]], vertices[(int)vIndex[3]]));
            normals.Add(Normal(vertices[(int)vIndex[3]], vertices[(int)vIndex[2]], vertices[(int)vIndex[0]]));


            if (!isDouble)
                ReverseNormals();
            normals = Smoothen(normals, smoothingFactor);
            m.normals = normals.ToArray();

            triangles.Add((int)vIndex.w);
            triangles.Add((isDouble) ? (int)vIndex.z : (int)vIndex.y);
            triangles.Add((isDouble) ? (int)vIndex.y : (int)vIndex.z);
            triangles.Add((int)vIndex.x);
            triangles.Add((isDouble) ? (int)vIndex.y : (int)vIndex.z);
            triangles.Add((isDouble) ? (int)vIndex.z : (int)vIndex.y);
            m.triangles = triangles.ToArray();

            vIndex.Set(vIndex.x += 2, vIndex.y += 2, vIndex.z += 2, vIndex.w += 2);
        }
        

        segmentIndex += 1;

        return m;
    }

    private Mesh RemoveSegment(int segmentChoice)
    {
        if (segmentChoice % 2 == 0 && segmentIndex > 0)
        {
            for (int i = 0; i < 2; i++)
            {
                vertices.RemoveAt((int)segmentChoice / 2 + i);
                uV.RemoveAt(segmentChoice * 2 + i);

            }
            m.vertices = vertices.ToArray();
            m.uv = uV.ToArray();

            for (int i = 0; i < 4; i++)
            {
                normals.RemoveAt((int)segmentChoice / 2 + i);
            }
            m.normals = normals.ToArray();

            for (int i = 0; i < 6; i++)
            {
                triangles.RemoveAt((int)segmentChoice / 2 + i);
            }
            m.triangles = triangles.ToArray();
        }

        return m;
    }

    private Vector3 VerticiesCoordinates(int vectorId, float angle)
    {
        float r = width / 2;
        float x = origin[origin.Count - 1].x + Mathf.Cos(2 * Mathf.PI * vectorId / 2 + angle) * r;
        float y = origin[origin.Count - 1].y + Mathf.Sin(2 * Mathf.PI * vectorId / 2 + angle) * r;
        float z = new float();

        z = origin[origin.Count - 1].z;

        return new Vector3(x, y, z);
    }

    private Vector2 UVCoordinatesStretch(bool isEven, int verticiesCount)
    {
        if (uV != null)
        {
            for (int i = 0; i < uV.Count - 1; i++)
            {
                float u = (segmentIndex == 0 || i == 0) ? u = 1 : u = 1f / segmentIndex * i;
                int v = (i % 2 == 0) ? 0 : 1;
                uV[i] = new Vector2(u, v);
            }
        }
        Vector2 uVCoordinates = (isEven) ? new Vector2(1, 0) : new Vector2(0, 0);
        return uVCoordinates;
    }

    private Vector2 UVCoordinatesTile(bool isEven, int verticiesCount)
    {
        if (uV != null)
        {
            float u, v;

            if (segmentIndex < 10)
            {
                for (int i = 0; i < verticiesCount - 1; i++)
                {
                    u = (segmentIndex == 0 || i == 0) ? u = 1 : u = 1f / segmentIndex * i;
                    v = (i % 2 == 0) ? 0 : 1;
                    uV[i] = new Vector2(u, v);
                }
            }
            else
            {
                for (int i = 0; i < verticiesCount - 1; i++)
                {
                    if (i <= segmentIndex / 3)
                    {
                        u = (segmentIndex == 0 || i == 0) ? u = 1 : u = 1f / segmentIndex * i;

                    }
                    else if (i > segmentIndex / 3 && i <= segmentIndex / 1.5f)
                    {
                        u = 0;
                    }
                    else
                    {
                        u = 1f / segmentIndex * i;
                    }

                    v = (i % 2 == 0) ? 0 : 1;
                    uV[i] = new Vector2(u, v);
                }
            }
        }
        Vector2 uVCoordinates = (isEven) ? new Vector2(1, 0) : new Vector2(1, 1);
        return uVCoordinates;
    }

    private float Rotation(Vector3 a, Vector3 b, Vector3 c)
    {
        float currentAngle = new float();

        Vector3 ba = b - a;
        Vector3 bc = b - c;

        rotation.Add(Mathf.Atan2(ba.y - bc.y, ba.x - bc.x) + Mathf.PI / 2);
        currentAngle = rotation[rotation.Count - 1];

        lastAngle = angle;

        return currentAngle;
    }

    private List<Vector3> Smoothen(List<Vector3> elementToSmoothen, int smoothingFactor)
    {
        if (segmentIndex >= smoothingFactor)
        {
            Vector3 target;

            for (int i = elementToSmoothen.Count - 3 - smoothingFactor; i < elementToSmoothen.Count - 3; i++)
            {
                target = Vector3.Lerp(elementToSmoothen[i - 2], elementToSmoothen[i + 2], 0.5f);
                elementToSmoothen[i] = target;
            }
        }

        return elementToSmoothen;
    }

    private Vector3 Normal(Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 side1 = b - a;
        Vector3 side2 = c - a;
        return Vector3.Cross(side1, side2).normalized;
    }

    private void ReverseNormals()
    {
        Vector3[] normals = m.normals;
        for (int i = 0; i < normals.Length; i++)
            normals[i] = -normals[i];
        m.normals = normals;
    }
}
