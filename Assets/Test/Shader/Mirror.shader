﻿Shader "Test/Mirror"
{
  Properties
  {
  }
  SubShader
  {
    Tags { "RenderType"="Opaque" }
    LOD 100

    Pass
    {
      CGPROGRAM
      #pragma vertex vert
      #pragma fragment frag
      // make fog work
      #pragma multi_compile_fog

      #include "UnityCG.cginc"

      struct appdata
      {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
      };

      struct v2f
      {
        float3 normal : TEXCOORD0;
        UNITY_FOG_COORDS(1)
        float4 vertex : SV_POSITION;
      };

      CBUFFER_START(UnityPerMaterial)
      CBUFFER_END

      v2f vert (appdata v)
      {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.normal = UnityObjectToWorldNormal(v.normal);
        UNITY_TRANSFER_FOG(o, o.vertex);
        return o;
      }

      half4 frag (v2f i) : SV_Target
      {
        half4 col = half4(0.7, 0.7, 0.7, 1);
        // apply fog
        UNITY_APPLY_FOG(i.fogCoord, col);
        return col;
      }
      ENDCG
    }
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
      #include "../../GPU-Ray-Tracing-in-One-Weekend/src/Assets/Shaders/PRNG.hlsl"

      struct IntersectionVertex
      {
        // Object space normal of the vertex
        float3 normalOS;
      };

      CBUFFER_START(UnityPerMaterial)
      CBUFFER_END

      void FetchIntersectionVertex(uint vertexIndex, out IntersectionVertex outVertex)
      {
        outVertex.normalOS = UnityRayTracingFetchVertexAttribute3(vertexIndex, kVertexAttributeNormal);
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

        // Get normal in world space.
        float3 normalOS = INTERPOLATE_RAYTRACING_ATTRIBUTE(v0.normalOS, v1.normalOS, v2.normalOS, barycentricCoordinates);
        float3x3 objectToWorld = (float3x3)ObjectToWorld3x4();
        float3 normalWS = normalize(mul(objectToWorld, normalOS));

        float4 subColor = float4(0, 0, 0, 1);
        // Get next color
        if (rayIntersection.remainingDepth > 1)
        {
          // Get position in world space.
          float3 origin = WorldRayOrigin();
          float3 direction = WorldRayDirection();
          float t = RayTCurrent();
          float3 positionWS = origin + direction * t;

          // Make reflection ray.
          RayDesc rayDescriptor;
          rayDescriptor.Origin = positionWS + 0.001f * normalWS;
          rayDescriptor.Direction = reflect(-positionWS, normalWS);
          rayDescriptor.TMin = 1e-5f;
          rayDescriptor.TMax = _CameraFarDistance;

          // Tracing reflection.
          RayIntersection reflectionRayIntersection;
          reflectionRayIntersection.remainingDepth = rayIntersection.remainingDepth - 1;
          reflectionRayIntersection.PRNGStates = rayIntersection.PRNGStates;
          reflectionRayIntersection.color = float4(0.0f, 0.0f, 0.0f, 0.0f);
          reflectionRayIntersection.type = 0;

          TraceRay(_AccelerationStructure, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xFF, 0, 1, 0, rayDescriptor, reflectionRayIntersection);

          rayIntersection.PRNGStates = reflectionRayIntersection.PRNGStates;
          float r = reflectionRayIntersection.distance;
          if (r < 1) r = 1;
          subColor = reflectionRayIntersection.color / (r * r);
        }

        float4 lightColor = float4(0, 0, 0, 1);
        // shadow ray
        if (rayIntersection.remainingDepth > 0) {
          float3 origin = WorldRayOrigin();
          float3 direction = WorldRayDirection();
          float t = RayTCurrent();
          float3 positionWS = origin + direction * t;

          // Make reflection ray.
          RayDesc rayDescriptor;
          rayDescriptor.Origin = positionWS + 0.001f * normalWS;
          float3 lightPos = float3(0, 1.845, 0);
          rayDescriptor.Direction = lightPos - positionWS;
          rayDescriptor.TMin = 1e-5f;
          rayDescriptor.TMax = _CameraFarDistance;

          // Tracing reflection.
          RayIntersection shadowRayIntersection;
          shadowRayIntersection.remainingDepth = rayIntersection.remainingDepth - 1;
          shadowRayIntersection.PRNGStates = rayIntersection.PRNGStates;
          shadowRayIntersection.color = float4(0.0f, 0.0f, 0.0f, 0.0f);
          shadowRayIntersection.type = 0;

          TraceRay(_AccelerationStructure, RAY_FLAG_CULL_BACK_FACING_TRIANGLES, 0xFF, 0, 1, 0, rayDescriptor, shadowRayIntersection);
          if (shadowRayIntersection.type == 1) {
            float r = shadowRayIntersection.distance;
            if (r < 1) r = 1;
            lightColor = shadowRayIntersection.color / (r * r);
            lightColor.a = 1.0;
          }
          rayIntersection.PRNGStates = shadowRayIntersection.PRNGStates;
        }


        rayIntersection.color = subColor * lightColor;
        rayIntersection.type = 0;
        rayIntersection.distance = GetDistance();
      }

      ENDHLSL
    }
  }
}
