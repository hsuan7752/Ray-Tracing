Shader "Test/Silhouette"
{
  Properties
  {
    _Color ("Main Color", Color) = (1, 1, 1, 1)
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
      half4 _Color;
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
        half4 col = _Color;
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
      float4 _Color;
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

        float3 direction = WorldRayDirection();
        rayIntersection.color = _Color;
        rayIntersection.type = 0;
        rayIntersection.distance = GetDistance();
        if (abs(dot(normalWS, -direction.xyz)) < 0.15 ) {
          rayIntersection.color = float4(1, 1, 1, 1);
        }
      }

      ENDHLSL
    }
  }
}
