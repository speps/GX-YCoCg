Shader "YCoCg/Decode"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
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
			uniform float4 _MainTex_TexelSize;
			uniform int _FilterType;

			float3 YCoCg2RGB(float3 c)
			{
				return float3(c.r + c.g - c.b, c.r + c.b, c.r - c.g - c.b);
			}

			float edgeFilter(float2 center, float2 a0, float2 a1, float2 a2, float2 a3)
			{
				float4 lum = float4(a0.x, a1.x, a2.x, a3.x);
				float4 w = 1.0f - step(30.0f / 255.0f, abs(lum - center.x));
				float W = w.x + w.y + w.z + w.w;
				//Handle the special case where all the weights are zero.
				//In HDR scenes it's better to set the chrominance to zero.
				//Here we just use the chrominance of the first neighbor.
				w.x = (W == 0) ? 1 : w.x;
				W = (W == 0) ? 1 : W;

				return (w.x * a0.y + w.y* a1.y + w.z* a2.y + w.w * a3.y) / W;
			}

			float4 frag(v2f_img i) : SV_Target
			{
				float4 col = tex2D(_MainTex, i.uv);

				float chroma = 0;
				if (_FilterType == 1) // Nearest
				{
					chroma = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0)).g;
				}
				else if (_FilterType == 2) // Bilinear
				{
					chroma = 0.25f * (
						tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0)).g +
						tex2D(_MainTex, i.uv - float2(_MainTex_TexelSize.x, 0)).g +
						tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y)).g +
						tex2D(_MainTex, i.uv - float2(0, _MainTex_TexelSize.y)).g
					);
				}
				else if (_FilterType == 3) // EdgeDirected
				{
					float2 a0 = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0)).rg;
					float2 a1 = tex2D(_MainTex, i.uv - float2(_MainTex_TexelSize.x, 0)).rg;
					float2 a2 = tex2D(_MainTex, i.uv + float2(0, _MainTex_TexelSize.y)).rg;
					float2 a3 = tex2D(_MainTex, i.uv - float2(0, _MainTex_TexelSize.y)).rg;
					chroma = edgeFilter(col.rb, a0, a1, a2, a3);
				}

				int2 screenXY = i.uv * _ScreenParams.xy;
				bool pattern = (screenXY.x % 2) == (screenXY.y % 2);
				col.b = chroma;
				col.rgb = pattern ? col.rbg : col.rgb;
				col.rgb = YCoCg2RGB(col.rgb);
				return float4(col.rgb, 1);
			}
			ENDCG
		}
	}
}
