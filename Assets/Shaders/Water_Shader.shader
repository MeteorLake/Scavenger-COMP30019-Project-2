Shader "Unlit/Water_Shader"
{
    Properties
    {
        [Header(Textures)]
        [Space]
        _MainTex ("Shallow Albedo", 2D) = "white" {}
        DeepWater ("Deep Albedo", 2D) = "white" {}
        NormalMap ("Normal", 2D) = "white" {}
        [NoScaleOffset] NormalMap_2 ("Normal 2", 2D) = "white" {}
        FoamMap ("Foam", 2D) = "white" {}
        [NoScaleOffset] FoamMap_2 ("Foam 2", 2D) = "white" {}
        HeightMap ("Height", 2D) = "white" {}

        [Header(Wave Shape Properties)]
        [Space]
        Amplitude ("Amplitude", float) = 0.2
        WaveSpeed ("Wave Speed", float) = 1.0
        
        [Header(Wave Texture Properties)]
        [Space]
        Transparency ("Transparency", float) = 0.75
        DiffuseIntensity ("Diffuse Intensity", float) = 0.1
        Reflectivity ("Reflectivity", float) = 0.025
        FoamDistance ("Foam Distance", float) = 100

        [Header(Other)]
        [Space]
        Time ("Time", float) = 0.0
    }

    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        LOD 200

        Pass
        {
            Name "DepthOnly"
		    Tags { "LightMode"="DepthOnly" }
            ZWrite On
            ColorMask 0

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            //-----------------------

            #include "UnityCG.cginc"
            
            //-----------------------

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            //-----------------------
            
            float Time;
            float Amplitude;
            float WaveSpeed;

            sampler2D HeightMap;
            float4 HeightMap_ST;

            //-----------------------

            v2f vert (appdata v)
            {
                v2f o;

                // Calculates the world position of the vertex
                float4 world_vert = mul(unity_ObjectToWorld, v.vertex);

                float4 wave_offset_tex = tex2Dlod(
                    HeightMap,
                    float4(
                        ((world_vert.x) / HeightMap_ST.x + HeightMap_ST.z) + Time * WaveSpeed * 0.01,
                        ((world_vert.z) / HeightMap_ST.y + HeightMap_ST.w) + Time * WaveSpeed * 0.01,
                        0.0,
                        0.0
                    )
                );

                // Using the texture, we calculate a final offset
                float4 wave_offset = float4(0.0, wave_offset_tex.r * 2.0 - 1.0, 0.0, 0.0);

                // Calculates how much the world vertex gets offset by the gerstner wave
                float4 world_vert_offset = world_vert + wave_offset * Amplitude;

                // We need to add an offset so that there isn't any Z-fighting here
                o.vertex = mul(UNITY_MATRIX_VP, world_vert_offset);

                return o;
            }

            //-----------------------

            float4 frag (v2f i) : SV_Target
            {
                return float4(1.0, 1.0, 1.0, 0.0);
            }
            ENDCG
        }

        Pass
        {
            ZWrite On
            ZTest LEqual
            Cull Off
            Blend SrcAlpha OneMinusSrcAlpha
            LOD 100

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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float2 uv_normal : TEXCOORD1;

                float4 world_pos : TEXCOORD2;
                float4 proj_pos : TEXCOORD3;
                float depth : TEXCOORD4;
                float4 gerstner_foam : TEXCOORD5;

                float4 vertex : SV_POSITION;

                UNITY_FOG_COORDS(7)
            };

            //-----------------------

            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D DeepWater;

            sampler2D NormalMap;
            sampler2D NormalMap_2;
            float4 NormalMap_ST;

            sampler2D FoamMap;
            sampler2D FoamMap_2;

            sampler2D HeightMap;
            float4 HeightMap_ST;

            sampler2D _CameraDepthTexture;
            
            float Time;
            float Amplitude;
            float WaveSpeed;

            float Transparency;
            float DiffuseIntensity;
            float Reflectivity;
            float FoamDistance;

            //-----------------------

            v2f vert (appdata v)
            {
                v2f o;

                // Calculates the world position of the vertex 
                float4 world_vert = mul(unity_ObjectToWorld, v.vertex);

                float4 wave_offset_tex = tex2Dlod(
                    HeightMap,
                    float4(
                        ((world_vert.x) / HeightMap_ST.x + HeightMap_ST.z) + Time * WaveSpeed * 0.01,
                        ((world_vert.z) / HeightMap_ST.y + HeightMap_ST.w) + Time * WaveSpeed * 0.01,
                        0.0,
                        0.0
                    )
                );

                // Using the texture, we calculate a final offset
                float4 wave_offset = float4(0.0, wave_offset_tex.r * 2.0 - 1.0, 0.0, 0.0);

                // Calculates how much the world vertex gets offset by the gerstner wave
                float4 world_vert_offset = world_vert + wave_offset * Amplitude;

                o.vertex = mul(UNITY_MATRIX_VP, world_vert_offset);
                
                o.uv = float2(
                    ((world_vert_offset.x) * _MainTex_ST.x + _MainTex_ST.z),
                    ((world_vert_offset.z) * _MainTex_ST.y + _MainTex_ST.w)
                );
                o.uv_normal = float2(
                    ((world_vert_offset.x) * NormalMap_ST.x + NormalMap_ST.z),
                    ((world_vert_offset.z) * NormalMap_ST.y + NormalMap_ST.w)
                );

                o.world_pos = world_vert_offset;
                o.gerstner_foam = clamp(wave_offset.y, 0.0, 1.0);
	            o.proj_pos = ComputeScreenPos(o.vertex);
                o.depth = -mul(UNITY_MATRIX_V, world_vert_offset).z *_ProjectionParams.w;

                UNITY_TRANSFER_FOG(o, o.vertex);

                return o;
            }

            //-----------------------

            float4 frag (v2f i) : SV_Target
            {
                // Calculates the vector from the light to the world pos
                float4 light_dir = _WorldSpaceLightPos0;

                // Gets the camera's direction
                float3 cam_dir = mul((float3x3)unity_CameraToWorld, float3(0, 0, -1));

                //-----------------------

                // Samples the albedo + normal textures
                float4 albedo = tex2D(_MainTex, i.uv);
                float4 deep_albedo = tex2D(DeepWater, i.uv);

                float4 normal = tex2D(
                    NormalMap,
                    i.uv_normal + float2(
                        Time * WaveSpeed * 0.0125,
                        Time * WaveSpeed * 0.0125
                    )
                );
                float4 normal_2 = tex2D(
                    NormalMap_2,
                    i.uv_normal + float2(
                        Time * WaveSpeed * 0.05,
                        Time * WaveSpeed * 0.05
                    )
                );

                // Gets the position of the pixel on the screen
                float2 pixel_position = float2(
                    (i.vertex.x)/_ScreenParams.x,
                    (i.vertex.y)/_ScreenParams.y
                );

                float screen_depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.proj_pos.xy / i.proj_pos.w));
	            float diff = clamp((screen_depth - (i.depth * 10000.0)) / 2.5, 0.0, 1.0);

                albedo = albedo * (1.0 - diff) + deep_albedo * diff;
                albedo.a = diff;

                // Re-does the normal maps so that they're in 1.0;
                normal = float4(normal.x * 2.0 - 1.0, normal.y, normal.z * 2.0 - 1.0, normal.w);
                normal_2 = float4(normal_2.x * 2.0 - 1.0, normal_2.y, normal_2.z * 2.0 - 1.0, normal_2.w);

                // Calculates a final normal based on the two normal maps
                float4 final_normal = (normal + normal_2) / 2.0;

                //-----------------------

                // Samples the foam textures, we'll use the coordinates for the normals
                float4 foam_tex = tex2D(
                    FoamMap,
                    i.uv_normal * 4.0 + float2(
                        Time * WaveSpeed * -0.25,
                        Time * WaveSpeed * -0.25
                    )
                );
                float4 foam_tex_2 = tex2D(
                    FoamMap_2,
                    i.uv_normal * 2.0 + float2(
                        Time * WaveSpeed * -0.125,
                        Time * WaveSpeed * -0.125
                    )
                );
                
                float4 foam_tex_final = (foam_tex + foam_tex_2) / 2.0;

                // Using the depth, we remove foam that's far away
                float foam_depth_modifier = 1.0 - clamp(i.depth * FoamDistance, 0.0, 1.0);

                // Calculates the foam
                float foam = foam_tex_final * clamp(i.gerstner_foam.y, 0.0, 1.0) * foam_depth_modifier;

                //-----------------------

                // Reflects light off of the water, calculates the specular highlight
                float3 reflected_light = reflect(light_dir, final_normal);
                float specular = max(dot(reflected_light, normalize(cam_dir)), 0.0);

                // Performs shine damping, multiplies by the light color
                specular = pow(specular, 16.0) * Reflectivity * float4(1.0, 1.0, 1.0, 1.0);
                
                // Calculates the level of diffuse based on the normal map
                half lighting_normal = max(0, dot(normalize(final_normal), light_dir));
                fixed4 diffuse = lighting_normal;

                //-----------------------

                float4 col = albedo;
                col.rgb = col.rgb + (((diffuse - 0.5) * 2.0 * DiffuseIntensity) + specular).rgb + foam;
                col.a = col.a * Transparency;

                // Applies fog to the texture
                UNITY_APPLY_FOG(i.fogCoord, col);
                
                return col;
            }
            ENDCG
        }
    }
    
    Fallback "Transparent/VertexLit"
}