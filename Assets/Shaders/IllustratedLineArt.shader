Shader "MementoMori/IllustratedLineArt"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Line", Color) = (.65,.57,1,1)
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "CanUseSpriteAtlas"="True" }
        Stencil { Ref [_Stencil] Comp [_StencilComp] Pass [_StencilOp] ReadMask [_StencilReadMask] WriteMask [_StencilWriteMask] }
        Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            struct appdata { float4 vertex:POSITION; float2 uv:TEXCOORD0; fixed4 color:COLOR; };
            struct v2f { float4 vertex:SV_POSITION; float2 uv:TEXCOORD0; float4 world:TEXCOORD1; fixed4 color:COLOR; };
            sampler2D _MainTex; float4 _MainTex_TexelSize; fixed4 _Color; float4 _ClipRect;
            v2f vert(appdata v) { v2f o; o.world=v.vertex; o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;o.color=v.color;return o; }
            fixed4 frag(v2f i):SV_Target
            {
                fixed4 c=tex2D(_MainTex,i.uv);
                fixed4 x=tex2D(_MainTex,i.uv+float2(_MainTex_TexelSize.x,0));
                fixed4 y=tex2D(_MainTex,i.uv+float2(0,_MainTex_TexelSize.y));
                fixed4 nx=tex2D(_MainTex,i.uv-float2(_MainTex_TexelSize.x,0));
                fixed4 ny=tex2D(_MainTex,i.uv-float2(0,_MainTex_TexelSize.y));
                float edge=max(max(abs(c.a-x.a),abs(c.a-y.a)),max(abs(c.a-nx.a),abs(c.a-ny.a)));
                float detail=max(length(c.rgb-x.rgb),length(c.rgb-y.rgb));
                float ink=step(.22,max(edge,detail))*c.a;
                #ifdef UNITY_UI_CLIP_RECT
                ink*=UnityGet2DClipping(i.world.xy,_ClipRect);
                #endif
                return fixed4(_Color.rgb,ink*_Color.a*i.color.a);
            }
            ENDCG
        }
    }
}
