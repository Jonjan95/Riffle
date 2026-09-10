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
                float2 p = input.positionWS.xz;
                // Broad flowing bands with a few quiet crest lines; no refraction or glitter noise.
                float bend = sin(p.x * .42 + time * .18) * .32;
                float current = p.x * .52 - time * .42 + sin(p.y * 2.2) * .30;
                float wave = sin(current) * .5 + .5;
                float crest = pow(saturate(sin(p.y * 7.0 + bend + sin(current) * .55)), 18);
                float broken = smoothstep(.15, .8, sin(p.x * 2.1 - time * .55 + p.y));
                half3 color = _Color.rgb * lerp(.92, 1.045, wave)
                            + half3(.13, .19, .17) * crest * broken * .26;
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
