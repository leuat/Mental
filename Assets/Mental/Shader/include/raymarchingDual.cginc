	//	uniform float _RenderType;

	
	float usign(float val) {
		if (val>0)
			return  1;
		return 0;
	}
	
	float clipBorders(float3 pos, AABB box) {
		float border = insideBox(pos, box);
		border *= clamp(sign(dot(_Camera-pos, _CameraDirection)),0,1);
		return border*sign(pointPlane(pos, _SplitPos, _SplitPlane));
	}


	float3 getGaussColor(float val, float center, float spread, float3 col, float strength) {

		float d = val-center;
		float c = clamp(exp(-d*d/(spread*spread)),0,1);
		c*=strength;
		return c*col;

	}

	uniform float _OuterOpacity;

	half4 raymarchOpacity(float3 pos, float3 dir, AABB box)
	{
		float4 dst = 0;
		float3 stepDist = dir * STEP_SIZE*1.25;
		//float _Time = _LTime;
		AABB halfBox = box;
		halfBox.Min*=0.5;
		halfBox.Max*=0.5;
		for (int k = 0; k < STEP_CNT; k++)
		{
			float border = clipBorders(pos,box);
			float borderDetail = clipBorders(pos,halfBox);
/*			float3 shiftt = float3( cos(_LTime*100.4 + 10*noise(pos*4.12) ),
									cos(_LTime*77.4 + 10*noise(pos*4.65) ),
									sin(_LTime*112.3 + 10*noise(pos*3.23) ) ); */
			
//			float4 src = getTex(_VolumeTex, pos + 0.05*shiftt, _InternalScaleData)*border;
			float4 src = getTex(_VolumeTex, pos, _InternalScaleData)*border;
			src.a*=_OuterOpacity;
			float4 src2 = getTex(_VolumeTexDetail, pos*2, _InternalScaleData)*borderDetail;
			src = src*(1-borderDetail) + src2;

//			float border = insideBox(pos, box);

			float v = src.a;
			src.a *= saturate(_Opacity);
			src.a = pow(src.a, _Power);
			src.rgb *= src.a;


			src.rgb += getGaussColor(v, _ColorCutoff, 0.02, float3(0,1,0), _ColorCutoffStrength);

			//float shift = clamp(cos(_Time*100 + 10*noise(pos*5.23)),0,1); 
			float shift = 1;
			src.rgb +=getTex(_VolumeTexDots, pos,_InternalScaleAtlas)*border*_DotStrength*shift;

			
			dst = ((1.0f - dst.a) * src + dst);
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
			float4 src = getTex(_VolumeTex, pos, _InternalScaleData)*clipBorders(pos, box);

			src.xyz *= _InteractColor;
			src.xyz*=src.a;
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

			v2f vert(appdata_img v)
	{
		v2f o;
		o.pos = UnityObjectToClipPos(v.vertex);
		o.uv[0] = v.texcoord.xy;
		o.uv[1] = v.texcoord.xy;
		o.vertex = v.vertex;
		return o;
	}
