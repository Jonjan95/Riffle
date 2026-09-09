Shader "Creek/Matte" {
 Properties { _Color ("Color", Color) = (1,1,1,1) }
 SubShader {
  Tags { "RenderType"="Opaque" }
  Pass {
   Tags { "LightMode"="ForwardBase" }
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma multi_compile_fwdbase
   #include "UnityCG.cginc"
   #include "Lighting.cginc"
   #include "AutoLight.cginc"
   fixed4 _Color;
   struct v2f { float4 pos:SV_POSITION; float3 normal:TEXCOORD0; float3 world:TEXCOORD1; SHADOW_COORDS(2) };
   v2f vert(appdata_base v) { v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.normal=UnityObjectToWorldNormal(v.normal); o.world=mul(unity_ObjectToWorld,v.vertex).xyz; TRANSFER_SHADOW(o); return o; }
   fixed4 frag(v2f i):SV_Target {
    float n=dot(normalize(i.normal),normalize(_WorldSpaceLightPos0.xyz));
    float bands=smoothstep(-.18,.12,n)*.18+smoothstep(.26,.48,n)*.22+smoothstep(.68,.86,n)*.14;
    float shadow=SHADOW_ATTENUATION(i);
    float3 warm=float3(1.0,.94,.80);
    float3 lit=_Color.rgb*(.55+bands*lerp(.48,1,shadow))*warm;
    return fixed4(lit,1);
   }
   ENDCG
  }
  UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
 }
}
