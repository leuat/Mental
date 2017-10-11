	struct v2f {
		float4 pos : POSITION;
		float2 uv[2] : TEXCOORD0;
		float4 vertex : TEXCOORD2;
	};

	sampler3D _VolumeTex;
	sampler3D _VolumeTexDots;
	float _Opacity;

	uniform float4x4 _ViewMatrix;
	uniform float3 _Camera;
	uniform float3 _CameraDirection;
	uniform float _Perspective;
	uniform float3 _SplitPlane;
	uniform float3 _SplitPos;
	uniform float _Cutoff;
	uniform float _Shininess;
	uniform float3 _InteractColor;
	uniform float _Saturation;
	uniform float _IntensityScale;
	uniform float _Power;
	uniform float3 _LightDir;
	uniform float _ColorCutoff;
	uniform float _ColorCutoffStrength;
	uniform float _DotStrength;




	inline float iqhash(float n)
{
	return frac(sin(n)*753.5453123);
	
/*	float p = 50.0*frac(n*0.3183099 + 0.71);
	return -1.0 + 2.0*frac(n*n+(0.23*n));*/

}

float noise(float3 x)
{

	float3 p = floor(x);
	float3 f = frac(x);

	f = f*f*(3.0 - 2.0*f);
	float n = p.x + p.y*157.0 + 113.0*p.z;

	return lerp(lerp(lerp(iqhash(n + 0.0), iqhash(n + 1.0), f.x),
		lerp(iqhash(n + 157.0), iqhash(n + 158.0), f.x), f.y),
		lerp(lerp(iqhash(n + 113.0), iqhash(n + 114.0), f.x),
			lerp(iqhash(n + 270.0), iqhash(n + 271.0), f.x), f.y), f.z);


}


	struct Ray {
		float3 Origin;
		float3 Dir;
	};
	struct AABB {
		float3 Min;
		float3 Max;
	};


	float boxLength(float3 d) {
		float m = max(d.x, d.y);
		m = max(m, d.z);
		return m;
	}

	float RayIntersectPlane(Ray ray, Ray plane, out float3 intersectionPoint)
	{
  		float rDotn = dot(ray.Dir, plane.Dir);

  		float s = dot(plane.Dir, (plane.Origin - ray.Origin)) / rDotn;
	
  		intersectionPoint = ray.Origin + s * ray.Dir;
  		return rDotn;
}

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

	inline float4 getTex(sampler3D tex, float3 pos) {
		return  tex3D(tex, pos + float3(0.5, 0.5, 0.5));
	}

	inline float getI(float3 pos) {
		return  length(tex3D(_VolumeTex, pos + float3(0.5, 0.5, 0.5)).xyz);
	}

	float3 saturateColors(float3 col) {
		float c = (col.r + col.g + col.b)/3;
		float3 tmp = float3(c-col.r, c-col.g, c-col.b);
		col.r = c - _Saturation*tmp.x;
		col.g = c - _Saturation*tmp.y;
		col.b = c - _Saturation*tmp.z;
		return col;
	}	

	float3 getNormal(float3 pos) {
		float d = 0.02;
		float dd = 2 * d;
		float3 x = float3(1, 0, 0);
		float3 y = float3(0, 1, 0);
		float3 z = float3(0, 0, 1);
		float3 N = float3((getI(pos - d*x) - getI(pos + d*x)),
			(getI(pos - d*y) - getI(pos + d*y)),
			(getI(pos - d*z) - getI(pos + d*z)));
		return normalize(N);

	}

	float pointPlane(float3 pos, float3 planePos, float3 planeNormal) {
		float3 p = planePos - pos;
		return clamp(-dot(p, planeNormal), 0, 1);
	}


	inline float getShadow(float3 pos) {
		float l = 1;
		float step = 0.05;
		for (int i = 0; i < 10; i++) {
			float3 p = pos + _LightDir * 1 * step;
			l = l - getTex(_VolumeTex, p).a*0.4*pointPlane(p, _SplitPos, _SplitPlane);;
		}

		return clamp(l, 0.25, 1);

	}

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


	half4 applyLight(half4 val, float3 opos, float3 viewDirection) {
			float3 normal = getNormal(opos);
			float diffuseLight = clamp(dot(normal, _LightDir) + 0.2,0.04,1);


			float specularReflection = float3(0.9,0.8,0.7) * pow(max(0.0, dot(
						reflect(-_LightDir, normal),
						viewDirection*-1)), _Shininess);

	//}		
			// Make sure only front is shown
			specularReflection = clamp(specularReflection*sign (dot(normal, _LightDir)),0,1);



			float scaling = clamp(pointPlane(opos, _SplitPos, _SplitPlane)*10.0 + 0.02,0,1);
			val.xyz = (val.xyz*diffuseLight + specularReflection)*scaling + (1-scaling)*val.xyz;
//			val.xyz *= diffuseLight;
//			val.xyz += specularReflection;

			return val;

		}
