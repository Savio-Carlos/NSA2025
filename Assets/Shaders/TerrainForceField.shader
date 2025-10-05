Shader "Universal Render Pipeline/Effects/TerrainForceField"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.3, 0.6, 1.0, 1.0)
        _FadeHeight ("Fade Height", Float) = 40
        _ScrollSpeed ("Scroll Speed", Float) = 0.25
        _Intensity ("Intensity", Float) = 1
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend One OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "TerrainForceField"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 2.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float _FadeHeight;
                float _ScrollSpeed;
                float _Intensity;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float heightNorm : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionHCS = TransformWorldToHClip(positionWS);

                float baseHeight = TransformObjectToWorld(float3(0.0, -0.5, 0.0)).y;
                float fadeHeight = max(_FadeHeight, 0.0001);
                float relativeHeight = max(positionWS.y - baseHeight, 0.0);
                output.heightNorm = saturate(relativeHeight / fadeHeight);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float falloff = 1.0 - input.heightNorm;
                float wave = 1.0 - frac(input.heightNorm + _ScrollSpeed * _Time.y);
                float strength = _Intensity * falloff;
                float alpha = saturate(falloff * wave) * _BaseColor.a;
                float3 color = _BaseColor.rgb * strength * wave;
                return half4(color, alpha);
            }
            ENDHLSL
        }
    }

    FallBack Off
}
