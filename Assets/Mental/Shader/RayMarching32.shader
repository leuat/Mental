// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LemonSpawn/RayMarching32"
{

	CGINCLUDE

#include "UnityCG.cginc"
#pragma target  3.0
#pragma profileoption MaxLocalParams=1024 
#pragma profileoption NumInstructionSlots=4096
#pragma profileoption NumMathInstructionSlots=4096

#pragma multi_compile __ SHADING_HARD 
#pragma multi_compile __ SHADING_OPACITY 
#pragma multi_compile __ HAS_SHADOWS 
#pragma multi_compile __ HAS_LIGHTING

#define S 32

#define TOTAL_STEPS S
#define STEP_CNT S
#define STEP_SIZE 1 / S


	ENDCG
Subshader{
		//ZTest Always Cull Off ZWrite Off
		Fog{ Mode off }

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "include/raymarchingBody.cginc"

	ENDCG
												
		}
	}

	Fallback off

} // shader
