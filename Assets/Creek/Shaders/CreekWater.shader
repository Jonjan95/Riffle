Shader "Creek/Water" {
 Properties { _Color ("Water",Color)=(.16,.58,.60,1) }
 SubShader {
 Tags { "RenderType"="Opaque" }
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 fixed4 _Color;
 struct v2f { float4 pos:SV_POSITION; float3 world:TEXCOORD0; };
 v2f vert(appdata_base v) { v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.world=mul(unity_ObjectToWorld,v.vertex).xyz; return o; }
 fixed4 frag(v2f i):SV_Target {
 float t=_Time.y;
 float wave=sin(i.world.x*2.3+i.world.z*3.4+t*.8)+sin(i.world.z*8-i.world.x*1.7+t*1.3)*.35;
 float ribbon=smoothstep(.92,1.25,wave)*.10;
 float3 c=_Color.rgb+float3(.11,.18,.14)*ribbon+sin(i.world.z*2.5+t*.65)*.024;
 return fixed4(c,1);
 }
 ENDCG
 }
 }
}
