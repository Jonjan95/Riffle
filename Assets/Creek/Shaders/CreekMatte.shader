Shader "Creek/Matte"
{
    Properties { _Color ("Color", Color) = (1,1,1,1) _ReceiveShadows ("Shadow softness", Range(0,1)) = 1 }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color; float _ReceiveShadows;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float4 shadowCoord : TEXCOORD1;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.shadowCoord = GetShadowCoord(positionInputs);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                Light mainLight = GetMainLight(input.shadowCoord);
                half ndotl = dot(normalize(input.normalWS), mainLight.direction);
                half bands = smoothstep(-.18h, .12h, ndotl) * .18h
                           + smoothstep(.26h, .48h, ndotl) * .22h
                           + smoothstep(.68h, .86h, ndotl) * .14h;
                half light = (.55h + bands * lerp(.48h, 1.0h, lerp(1.0h, mainLight.shadowAttenuation, _ReceiveShadows)))
                           * mainLight.distanceAttenuation;
                return half4(_Color.rgb * light * half3(1.0h, .94h, .80h), 1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
