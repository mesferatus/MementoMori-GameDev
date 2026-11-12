// Additive pixel-art light shapes for sprites/particles.
// _Shape 0: moon shaft from a window (top of the quad) with rays and drifting dust.
// _Shape 1: soft elliptical pool of light on the floor.
// _Shape 2: round glow with candle flicker (also used by firefly particles: vertex colour drives brightness).
Shader "Memento/FX/Light"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _Color ("Light colour", Color) = (0.55, 0.66, 1.0, 1)
        _Intensity ("Intensity", Range(0, 3)) = 0.6
        _Shape ("0 shaft 1 pool 2 glow", Float) = 0
        _TopWidth ("Shaft width at window", Range(0, .5)) = .16
        _BottomWidth ("Shaft width at floor", Range(0, .5)) = .42
        _Skew ("Shaft lean", Range(-.5, .5)) = .12
        _Dust ("Dust motes", Range(0, 1)) = .7
        _Flicker ("Flicker", Range(0, 1)) = 0
        _Pixels ("Pixel grid (x,y)", Vector) = (96, 160, 0, 0)
        _Seed ("Phase", Float) = 0
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
            CBUFFER_START(UnityPerMaterial)
            float4 _Color, _Pixels;
            float _Intensity, _Shape, _TopWidth, _BottomWidth, _Skew, _Dust, _Flicker, _Seed;
            CBUFFER_END
            struct A { float4 positionOS : POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            struct V { float4 positionCS : SV_POSITION; float2 uv : TEXCOORD0; float4 color : COLOR; };
            V vert (A a) { V o; o.positionCS = TransformObjectToHClip(a.positionOS.xyz); o.uv = a.uv; o.color = a.color; return o; }
            float hash (float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
            float noise (float2 p)
            {
                float2 i = floor(p), f = frac(p); f = f * f * (3 - 2 * f);
                return lerp(lerp(hash(i), hash(i + float2(1, 0)), f.x), lerp(hash(i + float2(0, 1)), hash(i + 1), f.x), f.y);
            }
            half4 frag (V i) : SV_Target
            {
                float t = _Time.y + _Seed * 7.3;
                // snap to the pixel grid so the light reads as pixel art, not a smooth gradient
                float2 uv = (floor(i.uv * _Pixels.xy) + .5) / _Pixels.xy;
                float light = 0;
                if (_Shape < .5)
                {
                    float down = 1 - uv.y;                       // 0 at the window, 1 at the floor
                    float centre = .5 + _Skew * (down - .5);
                    float halfW = lerp(_TopWidth, _BottomWidth, down);
                    float sill = smoothstep(.06, 0, down) * .6;  // brighter where light enters the room
                    float dx = abs(uv.x - centre);
                    float body = smoothstep(halfW, halfW * .3, dx);
                    float ends = smoothstep(0, .3, uv.y) * smoothstep(1, .985, uv.y);
                    float rays = .72 + .28 * noise(float2((uv.x - centre) / max(halfW, .01) * 3.2 + t * .07, t * .05));
                    float breathe = .9 + .1 * sin(t * .4);
                    light = body * ends * rays * breathe * (.8 + .2 * uv.y + sill);
                    // Suspended dust: bounded drift instead of a continuous downward stream.
                    float2 g = uv * float2(18, 30) + float2(sin(t * .09) * .10, cos(t * .07) * .08);
                    float2 cell = floor(g), f = frac(g);
                    float h = hash(cell + _Seed);
                    float2 mote = float2(.5 + .18 * sin(t * .17 + h * 30), .5 + .14 * cos(t * .13 + h * 17));
                    float sparkle = step(.92, h) * smoothstep(.12, .0, length(f - mote)) * (.8 + .2 * sin(t * .25 + h * 40));
                    light += sparkle * body * ends * _Dust * .55;
                }
                else if (_Shape < 1.5)
                {
                    float2 c = (uv - .5) * 2;
                    float d = length(c);
                    light = smoothstep(1, .05, d) * (.85 + .15 * noise(uv * 6 + t * .1));
                }
                else
                {
                    float d = length((uv - .5) * 2);
                    float flick = 1 - _Flicker * (.16 * noise(float2(t * 2.1, _Seed)) + .08 * noise(float2(t * 5.3, _Seed + 3)));
                    light = (smoothstep(1, 0, d) * .55 + smoothstep(.35, 0, d) * .9) * flick;
                }
                float3 col = _Color.rgb * light * _Intensity * i.color.rgb * i.color.a;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
