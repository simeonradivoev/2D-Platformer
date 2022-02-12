// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Sprites/Default (Motion Blured, Light Blocker)"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector] _RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
		_LightBlockColor ("Light Block Color",Color) = (1,1,1,1)
		_TextureWeight ("Texture Weight",Range(0,1)) = 1
    }

		SubShader
		{
			Tags 
			{
				"RenderType" = "LightBlocker"
				"IgnoreProjector" = "True"
				"PreviewType" = "Plane"
				"CanUseSpriteAtlas" = "True"
			}

			Cull Off

			Pass
			{
				Name "LightBlock"
				Tags
				{
					"LightMode" = "LightBlocker"
				}
				Lighting Off
				ZWrite Off
				Blend One OneMinusSrcAlpha

			CGPROGRAM
				#pragma vertex SpriteVert
				#pragma fragment SpriteFragCustom
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile _ PIXELSNAP_ON
				#pragma multi_compile _ ETC1_EXTERNAL_ALPHA
				#include "UnitySprites.cginc"

				half4 _LightBlockColor;

				fixed4 SpriteFragCustom(v2f IN) : SV_Target
				{
					fixed4 c = SpriteFrag(IN);
					c *= _LightBlockColor;
					return c;
				}
			ENDCG
			}

			UsePass "Sprites/Default (Motion Blured)/Sprite"

			UsePass "Sprites/Default (Motion Blured)/SpriteMotionVectors"
		}
}
