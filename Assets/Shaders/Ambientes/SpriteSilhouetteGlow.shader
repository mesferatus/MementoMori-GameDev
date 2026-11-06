// Solid additive silhouette of a sprite. Drawn slightly enlarged behind an interactable,
// it reads as a soft lilac rim (interaction highlight). Pixel-snapped pulse driven by _Color alpha.
Shader "Memento/FX/SpriteSilhouetteGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Glow", Color) = (0.72, 0.52, 1.0, 1)
        _Intensity ("Intensity", Range(0, 3)) = 1.2
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "IgnoreProjector"="True" }
        Cull Off ZWrite Off
        Blend One One
        Pass
        {
            Tags { "LightMode"="SRPDefaultUnlit" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            TEXTURE2D(_MainTex); SAMPLER(sampler_MainTex);
            CBUFFER_START(UnityPerMaterial)
            float4 _Color; float _Intensity;
            CBUFFER_END
            struct A { float4 positionOS : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct V { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            V vert (A a) { V o; o.positionCS = TransformObjectToHClip(a.positionOS.xyz); o.uv = a.uv; o.color = a.color; return o; }
            half4 frag (V i) : SV_Target
            {
                float alpha = step(.35, SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv).a);
                float3 col = _Color.rgb * _Intensity * alpha * i.color.a;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
