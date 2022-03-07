Shader "RayTracing/AO"
{
    Properties
    {
      _MaxLength ("MaxLength", float) = 1
      _Normal ("Normal Map (RGB)", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        struct Input
        {
            float2 uv_MainTex;
        };

        float _MaxLength;


        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            o.Albedo = float4(1, 1, 1, 1);
        }
        ENDCG
    }
    SubShader
  {
    Pass
    {
      Name "RayTracing"
      Tags { "LightMode" = "RayTracing" }

      HLSLPROGRAM

      #pragma raytracing test

      #include "./Common.hlsl"
      #include "../GPU-Ray-Tracing-in-One-Weekend/src/Assets/Shaders/PRNG.hlsl"

      struct IntersectionVertex
      {
        // Object space normal of the vertex
        float3 normalOS;
        float2 uv0;
      };

      CBUFFER_START(UnityPerMaterial)
      float _MaxLength;
      Texture2D<float4> _Normal;
      SamplerState sampler_Normal
      {
          Filter = MIN_MAG_MIP_POINT;
          AddressU = Wrap;
          AddressV = Wrap;
      };
      CBUFFER_END

      void FetchIntersectionVertex(uint vertexIndex, out IntersectionVertex outVertex)
      {
        outVertex.normalOS = UnityRayTracingFetchVertexAttribute3(vertexIndex, kVertexAttributeNormal);
        outVertex.uv0 = UnityRayTracingFetchVertexAttribute3(vertexIndex, kVertexAttributeTexCoord0);
      }

      [shader("closesthit")]
      void ClosestHitShader(inout RayIntersection rayIntersection : SV_RayPayload, AttributeData attributeData : SV_IntersectionAttributes)
      {
        // Fetch the indices of the currentr triangle
        uint3 triangleIndices = UnityRayTracingFetchTriangleIndices(PrimitiveIndex());

        // Fetch the 3 vertices
        IntersectionVertex v0, v1, v2;
        FetchIntersectionVertex(triangleIndices.x, v0);
        FetchIntersectionVertex(triangleIndices.y, v1);
        FetchIntersectionVertex(triangleIndices.z, v2);

        // Compute the full barycentric coordinates
        float3 barycentricCoordinates = float3(1.0 - attributeData.barycentrics.x - attributeData.barycentrics.y, attributeData.barycentrics.x, attributeData.barycentrics.y);

        // get attribute in vertex
        float2 uv0 = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.uv0, v1.uv0, v2.uv0, barycentricCoordinates);
        float3 normalOS = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.normalOS, v1.normalOS, v2.normalOS, barycentricCoordinates);
        float3 normalMapOS = _Normal.SampleLevel(sampler_Normal, uv0, 0).xyz * 2 - float3(1, 1, 1);
        normalOS = normalize(normalMapOS + normalOS);
        
        // Get normal in world space.
        float3x3 objectToWorld = (float3x3)ObjectToWorld3x4();
        float3 normalWS = normalize(mul(objectToWorld, normalOS));

        rayIntersection.color = float4(0, 1, 0, 1);
        if (rayIntersection.remainingDepth > 0) 
        {
          rayIntersection.color = float4(0, 0, 0, 1);
          // Get position in world space.
          float3 origin = WorldRayOrigin();
          float3 direction = WorldRayDirection();
          float t = RayTCurrent();
          float3 positionWS = origin + direction * t;

          // Make reflection ray.
          RayDesc rayDescriptor;
          rayDescriptor.Origin = positionWS + 0.001f * normalWS;
          rayDescriptor.Direction = normalWS + GetRandomOnUnitSphere(rayIntersection.PRNGStates);
          rayDescriptor.TMin = 1e-5f;
          rayDescriptor.TMax = _MaxLength;

          // Tracing reflection.
          RayIntersection reflectionRayIntersection;
          reflectionRayIntersection.remainingDepth = 0;
          reflectionRayIntersection.PRNGStates = rayIntersection.PRNGStates;
          reflectionRayIntersection.color = float4(0.0f, 0.0f, 0.0f, 0.0f);
          reflectionRayIntersection.type = 0;

          TraceRay(_AccelerationStructure, RAY_FLAG_NONE, 0xFF, 0, 1, 0, rayDescriptor, reflectionRayIntersection);
          float r = reflectionRayIntersection.distance;
          if (reflectionRayIntersection.type == 2) rayIntersection.color = float4(0, 0, 0, 1);
          else rayIntersection.color = float4(1, 1, 1, 1);
          rayIntersection.PRNGStates = reflectionRayIntersection.PRNGStates;
        }
        rayIntersection.type = 2;
        rayIntersection.distance = GetDistance();
      }

      ENDHLSL
    }
  }
    FallBack "Diffuse"
}
