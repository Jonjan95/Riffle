Shader "Creek/Water"
{
    Properties { _Color ("Water", Color) = (.16,.58,.60,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings { float4 positionCS : SV_POSITION; float3 positionWS : TEXCOORD0; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float time = _Time.y;
                float wave = sin(input.positionWS.x * 2.3 + input.positionWS.z * 3.4 + time * .8)
                           + sin(input.positionWS.z * 8.0 - input.positionWS.x * 1.7 + time * 1.3) * .35;
                half ribbon = smoothstep(.92, 1.25, wave) * .10;
                half3 color = _Color.rgb + half3(.11, .18, .14) * ribbon
                            + sin(input.positionWS.z * 2.5 + time * .65) * .024;
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
