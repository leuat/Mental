// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LemonSpawn/Ray Marching"
{

	CGINCLUDE


#include "UnityCG.cginc"
#pragma target  3.0
#pragma profileoption MaxLocalParams=1024 
#pragma profileoption NumInstructionSlots=4096
#pragma profileoption NumMathInstructionSlots=4096

//#pragma multi_compile SHADING_HARD SHADING_OPACITY 
//#pragma multi_compile HAS_SHADOWS HAS_LIGHTING

#pragma multi_compile __ SHADING_HARD 
#pragma multi_compile __ SHADING_OPACITY 
#pragma multi_compile __ HAS_SHADOWS 
#pragma multi_compile __ HAS_LIGHTING

	struct v2f {
		float4 pos : POSITION;
		float2 uv[2] : TEXCOORD0;
		float4 vertex : TEXCOORD2;
	};

	sampler3D _VolumeTex;
	float _Opacity;

	uniform float4x4 _ViewMatrix;
	uniform float3 _Camera;
	uniform float _Perspective;
	uniform float3 _SplitPlane;
	uniform float3 _SplitPos;
	uniform float _Cutoff;
	uniform float _Shininess;
	uniform float3 _InteractColor;
	uniform float _Saturation;
	//	uniform float _RenderType;

#define S 192

#define TOTAL_STEPS S
#define STEP_CNT S
#define STEP_SIZE 1 / S




	#include "include/util.cginc"


	float clipBorders(float3 pos, AABB box) {
		float border = insideBox(pos, box);
		return border*sign(pointPlane(pos, _SplitPos, _SplitPlane));
	}


	half4 raymarchOpacity(float3 pos, float3 dir, AABB box)
	{
		float4 dst = 0;
		float3 stepDist = dir * STEP_SIZE;
		for (int k = 0; k < STEP_CNT; k++)
		{
			float4 src = getTex(pos)*clipBorders(pos,box);
//			float border = insideBox(pos, box);

			src.a *= saturate(_Opacity);
			src.rgb *= src.a;
			dst = (1.0f - dst.a) * src + dst;
			pos += stepDist;
		}
		//		dst.x -=1;
		return dst * 2;
	}



	half4 raymarch(float3 pos, float3 dir, AABB box, out float3 opos)
	{
		float4 dst = float4(0, 0, 0, 1);
		float3 stepDist = dir * STEP_SIZE;
		opos = pos;
		for (int k = 0; k < STEP_CNT; k++)
		{
			float4 src = getTex(pos)*clipBorders(pos, box);

			src.xyz *= _InteractColor;
			//dst.xyz += src.xyz;
			if (length(src.xyz) > 4 * _Cutoff) {
				//dst = src;
				dst.xyz = normalize(src.xyz);
				dst.a = 1;
				opos = pos;
				break;
			}

			//dst += src*0.001;

			pos += stepDist;
		}

		//		dst.x -=1;
		return dst;
	}




	ENDCG

		Subshader{
		//ZTest Always Cull Off ZWrite Off
		Fog{ Mode off }

		Pass
	{
		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		v2f vert(appdata_img v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv[0] = v.texcoord.xy;
		o.uv[1] = v.texcoord.xy;
		o.vertex = v.vertex;
		/*#if SHADER_API_D3D9
		if (_MainTex_TexelSize.y < 0)
		o.uv[0].y = 1 - o.uv[0].y;
		#endif*/
		return o;
	}


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
		
		half4 frag(v2f i) : COLOR{
			float3 camera = _Camera;

			Ray r;
			r.Dir = -coord2ray2(i.uv[0].x, i.uv[0].y - 0.0, 1, 1);

			r.Origin = camera;

			AABB box;
			box.Min = float3(-1, -1, -1) * 0.5;
			box.Max = float3(1,1,1) * 0.5;
			float t0, t1;

			float planeDir = 0;
			float4 plane = renderPlane(r, float4(0.3, 0.6, 1.0, 1)*0.2, box, planeDir);
			float4 val = float4(0,0,0,0);
			if (IntersectBox(r, box, t0, t1)) {
				float3 opos = float3(0,0,0);


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


				val.xyz = saturateColors(val.xyz);

			}
/*			if (planeDir<=0 && length(val.xyz)<0.01)
				val+=plane;*/
			val+=plane;
			return val;


}
ENDCG
												}
	}

		Fallback off

} // shader