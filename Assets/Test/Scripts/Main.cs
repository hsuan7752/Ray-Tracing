using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;

/// <summary>
/// the output color tutorial.
/// </summary>
public class Test : RayTracingTutorial
{
  private readonly int _PRNGStatesShaderId = Shader.PropertyToID("_PRNGStates");

  /// <summary>
  /// the frame index.
  /// </summary>
  private int _frameIndex = 0;
  private readonly int _frameIndexShaderId = Shader.PropertyToID("_FrameIndex");
  private readonly int _lightSamplePosBufferId = Shader.PropertyToID("_LightSamplePosBuffer");

  public List<Vector3> LightSamplePos;
  /// <summary>
  /// constructor.
  /// </summary>
  /// <param name="asset">the tutorial asset.</param>
  public Test(TestAsset asset) : base(asset)
  {
    LightSamplePos = asset.LightSamplePos;
  }

  public override void Render(ScriptableRenderContext context, Camera camera)
  {
    base.Render(context, camera);
    var outputTarget = RequireOutputTarget(camera);
    var outputTargetSize = RequireOutputTargetSize(camera);

    var accelerationStructure = _pipeline.RequestAccelerationStructure();
    var PRNGStates = _pipeline.RequirePRNGStates(camera);
    var lightSamplePosBuffer = ((mRayTracingRenderPipeline)_pipeline).RequireComputeBuffer(_lightSamplePosBufferId, LightSamplePos);
    var cmd = CommandBufferPool.Get(typeof(OutputColorTutorial).Name);
    try
    {
      if (_frameIndex < 1000)
      {
        using (new ProfilingSample(cmd, "RayTracing"))
        {
          cmd.SetRayTracingShaderPass(_shader, "RayTracing");
          cmd.SetRayTracingAccelerationStructure(_shader, _pipeline.accelerationStructureShaderId,
            accelerationStructure);
          cmd.SetRayTracingIntParam(_shader, _frameIndexShaderId, _frameIndex);
          cmd.SetRayTracingBufferParam(_shader, _PRNGStatesShaderId, PRNGStates);
          cmd.SetRayTracingTextureParam(_shader, _outputTargetShaderId, outputTarget);
          cmd.SetRayTracingVectorParam(_shader, _outputTargetSizeShaderId, outputTargetSize);
          cmd.SetGlobalBuffer(_lightSamplePosBufferId, lightSamplePosBuffer);
          cmd.DispatchRays(_shader, "AntialiasingRayGenShader", (uint) outputTarget.rt.width,
            (uint) outputTarget.rt.height, 1, camera);
        }

        context.ExecuteCommandBuffer(cmd);
        if (camera.cameraType == CameraType.Game)
          _frameIndex++;
      }

      using (new ProfilingSample(cmd, "FinalBlit"))
      {
        cmd.Blit(outputTarget, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
      }

      context.ExecuteCommandBuffer(cmd);
    }
    finally
    {
      CommandBufferPool.Release(cmd);
    }
  }
 
}
