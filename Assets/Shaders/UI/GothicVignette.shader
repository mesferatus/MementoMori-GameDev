// Gothic gameplay vignette: violet-black edges, a faint lilac rim and slow drifting fog.
// Screen-space UI overlay; never receives input (the Image has raycastTarget off).
Shader "Memento/UI/GothicVignette"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Shadow", Color) = (0.018, 0.006, 0.04, 1)
        _Glow ("Rim glow", Color) = (0.30, 0.10, 0.46, 1)
        _Intensity ("Intensity", Range(0, 1)) = 0.9
        _Inner ("Inner radius", Range(0, 1.5)) = 0.62
        _Outer ("Outer radius", Range(0.2, 2)) = 1.28
        _Fog ("Fog amount", Range(0, 1)) = 0.45
        _Aspect ("Screen aspect", Float) = 1.7778
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" "PreviewType"="Plane" }
        Cull Off ZWrite Off ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct v2f { float4 pos : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            fixed4 _Color, _Glow;
            float _Intensity, _Inner, _Outer, _Fog, _Aspect;
            v2f vert (appdata v) { v2f o; o.pos = UnityObjectToClipPos(v.vertex); o.uv = v.uv; o.color = v.color; return o; }
            float hash (float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float noise (float2 p)
            {
                float2 i = floor(p), f = frac(p); f = f * f * (3 - 2 * f);
                return lerp(lerp(hash(i), hash(i + float2(1, 0)), f.x), lerp(hash(i + float2(0, 1)), hash(i + 1), f.x), f.y);
            }
            float fbm (float2 p) { float s = 0, a = .5; for (int k = 0; k < 4; k++) { s += a * noise(p); p *= 2.03; a *= .5; } return s; }
            fixed4 frag (v2f i) : SV_Target
            {
                float2 c = (i.uv - .5) * float2(_Aspect / 1.7778, 1) * 2;
                // squarer than a circle so the corners carry the weight, like a carved frame
                float d = pow(pow(abs(c.x), 2.6) + pow(abs(c.y) * 1.08, 2.6), 1 / 2.6);
                float t = _Time.y;
                float fog = fbm(i.uv * float2(5.5, 3.2) + float2(t * .018, -t * .011)) * .65
                          + fbm(i.uv * float2(11, 7) - float2(t * .03, t * .02)) * .35;
                float v = smoothstep(_Inner, _Outer, d + (fog - .5) * _Fog * .35);
                float rim = exp(-pow((d - _Inner * 1.12) * 7.5, 2)) * (.35 + .65 * fog);
                float pulse = .94 + .06 * sin(t * .55);
                // lilac haze between the lit room and the shadow: the approved "purple oval" look
                fixed3 col = lerp(_Glow.rgb, _Color.rgb, saturate(v * 3.5));
                float a = saturate(v * _Intensity * pulse + rim * .05 * _Fog);
                return fixed4(col, a * i.color.a);
            }
            ENDCG
        }
    }
}
