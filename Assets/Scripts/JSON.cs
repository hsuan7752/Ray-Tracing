using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class JSON : MonoBehaviour
{
    [Header("File Path")]
    static private string path = "D:\\3D Computer Game (2)\\Project 1 Ray Tracing\\Assets\\Files";

    static Data[] datas;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("The file has been start.");
        //path = Application.dataPath;

        Load();
    }

    public Data[] Load()
    {
        Debug.Log("The file has been load.");
        if (!File.Exists(path + "/record.json"))
            return null;
        string jsonInfo = File.ReadAllText(path + "/record.json");
        datas = JsonHelper.FromJson<Data>(jsonInfo);

        return datas;
    }

    static public void addData(Vector3 l_pos, Quaternion l_rot, Vector3 c_pos, Quaternion c_rot)
    {
        Data newData = new Data
        {
            lightPosition = l_pos,
            lightRotation = l_rot,
            cameraPosition = c_pos,
            cameraRotation = c_rot
        };

        datas = AddtoArray(datas, newData);
    }

    static public void Save()
    {
        string jsonInfo = JsonHelper.ToJson(datas, true);

        Debug.Log(jsonInfo);

        File.WriteAllText(path + "/record.json", jsonInfo);

        Debug.Log("The file has been writen.");
    }

    static public T[] AddtoArray<T>(T[] originalData, T addValue)
    {
        if (originalData == null)
        {
            originalData = new T[1];
            originalData[0] = addValue;
            return originalData;
        }

        T[] newData = new T[originalData.Length + 1];
        originalData.CopyTo(newData, 0);
        newData[originalData.Length] = addValue;
        return newData;
    }

    public void getPath()
    {
        path = EditorUtility.OpenFilePanel("Overwrite with json", "", "json");
        Debug.Log(path);
    }
}

[System.Serializable]
public class Data
{
    public Vector3 lightPosition;
    public Quaternion lightRotation;


    public Vector3 cameraPosition;
    public Quaternion cameraRotation;
}
