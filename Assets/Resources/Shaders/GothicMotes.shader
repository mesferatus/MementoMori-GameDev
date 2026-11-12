// Drifting dust motes / embers for the gothic ambience. Screen-space overlay,
// additive and very faint; loaded by name from Resources so no scene changes are needed.
Shader "Memento/UI/GothicMotes"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Tint ("Mote tint", Color) = (0.62, 0.45, 0.85, 1)
        _Density ("Density", Range(0, 2)) = 1
        _Intensity ("Intensity", Range(0, 1)) = 0.35
        _Aspect ("Screen aspect", Float) = 1.7778
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha One   // additive glow over the scene
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            fixed4 _Tint; float _Density, _Intensity, _Aspect;
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color; return o; }
            float2 hash2 (float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453);
            }
            // one layer of slowly rising motes on a jittered grid
            float layer (float2 uv, float cells, float speed, float t)
            {
                uv.y += t * speed;               // slow upward drift
                uv.x += sin(t * .07 + cells) * .02;
                float2 g = uv * cells;
                float2 id = floor(g);
                float2 f = frac(g);
                float2 h = hash2(id);
                // only some cells carry a mote; the rest stay dark
                if (h.x > .22 * _Density) return 0;
                float2 center = .5 + (h - .5) * .6;
                center.x += sin(t * (.35 + h.y) + h.x * 17) * .08; // lateral sway
                float d = length((f - center) * float2(1, _Aspect / 1.7778));
                float twinkle = .55 + .45 * sin(t * (1.1 + h.y * 2.4) + h.x * 31);
                return exp(-d * d * 220) * twinkle;
            }
            fixed4 frag (v2f i) : SV_Target
            {
                float t = _Time.y;
                float m = layer(i.uv, 9, .006, t) * .9
                        + layer(i.uv + 37.17, 15, .010, t) * .6
                        + layer(i.uv + 91.3, 24, .016, t) * .35;
                // keep the centre of the screen quieter than the edges
                float2 c = (i.uv - .5) * 2;
                float edge = saturate(dot(c, c) * .9 + .25);
                float a = saturate(m * _Intensity * edge);
                return fixed4(_Tint.rgb, a * i.color.a);
            }
            ENDCG
        }
    }
}
