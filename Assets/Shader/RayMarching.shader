// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LemonSpawn/Ray Marching"
{

	CGINCLUDE

#include "UnityCG.cginc"
#pragma target 4.0
#pragma profileoption MaxLocalParams=1024 
#pragma profileoption NumInstructionSlots=4096
#pragma profileoption NumMathInstructionSlots=4096

		struct v2f {
		float4 pos : POSITION;
		float2 uv[2] : TEXCOORD0;
		float3 opos : TEXCOORD2;
	};

	sampler3D _VolumeTex;
	float4 _VolumeTex_TexelSize;

	sampler2D _FrontTex;
	sampler2D _BackTex;

	float4 _LightPos;

	float _Dimensions;

	float _Opacity;
	float4 _ClipDims;
	float4 _ClipPlane;


#define TOTAL_STEPS 128.0
#define STEP_CNT 128
#define STEP_SIZE 1 / 128.0


	struct Ray {
		float3 Origin;
		float3 Dir;
	};
	struct AABB {
		float3 Min;
		float3 Max;
	};


	bool IntersectBox(Ray r, AABB aabb, out float t0, out float t1)
	{
		float3 invR = 1.0 / r.Dir;
		float3 tbot = invR * (aabb.Min - r.Origin);
		float3 ttop = invR * (aabb.Max - r.Origin);
		float3 tmin = min(ttop, tbot);
		float3 tmax = max(ttop, tbot);
		float2 t = max(tmin.xx, tmin.yz);
		t0 = max(t.x, t.y);
		t = min(tmax.xx, tmax.yz);
		t1 = min(t.x, t.y);
		return t0 <= t1;
	}

	float insideBox(float3 v, AABB box) {
		float3 s = step(box.Min, v) - step(box.Max, v);
		return s.x * s.y*s.z;
	}

	inline float4 getTex(float3 pos) {
		return  tex3D(_VolumeTex, pos + float3(0.5, 0.5, 0.5));
	}

	inline float getI(float3 pos) {
		return  tex3D(_VolumeTex, pos + float3(0.5, 0.5, 0.5)).a;
	}

	uniform float3 _LightDir;

	float3 getNormal(float3 pos) {
		float d = 0.02;
		float dd = 2 * d;
		float3 x = float3(1, 0, 0);
		float3 y = float3(0, 1, 0);
		float3 z = float3(0, 0, 1);
		float3 N = float3((getI(pos - d*x) - getI(pos + d*x)),
			(getI(pos - d*y) - getI(pos + d*y)) / dd,
			(getI(pos - d*z) - getI(pos + d*z)) / dd);
		return normalize(N);

	}

	inline float getShadow(float3 pos) {
		float l = 1;
		float step = 0.03;
		for (int i = 0; i < 10; i++) {
			l = l - getTex(pos + _LightDir * 1 * step).a*0.4;
		}

		return clamp(l, 0, 1);

	}


	half4 raymarch(float3 pos, float3 dir, AABB box, out float3 opos)
	{
		//float3 frontPos = float3(i.uv[0], -1);
		//float3 backPos  = float3(i.uv[1], 1);
		//float3 frontPos = tex2D(_FrontTex, i.uv[1]).xyz;
		//float3 backPos = tex2D(_BackTex, i.uv[1]).xyz;


//		float3 dir = backPos - frontPos;
	//	float3 pos = frontPos;
		float4 dst = 0;
		float3 stepDist = dir * STEP_SIZE;
		opos = pos;
		for (int k = 0; k < STEP_CNT; k++)
		{


			float4 src = getTex(pos);


			float3 border = insideBox(pos, box);

			src.a *= saturate(_Opacity * border);
			src.rgb *= src.a;
			dst = (1.0f - dst.a) * src + dst;
			if (length(dst) > 0.15) {
				dst *= 10;
				opos = pos;
				break;
			}

			//dst += src*0.001;

			pos += stepDist;
		}
		//		dst.x -=1;
		return dst * 2;
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
		o.opos = v.vertex.xyz;
		/*#if SHADER_API_D3D9
		if (_MainTex_TexelSize.y < 0)
		o.uv[0].y = 1 - o.uv[0].y;
		#endif*/
		return o;
	}
	uniform float4x4 _ViewMatrix;
	uniform float3 _Camera;
	uniform float _Perspective;

	float3 coord2ray2(float x, float y, float width, float height) {

		float aspect_ratio = 1;
		float FOV = _Perspective / 360.0f * 2 * 3.14159; // convert to radians
		float dx = tan(FOV*0.5f)*(x / (width / 2) - 1.0f) / aspect_ratio;
		float dy = tan(FOV*0.5f)*(1 - y / (width / 2));


		float far = 10;

		float3 Pfar = float3(dx*far, dy*far, far);
		float3 res = mul(_ViewMatrix,Pfar);

		return normalize(res);
	}

		half4 frag(v2f i) : COLOR{
			//return half4(1, 0, 0, 1);
				//t = 0;
			float3 camera = _Camera;

				Ray r;
				r.Dir = coord2ray2(i.uv[0].x, i.uv[0].y - 0.0, 1, 1);

				r.Origin = camera;

				AABB box;
				box.Min = float3(-1, -1, -1) * 0.5;
				box.Max = float3(1,1,1) * 0.5;
				float t0, t1;
				if (IntersectBox(r, box, t0, t1)) {
					float3 opos;
					float4 val = raymarch(r.Origin + r.Dir*t1 + float3(0.0, 0.0, 0), r.Dir*-1, box, opos);
					float l = getShadow(opos);
					val.xyz *= l;
					return val;
				}
			return half4(0, 0, 0, 1);


		}
	ENDCG
													}
	}

		Fallback off

} // shader