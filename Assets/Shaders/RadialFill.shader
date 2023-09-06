﻿﻿// Source: https://github.com/Nrjwolf/unity-shader-sprite-radial-fill

Shader "Custom/RadialFill"
{
	Properties
	{
		[PerRendererData] _MainTex("Sprite Texture", 2D) = "white" {}
		_Color("Tint", Color) = (1,1,1,1)
		_Angle("Angle", Range(0, 360)) = 0
		_Arc1("Arc Point 1", Range(0, 360)) = 15
		_Arc2("Arc Point 2", Range(0, 360)) = 15
		[MaterialToggle] PixelSnap("Pixel snap", Float) = 0
	}

		SubShader
		{
			Tags
			{
				"Queue" = "Transparent"
				"IgnoreProjector" = "True"
				"RenderType" = "Transparent"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off
			Lighting Off
			ZWrite Off
			Blend One OneMinusSrcAlpha

			Pass
			{
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile _ PIXELSNAP_ON
				#include "UnityCG.cginc"

				struct appdata_t
				{
					float4 vertex   : POSITION;
					float4 color    : COLOR;
					float2 texcoord : TEXCOORD0;
				};

				struct v2f
				{
					float4 vertex   : SV_POSITION;
					fixed4 color : COLOR;
					float2 texcoord  : TEXCOORD0;
				};

				fixed4 _Color;

				v2f vert(appdata_t IN)
				{
					v2f OUT;
					OUT.vertex = UnityObjectToClipPos(IN.vertex);
					OUT.texcoord = IN.texcoord;
					OUT.color = IN.color * _Color;
					#ifdef PIXELSNAP_ON
					OUT.vertex = UnityPixelSnap(OUT.vertex);
					#endif

					return OUT;
				}

				sampler2D _MainTex;
				sampler2D _AlphaTex;
				float _AlphaSplitEnabled;
				float _Angle;
				float _Arc1;
				float _Arc2;

				fixed4 SampleSpriteTexture(float2 uv)
				{
					fixed4 color = tex2D(_MainTex, uv);

	#if UNITY_TEXTURE_ALPHASPLIT_ALLOWED
					if (_AlphaSplitEnabled)
						color.a = tex2D(_AlphaTex, uv).r;
	#endif //UNITY_TEXTURE_ALPHASPLIT_ALLOWED

					return color;
				}

				fixed4 frag(v2f IN) : SV_Target
				{
					fixed4 c = SampleSpriteTexture(IN.texcoord) * IN.color;
					c.rgb *= c.a;

					//-------- Creating arc --------//
					// sector start/end angles
					float startAngle = _Angle - _Arc1;
					float endAngle = _Angle + _Arc2;

					// check offsets
					float offset0 = clamp(0, 360, startAngle + 360);
					float offset360 = clamp(0, 360, endAngle - 360);

					// convert uv to atan coordinates
					float2 atan2Coord = float2(lerp(-1, 1, IN.texcoord.x), lerp(-1, 1, IN.texcoord.y));
					float atanAngle = atan2(atan2Coord.y, atan2Coord.x) * 57.3; // angle in degrees

					// convert angle to 360 system
					if (atanAngle < 0) atanAngle = 360 + atanAngle;

					if (atanAngle >= startAngle && atanAngle <= endAngle) discard;
					if (atanAngle <= offset360) discard;
					if (atanAngle >= offset0) discard;

					return c;
				}
			ENDCG
			}
		}
}
