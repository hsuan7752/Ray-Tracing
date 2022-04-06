using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// the tutorial mono behaviour.
/// </summary>
public class mTutorialMono : TutorialMono
{
 
  static public bool loadModel = false;

  /// <summary>
  /// Unity Start.
  /// </summary>
  public IEnumerator Start()
  {
    yield return new WaitUntil(() => { return loadModel; });
    StartCoroutine(base.Start());
  }
}