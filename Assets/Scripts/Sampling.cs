using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Sampling : MonoBehaviour
{
    public enum SAMPLING
    {
        INITIAL,
        MONTE_CARLO,
        IMPORTANCE,
        MULTIPLE_IMPORTANCE,
    }

    [Header("Sampling")]
    public SAMPLING method;
    static string path;

    static public Vector3[] position;

    private void Start()
    {
        Debug.Log("Sampling Load File");
        path = Application.dataPath;
        switch (method)
        {
            case SAMPLING.INITIAL:
                if (!File.Exists(path + "/initial.json")) {
                    Debug.LogWarning("light sampling points not found: " + path + "/initial.json");
                    return;
                } else {
                    Debug.Log("The file Monte Carlo has been load.");
                    path = path + "/initial.json";
                }
                break;

            case SAMPLING.MONTE_CARLO:
                if (!File.Exists(path + "/monte_carlo.json"))
                    return;
                else
                {
                    Debug.Log("The file Monte Carlo has been load.");
                    path = path + "/monte_carlo.json";
                }
                break;

            case SAMPLING.IMPORTANCE:
                if (!File.Exists(path + "/importance.json"))
                    return;
                else
                {
                    Debug.Log("The file Monte Carlo has been load.");
                    path = path + "/importance.json";
                }
                break;

            case SAMPLING.MULTIPLE_IMPORTANCE:
                if (!File.Exists(path + "/multiple_importance.json"))
                    return;
                else
                {
                    Debug.Log("The file Monte Carlo has been load.");
                    path = path + "/multiple_importance.json";
                }
                break;
        }

        string jsonInfo = File.ReadAllText(path);
        position = JsonHelper.FromJson<Vector3>(jsonInfo);
    }

    static public void Save()
    {
        string jsonInfo = JsonHelper.ToJson(position, true);
        File.WriteAllText(path + "/initial.json", jsonInfo);
    }

    static public void AddPosition(Vector3 pos)
    {
        if (position == null)
            position = new Vector3[] { pos };
        else
        {
            Vector3[] temp = new Vector3[position.Length + 1];
            position.CopyTo(temp, 0);
            temp[position.Length] = pos;
            position = temp;
        }        
    }
}
