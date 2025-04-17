Shader "Custom/WallLightEffect" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Main Color", Color) = (1,1,1,1)
        _WaveOrigin ("Wave Origin", Vector) = (0,0,0,0)
        _WaveDistance ("Wave Distance", Float) = 0
        _WaveWidth ("Wave Width", Range(0.0, 5.0)) = 1.0
        _LightColor ("Light Color", Color) = (1,1,0,1)
        _LightIntensity ("Light Intensity", Range(0.0, 2.0)) = 1.0
        _FadeSpeed ("Fade Speed", Range(0.0, 5.0)) = 1.0
    }
    SubShader {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }
        
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
                float4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float4 _WaveOrigin;
            float _WaveDistance;
            float _WaveWidth;
            float4 _LightColor;
            float _LightIntensity;
            float _FadeSpeed;

            v2f vert (appdata v) {
                v2f o;
                
                // Normal sprite render etme
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                
                // Dünya koordinatını fragment shader'a ilet
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                
                #ifdef PIXELSNAP_ON
                o.vertex = UnityPixelSnap(o.vertex);
                #endif
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // Ana sprite'ı render et
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;
                
                // Işık kaynağına olan mesafeyi hesapla
                float dist = distance(_WaveOrigin.xy, i.worldPos.xy);
                
                // Işık efekti sınırları içerisinde olup olmadığını kontrol et
                if (dist < _WaveDistance) {
                    // Mesafeye göre azalan bir ışık efekti oluştur
                    float lightFactor = 1.0 - (dist / _WaveDistance);
                    
                    // Dalga genişliği parametresini ışık dalgaları için kullan
                    float waveRing = abs(sin((dist * _WaveWidth) + (_Time.y * _FadeSpeed)));
                    
                    // Parlak halka efekti oluştur
                    lightFactor = lightFactor * waveRing;
                    
                    // Işık rengini ve yoğunluğunu uygula
                    fixed4 lightColor = _LightColor * lightFactor * _LightIntensity;
                    
                    // Işık rengini orijinal renge ekle (daha parlak görünüm için)
                    col.rgb = col.rgb + lightColor.rgb;
                }
                
                // Sprite shader için premultiplied alpha
                col.rgb *= col.a;
                
                return col;
            }
            ENDCG
        }
    }
    FallBack "Sprites/Default"
} 