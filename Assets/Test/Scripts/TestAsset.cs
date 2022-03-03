using UnityEngine;

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
  public override RayTracingTutorial CreateTutorial()
  {
    return new Test(this);
  }
}
