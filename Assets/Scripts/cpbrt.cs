using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;

public class cpbrt : MonoBehaviour
{
    string path, datas;

    struct Film
    {
        public int xresolution;
        public int yresolution;
        public string filename;
    }

    struct LookAt
    {
        public Vector3 eye;
        public Vector3 look;
        public Vector3 up;
    }

    struct Camera
    {
        public string projection;
        public float fov;
    }

    struct Area
    {
        public float height;
        public float width;
        public int nsamples;
    }

    enum LIGHTSOURCE
    {
        DIRECTIONAL,
        POINT,
        AREA
    }

    enum Shape
    {
        SPHERE,
        CYLINDER,
        CONE,
        PLANE,
    }

    struct Material
    {
        public bool mirror;
        public Vector3 diffuse;
        public Vector3 specular;
    }

    Film film;
    int pixelsamples;
    LookAt lookat;
    Camera camera_self;
    Area area;
    LIGHTSOURCE type = new LIGHTSOURCE();

    public UnityEngine.Camera camera;
    public UnityEngine.Light light = new Light();

    public void Load()
    {
        path = EditorUtility.OpenFilePanel("Overwrite with cpbrt", "", "cpbrt");

        if (!File.Exists(path))
            return;
        else
            Debug.Log("The file has been load.");

        datas = File.ReadAllText(path);

        string[] lines = datas.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

        char[] charactors = new char[] { '\n', ' ', '\"', '[', ']', ':', '\t', '#' };

        string[][] words = new string[lines.Length][];

        for (int i = 0; i < lines.Length; ++i)
            words[i] = lines[i].Split(charactors, StringSplitOptions.RemoveEmptyEntries);

        //StreamWriter sw = new StreamWriter("TestFile.txt");
        //
        //foreach (var line in words)
        //{
        //    foreach (var word in line)
        //        sw.Write(word + '\n');
        //    sw.Write('\n');
        //}

        int index = 0;

        for (int i = 0; i < lines.Length; ++i)
        {
            switch (words[i][0])
            {
                case "Film":
                    index = find(words[i + 2], "xresolution") + 1;
                    film.xresolution = int.Parse(words[i + 2][index]);

                    index = find(words[i + 2], "yresolution") + 1;
                    film.yresolution = int.Parse(words[i + 2][index]);

                    index = find(words[i + 3], "filename") + 1;
                    film.filename = words[i + 3][index];
                    //Debug.Log(film.xresolution + ' ' + film.yresolution + ' ' + film.filename);
                    break;

                case "Sampler":
                    index = find(words[i], "pixelsamples") + 1;
                    pixelsamples = int.Parse(words[i][index]);
                    break;

                case "LookAt":
                    lookat.eye = new Vector3(float.Parse(words[i][1]), float.Parse(words[i][2]), float.Parse(words[i][3]));
                    lookat.look = new Vector3(float.Parse(words[i][4]), float.Parse(words[i][5]), float.Parse(words[i][6]));
                    lookat.up = new Vector3(float.Parse(words[i][7]), float.Parse(words[i][8]), float.Parse(words[i][9]));
                    camera.transform.position = lookat.eye;
                    camera.transform.LookAt(lookat.look, lookat.up);
                    break;

                case "Camera":
                    camera_self.projection = words[i][2];
                    camera_self.fov = float.Parse(words[i][find(words[i], "fov") + 1]);

                    camera.fieldOfView = camera_self.fov;
                    break;

                case "WorldBegin":
                    for (; i < lines.Length; ++i)
                        if (words[i][0] == "AttributeBegin")
                            getAttribute(words, i);
                    break;

                default:
                    break;
            }
        }
    }

    private int find(string[] line, string word)
    {
        for (int i = 0; i < line.Length; ++i)
            if (line[i] == word)
                return i;       

        return -1;
    }

