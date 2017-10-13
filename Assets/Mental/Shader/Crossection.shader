// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LemonSpawn/Crossection"
{

	CGINCLUDE


#include "UnityCG.cginc"
#pragma target  3.0
#pragma profileoption MaxLocalParams=1024 
#pragma profileoption NumInstructionSlots=4096
#pragma profileoption NumMathInstructionSlots=4096



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

		#include "include/util.cginc"
		#include "include/raymarching.cginc"


		half4 frag(v2f i) : COLOR{

			Ray r;
			r.Dir = -coord2ray2(1-i.uv[0].x, i.uv[0].y - 0.0, 1, 1);

			r.Origin = _Camera;

			Ray plane;
			plane.Origin = _SplitPos;
			plane.Dir = _SplitPlane;


				//float RayIntersectPlane(Ray ray, Ray plane, out float3 intersectionPoint)
			float4 val = float4(0,0,0,0);
			//_InternalScale.y*=10;
			float3 isp;
			float3 is = _InternalScale*0.5;
			if (RayIntersectPlane(r, plane, isp)) {
				isp.x = clamp(isp.x, -is.x,is.x);
				isp.y = clamp(isp.y, -is.y,is.y);
				isp.z = clamp(isp.z, -is.z,is.z);
				
				val = getTex(_VolumeTex, isp);
			}
			val.xyz*=val.a*6;
//			val.a = 1;
			val.xyz *= 2*_IntensityScale;
			return val;

	

		}
		ENDCG
	}
}

		Fallback off

} // shader