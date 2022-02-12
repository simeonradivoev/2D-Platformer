// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Sprites/Default (Motion Blured)"
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
		_LightBlockColor("Light Block Color",Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off

        Pass
        {
			Name "Sprite"
			Tags 
			{
				"Queue"="Geometry"
				"LightMode" = "ForwardBase" 
			}
			Lighting Off
			ZWrite Off
			Blend One OneMinusSrcAlpha

        CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment SpriteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"
        ENDCG
        }

		Pass
        {
			Name "SpriteMotionVectors"
            Tags 
			{ 
				"LightMode" = "MotionVectors" 
				"Queue"="Transparent"
			}
            ZTest LEqual
            ZWrite Off

            CGPROGRAM
            #pragma vertex VertMotionVectors
            #pragma fragment FragMotionVectors
            #pragma target 3.5
			#pragma multi_compile_instancing
			#include "UnitySprites.cginc"
            #include "Motion.cginc"
            ENDCG
        }
    }
}
