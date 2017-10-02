Shader "Custom/My First Shader" {

	SubShader{

		Pass{

			CGPROGRAM

			#pragma vertex MyVertexProgram
			#pragma fragment MyFragmentProgram

			#include "UnityCG.cginc"

			float4 MyVertexProgram (float4 pos : POSITION) : SV_POSITION {
				return mul(UNITY_MATRIX_MVP, pos);
			}

			float4 MyFragmentProgram (float4 pos : SV_POSITION) : SV_TARGET {
				return 0;
			}

			ENDCG
		}
	}
}