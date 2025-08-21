Shader "Custom/LEDShader"
{
    Properties
    {
        [NoScaleOffset]_MainTex ("Albedo (RGB)", 2D) = "white" {}
		[NoScaleOffset]_LEDTex("LEDTex",2D) = "white"{}
		_Tiling ("Tiling",float) = 1
		_Brightness ("Brightness",range(0,5)) = 1
		_OffsetX ("OffsetX",float) =0 
		_OffsetY("OffsetY",float) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
		sampler2D _LEDTex;

        struct Input
        {
            float2 uv_MainTex;
			float2 uv_LEDTex;
        };

		float _Tiling;
		float _Brightness;
		float _OffsetX;
		float _OffsetY;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float4 c = tex2D (_MainTex,float2(_OffsetX,_OffsetY) + floor(IN.uv_MainTex*_Tiling)/(_Tiling));
			float4 d = tex2D(_LEDTex, IN.uv_LEDTex * _Tiling);
			o.Emission = c*d*_Brightness;
            // o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}


