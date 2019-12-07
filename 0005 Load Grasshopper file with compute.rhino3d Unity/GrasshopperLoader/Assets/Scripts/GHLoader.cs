using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using GH_IO;
using GH_IO.Serialization;
using Newtonsoft.Json;
using RestSharp;

public class GHLoader : MonoBehaviour
{
    public string ghFile = "";
    public string serviceUrl = "http://localhost:8081";
    public GameObject meshObjPrefab;

    private string encodedData = "";
    private List<GHInput> ghInputs = new List<GHInput>();
    private bool loaded = false;
    private List<Rhino.Geometry.Mesh> meshes = new List<Rhino.Geometry.Mesh>();
    private GH_Archive archive;
    private RestClient client;

    // Start is called before the first frame update
    void Start()
    {
        client = new RestClient(serviceUrl);

        archive = new GH_Archive();
        archive.ReadFromFile(Application.streamingAssetsPath + "\\" + ghFile);

        var root = archive.GetRootNode;
        var def = root.FindChunk("Definition") as GH_Chunk;
        var objs = def.FindChunk("DefinitionObjects") as GH_Chunk;

        if(objs != null)
        {
            int count = objs.GetInt32("ObjectCount");

            var inputGuids = new List<Guid>();
            var inputNames = new List<string>();

            for(int i=0; i<count; i++)
            {
                var obj = objs.FindChunk("Object", i) as GH_Chunk;
                var container = obj.FindChunk("Container") as GH_Chunk;

                var name = container.GetString("Name");
                if(name == "Group")
                {
                    var nickname = container.GetString("NickName");
                    if(nickname.IndexOf("RH_IN:") != -1)
                    {
                        var inputname = nickname.Replace("RH_IN:", "");
                        var itemguid = container.GetGuid("ID", 0);
                        inputNames.Add(inputname);
                        inputGuids.Add(itemguid);
                    }
                }
            }

            for (int i = 0; i < count; i++)
            {
                var obj = objs.FindChunk("Object", i) as GH_Chunk;
                var container = obj.FindChunk("Container") as GH_Chunk;

                var instanceguid = container.GetGuid("InstanceGuid");
                if (inputGuids.Contains(instanceguid))
                {
                    var index = inputGuids.IndexOf(instanceguid);
                    var inputName = inputNames[index];
                    var componetName = container.GetString("Name");
                    object value = null;
                    switch (componetName) {
                        case ("Point"):
                            value = new float[] { 0.5f, 0.5f, 0.5f };
                            break;
                        case ("Number"):
                            value = 0.5f;
                            break;
                        case ("Integer"):
                            value = 10;
                            break;
                        default:
                            break;
                     
                    }

                    var ghInput = new GHInput(componetName, inputName, value);
                    ghInputs.Add(ghInput);
                }
            }
        }

        SendGHData();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMesh();
    }

    private void UpdateMesh()
    {
        if(loaded && meshes.Count > 0)
        {
            var objs = GameObject.FindGameObjectsWithTag("Object");
            foreach( var obj in objs)
            {
                Destroy(obj.GetComponent<MeshFilter>().mesh);
                Destroy(obj.GetComponent<MeshRenderer>().material);
                Destroy(obj);
            }
            Resources.UnloadUnusedAssets();

            foreach(var mesh in meshes)
            {
                var vertices = mesh.Vertices.ToList().ConvertAll(new Converter<Rhino.Geometry.Point3f, Vector3>(Point3fToVector3)).ToArray();
                var normals = mesh.Normals.ToList().ConvertAll(new Converter<Rhino.Geometry.Vector3f, Vector3>(Vector3fToVector3)).ToArray();
                var triangleList = new List<int>();
                foreach(var face in mesh.Faces)
                {
                    if (face.IsTriangle)
                    {
                        triangleList.Add(face.A);
                        triangleList.Add(face.B);
                        triangleList.Add(face.C);

                        triangleList.Add(face.C);
                        triangleList.Add(face.B);
                        triangleList.Add(face.A);
                    }
                    else
                    {
                        triangleList.Add(face.A);
                        triangleList.Add(face.B);
                        triangleList.Add(face.C);

                        triangleList.Add(face.C);
                        triangleList.Add(face.D);
                        triangleList.Add(face.A);

                        triangleList.Add(face.C);
                        triangleList.Add(face.B);
                        triangleList.Add(face.A);

                        triangleList.Add(face.A);
                        triangleList.Add(face.D);
                        triangleList.Add(face.C);
                    }
                }

                var gb = (GameObject)Instantiate(meshObjPrefab);
                var meshFilter = gb.GetComponent<MeshFilter>();
                var unityMesh = new Mesh();
                unityMesh.vertices = vertices;
                unityMesh.normals = normals;
                unityMesh.triangles = triangleList.ToArray();

                meshFilter.mesh = unityMesh;
            }

            loaded = false;
        }
    }

    public static Vector3 Point3fToVector3(Rhino.Geometry.Point3f pf)
    {
        return new Vector3(pf.X, pf.Z, pf.Y);
    }
    
    public static Vector3 Vector3fToVector3(Rhino.Geometry.Vector3f pf)
    {
        return new Vector3(pf.X, pf.Z, pf.Y);
    }


