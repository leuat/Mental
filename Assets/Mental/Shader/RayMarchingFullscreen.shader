﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LemonSpawn/RayMarchingFullscreen"
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

#define S 256

#define TOTAL_STEPS S
#define STEP_CNT S
#define STEP_SIZE 1 / S


	ENDCG

		Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
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



		uniform sampler2D _MainTex;

	half4 renderPlane(Ray r, float4 col, AABB box, out float dir) {
		float3 isp;
		Ray plane;
		plane.Origin = _SplitPos;
		plane.Dir = _SplitPlane;

		float d = RayIntersectPlane(r, plane, isp);
		//return clamp(sign(d)*col*insideBox(isp, box),0,1);
		dir = clamp(sign(d),0,1);
		return col*insideBox(isp, box);

	}
		
		uniform float _aspectRatio;

		half4 frag(v2f i) : COLOR{
			float3 camera = _Camera;

			Ray r;
			r.Dir = -coord2ray2(i.uv[0].x, i.uv[0].y - 0.0, 1, 1, _aspectRatio);

			r.Origin = camera;

			AABB box;
			box.Min = float3(-1, -1, -1)*0.5*_InternalScaleData;
			box.Max = float3(1,1,1)*0.5*_InternalScaleData;
			float t0, t1;


			float planeDir = 0;
			float4 plane = renderPlane(r, float4(0.3, 0.6, 1.0, 0.7)*0.05, box, planeDir);
			float4 val = float4(0,0,0,0);
			if (IntersectBox(r, box, t0, t1)) {
				float3 opos = float3(0,0,0);
				//return float4(1,0,0,1);

	#ifdef SHADING_HARD
				val = raymarch(r.Origin + r.Dir*t0 + float3(0.0, 0.0, 0), r.Dir, box, opos);
	#ifdef HAS_LIGHTING
				float border = clipBorders(opos, box);

				val = (1-border)*val + border*applyLight(val, opos, r.Dir);	
	#endif
	#ifdef HAS_SHADOWS
				val.xyz *= getShadow(opos);
	#endif

	#endif
	#ifdef SHADING_OPACITY
				val = raymarchOpacity(r.Origin + r.Dir*t0 + float3(0.0, 0.0, 0), r.Dir, box);
	#endif

				val.xyz = _IntensityScale*saturateColors(val.xyz);

			}
/*			if (planeDir<=0 && length(val.xyz)<0.01)
				val+=plane;*/
			val+=plane;

			val=val + (1-clamp(0.66*length(val),0,1))*tex2D(_MainTex, i.uv[0]);

			return val;
}


	ENDCG
												
		}
	}

	Fallback off

} // shader
