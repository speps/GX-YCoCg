Shader "PSNR/PSNR"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass // MSE
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma enable_d3d11_debug_symbols
			#pragma target 3.0

			#include "UnityCG.cginc"
			
			sampler2D _MainTex;
			uniform float4 _MainTex_TexelSize;
			sampler2D _SubjTex;

			float4 frag(v2f_img i) : SV_Target
			{
				float3 ref = tex2D(_MainTex, i.uv).rgb;
				float3 subj = tex2D(_SubjTex, i.uv).rgb;
				float3 diff = ref - subj;
				float3 mse = diff * diff;
				return float4(mse, 1);
			}
			ENDCG
		}

		Pass // Reduce
		{
			CGPROGRAM
			#pragma vertex vert_img
			#pragma fragment frag
			#pragma target 3.0

			#include "UnityCG.cginc"

			sampler2D _MainTex;
			uniform float4 _MainTex_TexelSize;

			float4 frag(v2f_img i) : SV_Target
			{
				float3 mse0 = tex2D(_MainTex, i.uv + float2(-0.5f, -0.5f) * _MainTex_TexelSize.xy).rgb;
				float3 mse1 = tex2D(_MainTex, i.uv + float2(+0.5f, -0.5f) * _MainTex_TexelSize.xy).rgb;
				float3 mse2 = tex2D(_MainTex, i.uv + float2(-0.5f, +0.5f) * _MainTex_TexelSize.xy).rgb;
				float3 mse3 = tex2D(_MainTex, i.uv + float2(+0.5f, +0.5f) * _MainTex_TexelSize.xy).rgb;
				return float4(mse0 + mse1 + mse2 + mse3, 1);
			}
			ENDCG

		}
	}
}
