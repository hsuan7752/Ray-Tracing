using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// the output color tutorial asset.
/// </summary>
[CreateAssetMenu(fileName = "TestAsset", menuName = "Rendering/TestAsset")]
public class TestAsset : RayTracingTutorialAsset
{
  /// <summary>
  /// create tutorial.
  /// </summary>
  /// <returns>the tutorial.</returns>
  public List<Vector3> LightSamplePos = new List<Vector3>();
  public override RayTracingTutorial CreateTutorial()
  {
    LightSamplePos.Clear();
    if (Sampling.position != null) {
      foreach (Vector3 pos in Sampling.position)
        LightSamplePos.Add(pos);
    }

    // for corneil box
    if (LightSamplePos.Count == 0) {
      for (float x=-0.5f; x<=0.5f; x+=0.01f) {
        for (float z=-0.5f; z<=0.5f; z+=0.01f) {
          LightSamplePos.Add(new Vector3(x, 1.845f, z));
        }
      }
    }
    // for church
    // for (float x=-5f; x<=20f; x+=1.3f) {
    //   for (float z=-2.5f; z<=2.5f; z+=0.8f) {
    //     LightSamplePos.Add(new Vector3(x, 1.8f, z));
    //   }
    // }

    return new Test(this);
  }
}
