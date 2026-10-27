Shader "MementoMori/LunarWaterV3"
{
 Properties { [PerRendererData] _MainTex("Sprite",2D)="white"{} }
 SubShader
 {
  Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
  Blend SrcAlpha OneMinusSrcAlpha
  Cull Off
  ZWrite Off
  Pass
  {
   Tags { "LightMode"="Universal2D" }
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   struct A { float4 positionOS:POSITION; };
   struct V { float4 positionCS:SV_POSITION; float2 world:TEXCOORD0; };
   V vert(A i) { V o; float3 w=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(w);o.world=w.xy;return o; }
   float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   half4 frag(V i):SV_Target
   {
    float2 p=floor(i.world*32)/32;
    float2 d=p-float2(57.5,-51.5);
    float t=_Time.y*.3;
    float depth=saturate(1-length(d/float2(9,6)));
    float w=sin(p.y*13+sin(p.x*2.8+t)*2.4-t)*.5+.5;
    float w2=sin(p.y*23+p.x*3.1+t)*.5+.5;
    float3 c=lerp(float3(.006,.035,.063),float3(.003,.009,.025),depth);
    c+=float3(.006,.018,.035)*w*w2;
    float trail=exp(-pow((d.x+sin(d.y*3+t)*.6)/2.4,2))*exp(-abs(d.y)/4);
    c+=float3(.07,.14,.23)*trail*step(.94,w)*step(.45,w2);
    float foam=step(.97,w)*step(.8,hash(floor(p*4)))*(1-depth);
    c+=float3(.04,.09,.12)*foam;
    return half4(c,1);
   }
   ENDHLSL
  }
 }
}
