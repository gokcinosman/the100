Shader "Custom/PlayerGlow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _GlowColor ("Glow Color", Color) = (1,1,1,1)
        _GlowSpeed ("Glow Speed", Range(0.1, 5)) = 1
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 1
        _GlowRange ("Glow Range", Range(0.1, 2)) = 1
    }

    SubShader
    {
        Tags { 
            "RenderType" = "Transparent" 
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _Color;
                float4 _GlowColor;
                float _GlowSpeed;
                float _GlowIntensity;
                float _GlowRange;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.color = input.color;
                return output;
            }

            float boxGlow(float2 uv)
            {
                float2 d = abs(uv - 0.5) * 2;
                float maxD = max(d.x, d.y);
                return smoothstep(0.8, 1.0, maxD);
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                
                // Create pulsing glow effect
                float glowPulse = (sin(_Time.y * _GlowSpeed) * 0.5 + 0.5) * _GlowRange;
                
                // Calculate box-shaped glow
                float edgeGlow = boxGlow(input.uv);
                
                // Combine main color with glow
                half4 finalColor = mainTex * _Color;
                float glowAmount = edgeGlow * glowPulse * _GlowIntensity;
                finalColor.rgb += _GlowColor.rgb * glowAmount;
                finalColor.a = max(mainTex.a, glowAmount * _GlowColor.a);
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}