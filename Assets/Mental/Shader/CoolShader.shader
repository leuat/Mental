Shader "CoolShader"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100



		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

#pragma target 4.0



		inline float iqhash(float n)
	{
		return frac(sin(n)*753.5453123);

	}

	float noise3(float3 x)
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


			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 realPosition: TEXCOORD2;

			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			uniform float myTime;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.realPosition = v.vertex.xyz;
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.vertex.x += noise3(float3(o.vertex.x*1.2 + myTime, 0, 0));
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			



			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				float3 add = float3(myTime*10, sin(myTime*25), 0);
				fixed4 col = float4(1,0,0,1)*noise3(i.realPosition.xyz*25.1 + add);
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}
