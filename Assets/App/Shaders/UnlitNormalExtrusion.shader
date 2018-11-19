Shader "Custom/UnlitNormalExtrusion"
{
	Properties
	{
		_ScaleBuffer("Scale Buffer", 2D) = "white" {}
		_Color ("Color", color) = (0, 0, 0, 0)
		_Offset("Dot offset", Vector) = (0, 0, 0, 0)
		_Amount("Extrusion Amount", Range(0,5)) = 0.5
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
			// #pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			float4 _Color;
			sampler2D _ScaleBuffer;
			float2 _Offset;
			float _Amount;

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};
			
			v2f vert (appdata v)
			{
				v2f o;
				//o.vertex = UnityObjectToClipPos(v.vertex);
				float4 uv = float4(_Offset, 0, 0);
				float4 p = tex2Dlod(_ScaleBuffer, uv);
				o.vertex = v.vertex;
				o.vertex.xyz += v.normal * _Amount * p.y;

				o.vertex = UnityObjectToClipPos(o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = _Color;
				return col;
			}
			ENDCG
		}
	}
}
