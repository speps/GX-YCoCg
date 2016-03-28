Shader "YCoCg/Encode"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma target 3.0
			
			#include "UnityCG.cginc"
			
			sampler2D _MainTex;

			float3 RGB2YCoCg(float3 c)
			{
				return float3( 0.25*c.r+0.5*c.g+0.25*c.b, 0.5*c.r-0.5*c.b +0.5, -0.25*c.r+0.5*c.g-0.25*c.b +0.5);
			}

			float4 frag(v2f_img i) : SV_Target
			{
				float3 col = tex2D(_MainTex, i.uv).rgb;
				float3 YCoCg = RGB2YCoCg(col);
				int2 screenXY = i.uv * _ScreenParams.xy;
				bool pattern = (screenXY.x % 2) == (screenXY.y % 2);
				return float4(pattern ? YCoCg.rb : YCoCg.rg, 0, 1);
			}
			ENDCG
		}
	}
}
