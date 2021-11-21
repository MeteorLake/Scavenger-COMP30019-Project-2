Shader "Unlit/Glitch_Shader"
{
    Properties
    {
        [Header(Textures)]
        [Space]
        _MainTex ("Color Texture", 2D) = "white" {}
        NoiseTexture ("Noise Texture", 2D) = "white" {}
        GlowTexture ("Glow Texture", 2D) = "white" {}

        [Header(Glitch Effect)]
        [Space]
        GlitchIntensity ("Glitch Intensity", float) = 0.02
        GlitchFrequency ("Glitch Frequency", float) = 20.0

        Tint ("Tint", Color) = (1.0, 1.0, 1.0, 1.0)

        NoiseScale ("Noise Scale", float) = 128.0
        NoiseTransparency ("Noise Transparency", float) = 0.5
        NoiseSpeed ("Noise Speed", float) = 2.0
        
        GlowScale ("Glow Scale", float) = 128.0
        GlowSpeed ("Glow Speed", float) = 0.1
        
        IsGlitching ("Is Glitching", float) = 0.0

        Transparency ("Transparency", float) = 0.5

        [Header(Control Variables)]
        [Space]
        Time ("Time", float) = 0.0
        _EmissionLM ("Emission (Lightmapper)", Float) = 0
        [Toggle] _DynamicEmissionLM ("Dynamic Emission (Lightmapper)", Int) = 0
    }
    SubShader
    {
        Tags {"Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent"}
        Blend One OneMinusSrcAlpha
        BlendOp Add

        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog

            //-----------------------

            #include "UnityCG.cginc"

            //-----------------------

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 normal : TEXCOORD1;
                float4 proj_pos : TEXCOORD2;
                float4 vertex : SV_POSITION;
                
                UNITY_FOG_COORDS(3)
            };

            //-----------------------

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D NoiseTexture;
            sampler2D GlowTexture;
            
            sampler2D _CameraDepthNormalsTexture;

            //-----------------------

            float4 Tint;

            float Time;

            float GlitchIntensity;
            float GlitchFrequency;

            float NoiseScale;
            float NoiseTransparency;
            float NoiseSpeed;
            
            float GlowScale;
            float GlowSpeed;

            float IsGlitching;

            float Transparency;

            //-----------------------

            float PlayerOxygen;

            //-----------------------

            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex = o.vertex + float4(
                    sin(o.vertex.y * GlitchFrequency + Time) * GlitchIntensity,
                    0.0,
                    0.0,
                    0.0
                ) * IsGlitching * pow((1.0 - PlayerOxygen), 2.0);
                
	            o.proj_pos = ComputeScreenPos(o.vertex);

                o.normal = UnityObjectToWorldNormal(v.normal);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);

                UNITY_TRANSFER_FOG(o, o.vertex);
                return o;
            }

            //-----------------------

            float calc_fresnel(float3 normal, float3 view_dir, float power) {
                return pow((1.0 - saturate(dot(normalize(normal), normalize(view_dir)))), power);
            }

            float hash_func(float2 p) {
                return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
            }

            float4 frag(v2f i) : SV_Target
            {

                // Calculates the point this pixel is on the screen
                float2 screen_pos = i.proj_pos.xy / i.proj_pos.w;
                    
                //-----------------------
                
                // Gets the camera's direction
                float3 cam_dir = mul((float3x3)unity_CameraToWorld, float3(0, 0, -1));

                // Samples the main diffuse texture
                float4 diffuse = tex2D(_MainTex, i.uv);

                // Gets the desired coordinates for the scanline
                float2 noise_coords = i.vertex.xy + float2(Time * NoiseSpeed * NoiseScale, 0.0);
                float2 glow_coords = i.vertex.xy + float2(0.0, Time * GlowSpeed * GlowScale);

                // Samples the scanline texture
                float4 noise_col = tex2D(NoiseTexture, noise_coords / NoiseScale);
                noise_col = noise_col * NoiseTransparency + PlayerOxygen * 0.5;

                // Samples the glow texture
                float4 glow_overlay = tex2D(GlowTexture, glow_coords / GlowScale) * diffuse;

                //-----------------------

                // Calculates a screen pos offset to influence the hash function
                float2 screen_pos_offset = screen_pos + Time * 100.0;

                // Using the hash function, blots out squares of this grid.
                float static_effect = pow(hash_func(floor(screen_pos_offset * 20.0) / 20.0), 0.4);
                float static_secondary = pow(hash_func(floor(screen_pos_offset * 50.0) / 50.0), 0.1);

                float static_final = clamp((PlayerOxygen) + static_effect * static_secondary, 0.0, 1.0);
                
                //-----------------------

                // Calculates the fresnel effect
                float fresnel = calc_fresnel(i.normal, cam_dir, 1.0) * 0.4;

                //-----------------------

                // Creates the final color
                float3 final_col = diffuse.rgb * Tint * noise_col.rgb + glow_overlay.rgb + Tint * fresnel;
                float4 col = float4(final_col, 1.0) * static_final;
                col.a = Transparency * noise_col.r;
                col = col * 1.25;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}