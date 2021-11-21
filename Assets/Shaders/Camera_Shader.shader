Shader "Hidden/Camera_Shader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        TopHeight ("Top Height", float) = 4.5
        BottomHeight ("Bottom Height", float) = 4.2

        CurrentHeight ("Current Height", float) = 5.0
    }
    SubShader
    {
        // No culling or depth 
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 proj_pos : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
	            o.proj_pos = ComputeScreenPos(o.vertex);
                o.uv = v.uv;
                return o;
            }

            //-----------------------

            sampler2D _MainTex;

            sampler2D _CameraDepthTexture;

            float4 WaterColor;

            float TopHeight;
            float BottomHeight;

            float CurrentHeight;

            //-----------------------

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculates the screen depth + color of the 
                float screen_depth = LinearEyeDepth(tex2D(_CameraDepthTexture, i.proj_pos.xy / i.proj_pos.w)) / 25.0f;
                fixed4 col = tex2D(_MainTex, i.uv);

                // Calculates the screen depth, adjusted for clamping + raised to a power so that the effect looks nicer
                float adjusted_screen_depth = pow(clamp(1.0 - screen_depth, 0.3, 1.0), 2.0);

                // Calculates the height difference between the two ends and how submerged the player is
                float height_diff = TopHeight - BottomHeight;
                float submerge_factor = clamp((CurrentHeight - BottomHeight) / height_diff, 0.0, 1.0);

                // Calculates the underwater player color
                float3 underwater_color = col.rgb * float3(0.3, 0.9, 0.8) * adjusted_screen_depth + (1.0 - adjusted_screen_depth) * float3(0.15, 0.5, 0.4);

                col.rgb = col.rgb * submerge_factor + (1.0 - submerge_factor) * underwater_color;

                return col; 
            }
            ENDCG
        }
    }
}
