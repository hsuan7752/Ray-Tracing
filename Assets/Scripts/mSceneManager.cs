using UnityEngine;
using UnityEngine.Experimental.Rendering;

/// <summary>
/// the scene manager.
/// </summary>
public class mSceneManager : SceneManager
{
    public void Awake()
    {
        isDirty = true;
    }
   public void AddRenderers(MeshRenderer meshRenderer)
    {
        Renderer[] temp = new Renderer[renderers.Length + 1];
        renderers.CopyTo(temp, 0);
        temp[renderers.Length] = meshRenderer;
        renderers = temp;
    }
}
