Shader "Mask/Soft Mask"
{
	Properties
	{
		[PerRendererData] _MainTex ("Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		[Toggle] _ClampHoriz ("Clamp Alpha Horizontally", Float) = 0
		[Toggle] _ClampVert ("Clamp Alpha Vertically", Float) = 0
		[Toggle] _UseAlphaChannel ("Use Mask Alpha Channel (not RGB)", Float) = 0
        _MaskRotation ("Mask Rotation in Radians", Float) = 0
		_AlphaTex ("Alpha Mask", 2D) = "white" {}
		_ClampBorder ("Clamping Border", Float) = 0.01
		[KeywordEnum(X, Y, Z)] _Axis ("Alpha Mapping Axis", Float) = 0
		_IsThisText("Is This Text?", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend One OneMinusSrcAlpha
		
		Pass
		{
		CGPROGRAM
			#include "UnityCG.cginc"
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DUMMY PIXELSNAP_ON
			#pragma multi_compile DUMMY _CLAMPHORIZ_ON
			#pragma multi_compile DUMMY _CLAMPVERT_ON
			#pragma multi_compile DUMMY _USEALPHACHANNEL_ON
			#pragma multi_compile DUMMY _AXIS_X
			#pragma multi_compile DUMMY _AXIS_Y
			#pragma multi_compile DUMMY _AXIS_Z
			#pragma multi_compile DUMMY _SCREEN_SPACE_UI

			sampler2D _MainTex;
			sampler2D _AlphaTex;
			float _ClampBorder;
			float _MaskRotation;
			
			float _Blend;

			float _IsThisText;

			struct appdata_t
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			
			struct v2f
			{
				float4 pos : SV_POSITION;
				fixed4 color : COLOR;
				float2 uvMain : TEXCOORD1;
				float2 uvAlpha : TEXCOORD2;
			};
			
			fixed4 _Color;
			float4 _MainTex_ST;
			float4 _AlphaTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.vertex);
				o.uvMain = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color * _Color;
				
				o.uvAlpha = float2(0, 0);
				#ifdef _SCREEN_SPACE_UI
					#ifdef _AXIS_X
					o.uvAlpha = v.vertex.zy;
					#elif _AXIS_Y
					o.uvAlpha = v.vertex.xz;
					#elif _AXIS_Z
					o.uvAlpha = v.vertex.xy;
					#endif
				#else
					#ifdef _AXIS_X
					o.uvAlpha = mul(unity_ObjectToWorld, v.vertex).zy;
					#elif _AXIS_Y
					o.uvAlpha = mul(unity_ObjectToWorld, v.vertex).xz;
					#elif _AXIS_Z
					o.uvAlpha = mul(unity_ObjectToWorld, v.vertex).xy;
					#endif
				#endif

				float s = sin(_MaskRotation);
				float c = cos(_MaskRotation);
				float2x2 rotationMatrix = float2x2(c, -s, s, c);
				o.uvAlpha = mul(o.uvAlpha, rotationMatrix);

				o.uvAlpha = o.uvAlpha * _AlphaTex_ST.xy + _AlphaTex_ST.zw;

				#ifdef PIXELSNAP_ON
				o.pos = UnityPixelSnap(o.pos);
				#endif
				
				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				float2 alphaCoords = i.uvAlpha;
				
				#ifdef _CLAMPHORIZ_ON
				alphaCoords.x = clamp(alphaCoords.x, _ClampBorder, 1.0 - _ClampBorder);
				#endif
				
				#ifdef _CLAMPVERT_ON
				alphaCoords.y = clamp(alphaCoords.y, _ClampBorder, 1.0 - _ClampBorder);
				#endif
		
				half4 texcol = tex2D(_MainTex, i.uvMain);
				texcol.a *= i.color.a;
				texcol.rgb = clamp(texcol.rgb + _IsThisText, 0, 1) * i.color.rgb;
				
				#ifdef _USEALPHACHANNEL_ON
				texcol.a *= tex2D(_AlphaTex, alphaCoords).a;
				#endif
				
				#ifndef _USEALPHACHANNEL_ON
				texcol.a *= tex2D(_AlphaTex, alphaCoords).rgb;
				#endif
				
				texcol.rgb *= texcol.a;
				
				return texcol;
			}
			
		ENDCG
		}
	}
	
	Fallback "Unlit/Texture"
}
