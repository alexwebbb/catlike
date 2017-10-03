// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/My First Shader" {

	SubShader{

		Pass{

			CGPROGRAM

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#include "UnityCG.cginc"

			float4 MyVertexProgram (float4 pos : POSITION) : SV_POSITION {
				return UnityObjectToClipPos(pos);
			}

			float4 MyFragmentProgram (float4 pos : SV_POSITION) : SV_TARGET {
				return float4(1,1,0,1);
			}

			ENDCG
		}
	}
}