    private void getAttribute(string[][] words, int index)
    {
        GameObject gameObject;
        var word = words[index - 1].Length - 1;
        var obj = words[index - 1][word];
        var line = words[index + 1];
        switch (obj)
        {
            case "light":                
                int i = find(line, "LightSource");

                if (line[i + 1] == "point")
                {
                    light.type = LightType.Point;
                    type = LIGHTSOURCE.POINT;
                }
                else if (line[i + 1] == "area")
                {
                    area = new Area();

                    type = LIGHTSOURCE.AREA;
                    light.type = LightType.Area;

                    i = find(line, "nsamples"); 
                    light.range = int.Parse(line[i + 1]);

                    i = find(line, "height");
                    int j = find(line, "width");
                    light.areaSize = new Vector2(float.Parse(line[i + 1]), float.Parse(line[j + 1]));
                }

                i = find(line, "color");
                light.color = new Color(int.Parse(line[i + 2]), int.Parse(line[i + 3]), int.Parse(line[i + 4]));

                i = find(line, "from");
                light.transform.position = new Vector3(float.Parse(line[i + 1]), float.Parse(line[i + 2]), float.Parse(line[i + 3]));
                break;

            case "sphere":
                gameObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                gameObject.name = "Sphere";
                getObject(words, index + 1, gameObject);
                break;

            case "cylinder":
                gameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                gameObject.name = "Cylinder";
                getObject(words, index + 1, gameObject);
                break;

            case "cone":
                gameObject = new GameObject();
                gameObject.AddComponent<MeshFilter>();
                gameObject.GetComponent<MeshFilter>().mesh = CreateCone();
                gameObject.GetComponent<MeshFilter>().mesh.name = "Cone";
                
                gameObject.AddComponent<MeshRenderer>();
                gameObject.GetComponent<MeshRenderer>().material = new UnityEngine.Material(Shader.Find("Standard"));

                gameObject.name = "Cone";
                getObject(words, index + 1, gameObject);
                break;

            case "plane":
                gameObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
                gameObject.name = "Plane";
                getObject(words, index + 1, gameObject);
                break;

            case "obj":
                gameObject = new GameObject();
                gameObject.AddComponent<MeshFilter>();
                gameObject.AddComponent<MeshRenderer>();
                gameObject.name = "Model";
                getObject(words, index + 1, gameObject);
                break;
        }
    }

    private void getObject(string[][] words, int index, GameObject gameObject)
    {
        while (words[index][0] != "AttributeEnd")
        {
            var line = words[index];
            switch (line[0])
            {
                case "Translate":
                    gameObject.transform.position = new Vector3(float.Parse(line[1]), float.Parse(line[2]), float.Parse(line[3]));
                    break;

                case "Rotate":
                    Quaternion angle = new Quaternion();
                    if (line[2] == "1")
                        angle.eulerAngles = new Vector3(float.Parse(line[1]), 0, 0);
                    if (line[3] == "1")
                        angle.eulerAngles = new Vector3(0, float.Parse(line[1]), 0);
                    if (line[4] == "1")
                        angle.eulerAngles = new Vector3(0, 0, float.Parse(line[1]));

                    gameObject.transform.rotation = angle;
                    break;

                case "Material":
                    //gameObject.GetComponent<MeshRenderer>().material = new Material( Shader.Find("Standard"));
                    
                    break;
                case "Shape":
                    int i = find(line, "radius");
                    if (i != -1)
                        gameObject.transform.localScale = new Vector3(float.Parse(line[i + 1]), float.Parse(line[i + 1]), float.Parse(line[i + 1]));

                    i = find(line, "height");
                    if (i != -1)
                        gameObject.transform.localScale = new Vector3(float.Parse(line[i + 1]), gameObject.transform.localScale.y, gameObject.transform.localScale.z);

                    i = find(line, "width");
                    if (i != -1)
                        gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, gameObject.transform.localScale.y, float.Parse(line[i + 1]));

                    break;

                case "Scale":
                    break;

                case "Include":
                    break;

                default:
                    Debug.Log(line[0]);
                    break;
            }

            index++;
        }
    }

    private Mesh CreateCone()
    {
        Mesh mesh = new Mesh() ;

        float height = 1.0f;
        float radius = 0.5f;
        int segment = 36;

        Vector3 pos;

        float angle = 0.0f;
        float angleAmount = 0.0f;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        mesh = new Mesh();

        angleAmount = 2 * Mathf.PI / segment;

        pos = new Vector3(0.0f, height, 0.0f);
        vertices.Add(pos);

        pos = new Vector3(0.0f, 0.0f, 0.0f);
        vertices.Add(pos);

        for (int i = 0; i < segment; ++i)
        {
            pos.x = radius * Mathf.Sin(angle);
            pos.z = radius * Mathf.Cos(angle);

            vertices.Add(pos);

            angle -= angleAmount;
        }

        mesh.vertices = vertices.ToArray();

        for (int i = 2; i < segment + 1; ++i)
        {
            triangles.Add(0);
            triangles.Add(i + 1);
            triangles.Add(i);
        }

        triangles.Add(0);
        triangles.Add(2);
        triangles.Add(segment + 1);

        for (int i = segment + 1; i > 2; --i)
        {
            triangles.Add(1);
            triangles.Add(i - 1);
            triangles.Add(i);
        }

        triangles.Add(1);
        triangles.Add(segment + 1);
        triangles.Add(2);

        mesh.triangles = triangles.ToArray();

        return mesh;
    }
}
