Shader "Memento/V65/Environment"
{
 Properties {
  [PerRendererData] _MainTex("Sprite",2D)="white"{}
  _Color("Tint",Color)=(1,1,1,1)
  _Mode("0 art 1 wind 2 cloth 3 water 4 void 5 magic",Float)=0
  _Strength("Motion",Float)=0.012
  _Seed("Phase",Float)=0
  _Pixels("Source pixels",Vector)=(128,128,0,0)
  _Gate("Gate normalized left top right bottom",Vector)=(-1,-1,-1,-1)
  _Gate2("Second gate",Vector)=(-1,-1,-1,-1)
  _State("Real state",Float)=0
  _BackgroundSurface("Repeat approved water",Float)=0
 }
 SubShader {
  Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "CanUseSpriteAtlas"="False" }
  Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
  Pass {
   Tags {"LightMode"="SRPDefaultUnlit"}
   HLSLPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
   CBUFFER_START(UnityPerMaterial)
   float4 _Color, _Pixels, _Gate, _Gate2;
   float _Mode, _Strength, _Seed, _State, _BackgroundSurface; float4 _MainTex_TexelSize;
   CBUFFER_END
   struct A { float4 positionOS:POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; };
   struct V { float4 positionCS:SV_POSITION; float2 uv:TEXCOORD0; float4 color:COLOR; float2 world:TEXCOORD1; };
   V vert(A a){ V o; float t=_Time.y; float anchor=_Mode<1.5?a.uv.y:1-a.uv.y;
    if(_Mode>.5&&_Mode<2.5) a.positionOS.x += sin(t*.8+a.uv.y*2.7+_Seed)*anchor*anchor*_Strength;
    o.world=TransformObjectToWorld(a.positionOS.xyz).xy; o.positionCS=TransformObjectToHClip(a.positionOS.xyz);o.uv=a.uv;o.color=a.color*_Color;return o; }
   bool gate(float2 p,float4 g){return g.x>=0&&p.x>=g.x&&p.y>=g.y&&p.x<g.z&&p.y<g.w;}
   float3 ToLinear(float3 c){return lerp(c/12.92,pow((max(c,0)+.055)/1.055,2.4),step(.04045,c));}
   half4 frag(V i):SV_Target {
    float2 top=float2(i.uv.x,1-i.uv.y); if(gate(top,_Gate)||gate(top,_Gate2))discard;
    float t=_Time.y;float2 uv=i.uv;
    if(_Mode>.5&&_Mode<2.5){float anchor=_Mode<1.5?uv.y:1-uv.y;uv.x+=sin(t*.8+uv.y*5+_Seed)*anchor*anchor*_Strength*.02;}
    if(_Mode>4.5&&_Mode<5.5)uv.x+=sin(uv.y*24+t*.7+_Seed)*.0015;
    half4 c=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,uv)*i.color;
    if(_Mode>2.5&&_Mode<4.5){
     float2 p=floor(float2(i.world.x,-i.world.y)*32); float wave=sin(p.x*.07+sin(p.y*.055+t*1.1)*2.6+t*.6)+sin(p.y*.12-p.x*.035-t*.95);
     float caustic=pow(max(0.,cos(p.x*.095+sin(p.y*.12+t*.8)*2.)*cos(p.y*.13-sin(p.x*.075-t*.7)*2.)),9.);
     float3 water=float3(.045,.095,.22)+wave*float3(.018,.027,.05)+caustic*float3(.14,.23,.39);
     float flow=sin(p.x*.041+sin(p.y*.025-t*.65)*4.+t*.9)*cos(p.y*.038+sin(p.x*.03+t*.6)*3.);
     float mist=pow(max(0.,flow),3.);float stars=pow(max(0.,sin(p.x*.91+sin(p.y*.47)*9.)*cos(p.y*.82)),48.);
     float3 abyss=float3(.035,.014,.09)+mist*float3(.20,.045,.36)+stars*(.5+.5*sin(t*2.+p.x))*float3(.42,.23,.72);
     if(_Mode>3.5)c.rgb=ToLinear(abyss);else{float2 waterUV=i.uv;if(_BackgroundSurface>.5){float2 q=abs(frac(float2(i.world.x,-i.world.y)*64/float2(1536,1280))*2-1);waterUV=q*float2(768,640)*_MainTex_TexelSize.xy;c.a=1;}float2 drift=float2(sin(t*.45+waterUV.y*19),cos(t*.38+waterUV.x*17))*_MainTex_TexelSize.xy*1.5;c.rgb=SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,waterUV+drift).rgb*(1+wave*.015+caustic*.025);}
    }
    if(_Mode>4.5&&_Mode<5.5){float pulse=.5+.5*sin(t*1.4+_Seed);float bands=pow(max(0.,sin(i.uv.y*38-t*1.1)),10.);c.rgb+=c.rgb*(.06*pulse+.09*bands)+float3(.12,.04,.2)*_State*pulse*c.a;}
    if(_Mode>5.5){float2 leaf=(floor(i.uv*8)/8-.5)*2;float shape=leaf.x*leaf.x+leaf.y*leaf.y*.55;c.a*=step(shape,.6);}
    return c;
   }
   ENDHLSL
  }
 }
}


