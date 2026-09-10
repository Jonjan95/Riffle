Shader "Creek/PanSurface"
{
    Properties { _Color ("Color", Color) = (1,1,1,1) _ReceiveShadows ("Shadow softness", Range(0,1)) = 1 _Sheen ("Broad satin highlight", Range(0,0.25)) = 0 }
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
                float4 _Color; float _ReceiveShadows; float _Sheen;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; float3 normalOS : NORMAL; float4 color : COLOR; };
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0; half4 tint : COLOR;
                float4 shadowCoord : TEXCOORD1; float3 positionWS : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS; output.positionWS = positionInputs.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS); output.tint = input.color;
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
                half light = (.55h + bands * lerp(.65h, 1.0h, lerp(1.0h, mainLight.shadowAttenuation, _ReceiveShadows)))
                           * mainLight.distanceAttenuation;
                half3 halfDir = normalize(mainLight.direction + GetWorldSpaceNormalizeViewDir(input.positionWS));
                half satin = pow(saturate(dot(normalize(input.normalWS), halfDir)), 18.0h) * _Sheen;
                return half4(_Color.rgb * input.tint.rgb * light * half3(1.0h, .975h, .92h) + satin * half3(1.0h, .94h, .78h), 1);
            }
            ENDHLSL
        }
        UsePass "Universal Render Pipeline/Lit/ShadowCaster"
        UsePass "Universal Render Pipeline/Lit/DepthOnly"
    }
}