    private void SendGHData()
    {
        if(loaded == false)
        {
            loaded = true;

            meshes.Clear();

            var request = new RestRequest("/grasshopper/", Method.POST);

            var json = ParseToJson();

            request.AddParameter("application/json; charset=utf-8", json, ParameterType.RequestBody);
            request.RequestFormat = DataFormat.Json;

            try
            {
                client.ExecuteAsync(request, response =>
                {
                    if(response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var ghOut = JsonConvert.DeserializeObject<GHData>(response.Content);
                        foreach(var ghOutVal in ghOut.values)
                        {
                            foreach(var set in ghOutVal.InnerTree)
                            {
                                var ghInnerVals = set.Value;
                                foreach(var ghInnerVal in ghInnerVals)
                                {
                                    if(ghInnerVal.type == "Rhino.Geometry.Mesh")
                                    {
                                        var mesh = JsonConvert.DeserializeObject<Rhino.Geometry.Mesh>(ghInnerVal.data);
                                        meshes.Add(mesh);

                                    }
                                }
                            }
                        }
                        if(meshes.Count == 0)
                        {
                            loaded = false;
                        }
                    }
                });
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }
    }

    private string ParseToJson()
    {
        var bytes = archive.Serialize_Binary();
        encodedData = Convert.ToBase64String(bytes, Base64FormattingOptions.InsertLineBreaks);


        var ghData = new GHData();
        ghData.algo = encodedData;
        ghData.values = GHInput.ToGHValueList(ghInputs);

        var json = JsonConvert.SerializeObject(ghData);

        return json;
    }

    private void OnGUI()
    {
        for(int i=0; i<ghInputs.Count; i++)
        {
            var ghInput = ghInputs[i];

            int offset = 25;
            int width = 150;

            GUI.Label(new Rect(offset, offset + 60 * i, width, 30), ghInput.label);
            switch (ghInput.componentName)
            {
                case "Point":
                    if(ghInput.value == null)
                    {
                        ghInput.value = new float[3];
                        for(int n=0; n< 3; n++)
                        {
                            ((float[])ghInput.value)[n] = 0.5f;
                        }
                    }
                    for(int n=0; n<3; n++)
                    {
                        var sliderVal = GUI.HorizontalSlider(new Rect(offset + ((width-10)/3f +5) *n, 25+30+60*i, (width-10)/3f, 30), ((float[])ghInput.value)[n], 0f, 1.0f);
                        if(((float[])ghInput.value)[n] !=sliderVal){
                            ((float[])ghInput.value)[n] = sliderVal;
                            SendGHData();
                        }
                    }
                    break;
                case "Number":
                    if(ghInput.value == null) { ghInput.value = 0.5f; }
                    var sFVal = GUI.HorizontalSlider(new Rect(offset, offset + 30 + 60 * i, width, 30), (float)ghInput.value, 0f, 1f);
                    if ((float)ghInput.value != sFVal)
                    {
                        ghInput.value = sFVal;
                        SendGHData();
                    }
                    break;
                case "Integer":
                    if (ghInput.value == null) { ghInput.value = 10; }
                    var sIVal = (int)GUI.HorizontalSlider(new Rect(offset, offset + 30 + 60 * i, width, 30), (int)ghInput.value, 0, 100);
                    if ((int)ghInput.value != sIVal)
                    {
                        ghInput.value = sIVal;
                        SendGHData();
                    }
                    break;
                default:
                    continue;
            }

        }
    }
}

public class GHInput
{
    public string componentName { get; set; }
    public string label { get; set; }
    public object value { get; set; }

    public GHInput(string _componentName, string _label, object _value)
    {
        label = _label;
        componentName = _componentName;
        value = _value;
    }

    public GHValue ToGHValue()
    {
        switch (componentName)
        {
            case "Point":
                var array = (float[])value;
                return new GHValue("RH_IN:" + label, new GHInnerValue("Rhino.Geometry.Point3d", "{\"X\":" + array[0] +",\"Y\":" + array[1] + ",\"Z\":" + array[2] + "}"));
            case "Number":
                return new GHValue("RH_IN:" + label, new GHInnerValue("System.Double", Convert.ToSingle(value).ToString()));
            case "Integer":
                return new GHValue("RH_IN:" + label, new GHInnerValue("System.Integer", Convert.ToInt32(value).ToString()));
            default:
                return null;
        }
    }

    public static List<GHValue> ToGHValueList(List<GHInput> ghInputList)
    {
        var ghValueList = new List<GHValue>();

        for(int i = 0; i<ghInputList.Count; i++)
        {
            ghValueList.Add(ghInputList[i].ToGHValue());
        }

        return ghValueList;
    }
}

public class GHData
{
    public string algo { get; set; }
    public string pointer { get; set; }
    public List<GHValue> values { get; set; }

    public GHData()
    {
        values = new List<GHValue>();
    }
}

public class GHValue
{
    public string ParamName { get; set; }
    public Dictionary<string, List<GHInnerValue>> InnerTree { get; set; }

    public GHValue()
    {
        InnerTree = new Dictionary<string, List<GHInnerValue>>();
    }

    public GHValue(string _ParamName, GHInnerValue innerValue)
    {
        ParamName = _ParamName;
        InnerTree = new Dictionary<string, List<GHInnerValue>>();
        InnerTree.Add("{0; }", new List<GHInnerValue> { innerValue });
    }
}

public class GHInnerValue
{
    public string type { get; set; }
    public string data { get; set; }

    public GHInnerValue()
    {

    }

    public GHInnerValue(string _type, string _data)
    {
        data = _data;
        type = _type;
    }
}