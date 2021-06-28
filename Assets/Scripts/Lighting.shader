Shader "Custom/Lighting"
{
	Properties
	{
		_MainTex("_MainTex", 2D) = "white" {}
		_LightValues("_LightValues", 2D) = "white" {}
	}
		SubShader
		{
			// No culling or depth
			Cull Off ZWrite Off ZTest Always
			//Blend SrcAlpha OneMinusSrcAlpha

			Pass
			{
				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				struct appdata
				{
					float4 vertex : POSITION;
					float2 uv : TEXCOORD0;
				};

				struct v2f
				{
					float2 uv : TEXCOORD0;
					float4 vertex : SV_POSITION;
				};

				v2f vert(appdata v)
				{
					//do SOMETHING in here??
					v2f o;
					o.vertex = UnityObjectToClipPos(v.vertex);
					o.uv = v.uv;
					return o;
				}

				sampler2D _MainTex;
				sampler2D _LightValues;

				fixed4 frag(v2f i) : SV_Target
				{
					fixed4 col = tex2D(_MainTex, i.uv);
					//float2 offset = float2(_XOffset, _YOffset);

					fixed4 lightingcol = tex2D(_LightValues, i.uv /*+ offset*/ );
/*
					if (i.uv.x > (1 - _UnitInUV)) {
						lightingcol = tex2D(_LightValues, i.uv - _UnitInUV);
					}
					else if (i.uv.x < (_UnitInUV)) {
						lightingcol = tex2D(_LightValues, i.uv + _UnitInUV);
					}*/


					lightingcol.rgb *= col.rgb;

					return lightingcol;
				}
				ENDCG
			}
		}
}
