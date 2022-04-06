using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// the ray tracing render pipeline asset.
/// </summary>
[CreateAssetMenu(fileName = "mRayTracingRenderPipelineAsset", menuName = "Rendering/mRayTracingRenderPipelineAsset", order = -1)]
public class mRayTracingRenderPipelineAsset : RayTracingRenderPipelineAsset
{
  protected override RenderPipeline CreatePipeline()
  {
    return new mRayTracingRenderPipeline(this);
  }
}
