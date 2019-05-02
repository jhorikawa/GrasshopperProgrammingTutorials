using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Rhino.Compute;
using Rhino;

public class TestCompute : MonoBehaviour
{
    public string authToken;
    public Material mat;
    private Rhino.FileIO.File3dm model;

    private float prevHeight, prevPipeRad, prevSegments, prevAngle;
    private float height = 10f;
    private float pipeRad = 2f;
    private int segments = 5;
    private float angle = 30f;



    // Start is called before the first frame update
    void Start()
    {
        ComputeServer.AuthToken = authToken;
        ComputeServer.WebAddress = "http://127.0.0.1:8081/";
    }

    // Update is called once per frame
    void Update()
    {
        bool updateGeo = false;
        if(prevHeight != height)
        {
            updateGeo = true;
            prevHeight = height;
        }

        if(prevAngle != angle)
        {
            updateGeo = true;
            prevAngle = angle;
        }

        if(prevPipeRad != pipeRad)
        {
            updateGeo = true;
            prevPipeRad = pipeRad;
        }

        if(prevSegments != segments)
        {
            updateGeo = true;
            prevSegments = segments;
        }


        if (updateGeo)
        {
            DeleteMeshes();

            GenerateGeo();
        }
    }

    private void DeleteMeshes()
    {
        var objs = GameObject.FindObjectsOfType<MeshFilter>();
        foreach (var obj in objs)
        {
            Destroy(obj.mesh);
            Destroy(obj.gameObject);
        }
        Resources.UnloadUnusedAssets();
    }

    private void GenerateGeo()
    {
        model = new Rhino.FileIO.File3dm();

        int num = 10;
        float outerRad = 2f;

        var curves = new List<Rhino.Geometry.NurbsCurve>();
        for (int i = 0; i < num; i++)
        {
            var pt = new Rhino.Geometry.Point3d(0, 0, height / (num - 1) * i);
            var circle = new Rhino.Geometry.Circle(pt, pipeRad);
            var polygon = Rhino.Geometry.Polyline.CreateInscribedPolygon(circle, segments);
            var curve = polygon.ToNurbsCurve();
            curve.Rotate(i * Mathf.Deg2Rad * angle, new Rhino.Geometry.Vector3d(0, 0, 1), new Rhino.Geometry.Point3d(0, 0, 0));
            curve.Translate(new Rhino.Geometry.Vector3d(Mathf.Cos(Mathf.Deg2Rad * angle * i), Mathf.Sin(Mathf.Deg2Rad * angle * i), 0) * outerRad);
            curves.Add(curve);
        }

        var breps = BrepCompute.CreateFromLoft(curves, Rhino.Geometry.Point3d.Unset, Rhino.Geometry.Point3d.Unset, Rhino.Geometry.LoftType.Normal, false);

        var meshList = new List<Rhino.Geometry.Mesh>();
        foreach (var brep in breps)
        {
            var brep2 = brep.CapPlanarHoles(0.001);
            var meshes = MeshCompute.CreateFromBrep(brep2);
            meshList.AddRange(meshes.ToList());

            model.Objects.AddBrep(brep2);
        }

        foreach (var mesh in meshList)
        {

            Mesh meshObj = new Mesh();

            var vertices = new List<Vector3>();
            foreach (var meshVertex in mesh.Vertices)
            {
                var vertex = new Vector3(meshVertex.X, meshVertex.Z, meshVertex.Y);
                vertices.Add(vertex);
            }

            var triangles = new List<int>();
            foreach (var meshFace in mesh.Faces)
            {
                if (meshFace.IsTriangle)
                {
                    triangles.Add(meshFace.C);
                    triangles.Add(meshFace.B);
                    triangles.Add(meshFace.A);
                }
                else if (meshFace.IsQuad)
                {
                    triangles.Add(meshFace.C);
                    triangles.Add(meshFace.B);
                    triangles.Add(meshFace.A);
                    triangles.Add(meshFace.D);
                    triangles.Add(meshFace.C);
                    triangles.Add(meshFace.A);
                }
            }

            var normals = new List<Vector3>();
            foreach(var normal in mesh.Normals)
            {
                normals.Add(new Vector3(normal.X, normal.Z, normal.Y));
            }

            meshObj.vertices = vertices.ToArray();
            meshObj.triangles = triangles.ToArray();
            meshObj.normals = normals.ToArray();


            GameObject gb = new GameObject();
            gb.AddComponent<MeshFilter>().mesh = meshObj;
            gb.AddComponent<MeshRenderer>().material = mat;
        }


        
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10f, 10f, 90, 20), "Height");
        height = GUI.HorizontalSlider(new Rect(100, 15, 100, 20), height, 1f, 20f);

        GUI.Label(new Rect(10f, 30f, 90, 20), "Radius");
        pipeRad = GUI.HorizontalSlider(new Rect(100, 35, 100, 20), pipeRad, 0.5f, 5f);

        GUI.Label(new Rect(10f, 50f, 90, 20), "Angle");
        angle = GUI.HorizontalSlider(new Rect(100, 55, 100, 20), angle, 0, 90f);

        GUI.Label(new Rect(10f, 70f, 90, 20), "Segments");
        segments = (int)GUI.HorizontalSlider(new Rect(100, 75, 100, 20), segments, 3, 8);
        
        if(GUI.Button(new Rect(10f, 100f, 50, 20), "Save"))
        {
            if(model != null)
            {
                var path = Application.dataPath + "/../Outputs/model.3dm";
                model.Write(path, 5);
            }
        }
    }
}
