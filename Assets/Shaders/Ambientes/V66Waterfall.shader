Shader "Memento/V66/Waterfall"
{
 Properties {
  _Color("Lunar water",Color)=(.18,.35,.85,.66)
  _Pixels("Width and height in source pixels",Vector)=(62,150,0,0)
  _Speed("Downward pixels per second",Float)=100
  _Seed("Phase",Float)=0
  _Foam("Impact surface",Float)=0
 }
 SubShader {
  Tags {"Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline"}
  Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   Tags {"LightMode"="SRPDefaultUnlit"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   CBUFFER_START(UnityPerMaterial)
   float4 _Color, _Pixels; float _Speed, _Seed, _Foam;
   CBUFFER_END
   struct A {float4 positionOS:POSITION;float2 uv:TEXCOORD0;};
   struct V {float4 positionCS:SV_POSITION;float2 uv:TEXCOORD0;};
   V vert(A a){V o;o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.uv=a.uv;return o;}
   float hash(float x){return frac(sin(x*127.1+_Seed)*43758.5453);}
   half4 frag(V i):SV_Target {
    float2 p=floor(i.uv*_Pixels.xy);float t=_Time.y;
    if(_Foam>.5){
     float2 q=(p/_Pixels.xy-.5)*2;float radius=length(q);
     float ring=abs(frac(radius*2.8-t*.9+_Seed)-.5);
     float foam=(1-smoothstep(.03,.13,ring))*saturate(1-radius)*.6;
     float core=(1-smoothstep(.18,.65,radius))*(.5+.2*sin(p.x*2.1+t*8));
     return half4(.43,.62,1,saturate(foam+core)*_Color.a);
    }
    // A feature moves toward decreasing UV.y as time increases: real downward flow.
    float column=floor(p.x/2);float velocity=_Speed*(.75+hash(column)*.5);
    float travel=p.y+t*velocity;
    float stream=sin(travel*.12+hash(column)*25);
    float glint=pow(saturate(stream),9)*(.35+.65*hash(column+7));
    float detail=pow(saturate(sin(travel*.27+column*2.7)),16)*.3;
    float edge=smoothstep(0,2,p.x)*smoothstep(0,2,_Pixels.x-1-p.x);
    float ends=smoothstep(0,3,p.y)*smoothstep(0,3,_Pixels.y-p.y);
    float3 rgb=_Color.rgb+float3(.24,.34,.45)*(glint+detail);
    return half4(rgb,_Color.a*edge*ends*(.45+.45*glint+.1*sin(column*3)));
   }
   ENDHLSL
  }
 }
}
