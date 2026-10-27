Shader "MementoMori/LunarLakeWater"
{
    Properties
    {
        _DeepColor ("Deep water", Color) = (0.012, 0.038, 0.12, 1)
        _ShallowColor ("Moonlit water", Color) = (0.025, 0.22, 0.43, 1)
        _MoonColor ("Reflection", Color) = (0.38, 0.79, 1, 1)
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        Pass
        {
            Name "LunarLakeSurface"
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float4 _DeepColor;
            float4 _ShallowColor;
            float4 _MoonColor;

            struct Attributes { float4 positionOS : POSITION; float4 color : COLOR; };
            struct Varyings { float4 positionCS : SV_POSITION; float2 world : TEXCOORD0; float4 color : COLOR; };

            Varyings vert(Attributes input)
            {
                Varyings output;
                float3 world = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(world);
                output.world = world.xy;
                output.color = input.color;
                return output;
            }

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 p = floor(input.world * 8.0) / 8.0;
                float t = _Time.y * 0.42;
                float ripple = sin(p.x * 1.8 + t + sin(p.y * 1.1 - t) * 1.8) * 0.23;
                ripple += sin(p.y * 2.5 - t * 1.4) * 0.13;
                float band = saturate(0.44 + ripple);
                float3 water = lerp(_DeepColor.rgb, _ShallowColor.rgb, band);
                float2 moon = p - float2(57.0, -52.0);
                float disc = 1.0 - smoothstep(1.0, 1.45, length(moon));
                float glint = step(0.982, hash21(floor(p * 3.0 + floor(t * 2.0)))) * step(0.60, band);
                water += _MoonColor.rgb * (disc * 0.50 + glint * 0.30);
                return half4(water * input.color.rgb, 0.97 * input.color.a);
            }
            ENDHLSL
        }
    }
}
