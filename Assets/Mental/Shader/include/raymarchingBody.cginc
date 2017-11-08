

		#include "include/util.cginc"
		#include "include/raymarching.cginc"




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
			r.Dir = -coord2ray2(i.uv[0].x, i.uv[0].y - 0.0, 1, 1,1);

			r.Origin = camera;

			AABB box;
			box.Min = float3(-1, -1, -1)*0.5*_InternalScaleData;
			box.Max = float3(1,1,1)*0.5*_InternalScaleData;
			float t0, t1;


			float planeDir = 0;
			float4 plane = renderPlane(r, float4(0.3, 0.6, 1.0, 0.7)*0.06, box, planeDir);
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
			return val;
}
