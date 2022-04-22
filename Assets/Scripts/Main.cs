using System.IO;
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
  public const int SamplingCountOneSide = 300;
  private readonly int _frameIndexShaderId = Shader.PropertyToID("_FrameIndex");
  private readonly int _lightSamplePosBufferId = Shader.PropertyToID("_LightSamplePosBuffer");
  private readonly int _samplingCountOneSideId = Shader.PropertyToID("_SamplingCountOneSide");

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
    if (camera == Camera.main) return;
    var outputTarget = RequireOutputTarget(camera);
    var outputTargetSize = RequireOutputTargetSize(camera);

    var accelerationStructure = _pipeline.RequestAccelerationStructure();
    var PRNGStates = _pipeline.RequirePRNGStates(camera);
    var lightSamplePosBuffer = ((mRayTracingRenderPipeline)_pipeline).RequireComputeBuffer(_lightSamplePosBufferId, LightSamplePos);
    var cmd = CommandBufferPool.Get(typeof(OutputColorTutorial).Name);
    try
    {
      if (_frameIndex < SamplingCountOneSide * SamplingCountOneSide)
      {
        using (new ProfilingSample(cmd, "RayTracing"))
        {
          cmd.SetRayTracingShaderPass(_shader, "RayTracing");
          cmd.SetRayTracingAccelerationStructure(_shader, _pipeline.accelerationStructureShaderId,
            accelerationStructure);
          cmd.SetRayTracingIntParam(_shader, _frameIndexShaderId, _frameIndex);
          cmd.SetRayTracingIntParam(_shader, _samplingCountOneSideId, SamplingCountOneSide);
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

        using (new ProfilingSample(cmd, "FinalBlit"))
        {
          cmd.Blit(outputTarget, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);

          if (_frameIndex % 100 == 0) {
            // RenderTexture outputRenderTexture = RenderTexture.active;
            // var scale = RTHandles.rtHandleProperties.rtHandleScale;
            // cmd.Blit(outputTarget, outputRenderTexture, new Vector2(scale.x, scale.y), Vector2.zero, 0, 0);

            Texture2D tex = new Texture2D(camera.pixelWidth, camera.pixelHeight, TextureFormat.RGB24, false);
            // // ReadPixels looks at the active RenderTexture.
            // RenderTexture.active = outputRenderTexture;
            tex.ReadPixels(new Rect(0, 0, camera.pixelWidth, camera.pixelHeight), 0, 0);
            tex.Apply();

            string path = Application.dataPath + "/" + _frameIndex + ".png";
            Debug.Log(path);
            File.WriteAllBytes(path, tex.EncodeToPNG());
          }
        }
      }

      context.ExecuteCommandBuffer(cmd);
    }
    finally
    {
      CommandBufferPool.Release(cmd);
    }
  }
 
}
