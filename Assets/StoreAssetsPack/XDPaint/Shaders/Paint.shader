Shader "XD Paint/Paint"
{
    Properties
    {
        _MainTex ("Main", 2D) = "white" {}
        _PaintTex ("Paint", 2D) = "white" {}
        _InputTex ("Draw", 2D) = "white" {}
        _BrushTex ("Brush", 2D) = "white" {}
        _BrushOffset ("Brush offset", Vector) = (0, 0, 0, 0)
        _Color ("Main Color", Color) = (1, 1, 1, 1)
        
	    [Header(Blending)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcColorBlend ("__srcC", Int) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstColorBlend ("__dstC", Int) = 10
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcAlphaBlend ("__srcA", Int) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] _DstAlphaBlend ("__dstA", Int) = 10
    }
    
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"}
        Cull Off
    	Lighting Off 
    	ZWrite Off
        ZTest Always
    	Fog { Color (0, 0, 0, 0) }

    	Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
        	//Background
            SetTexture [_MainTex]
            {
                combine texture
            }
        }
        Pass
        {
        	//Paint
 			Blend [_SrcColorBlend] [_DstColorBlend], [_SrcAlphaBlend] [_DstAlphaBlend]
            SetTexture [_PaintTex]
            {
                combine texture
            }
        }
        Pass
        {
        	//Blend
        	Blend [_SrcColorBlend] [_DstColorBlend], [_SrcAlphaBlend] [_DstAlphaBlend]
            CGINCLUDE
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
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			sampler2D _PaintTex;
			sampler2D _InputTex;
			ENDCG

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			float4 frag (v2f i) : SV_Target
			{
				float4 paintColor = tex2D(_PaintTex, i.uv);
				float4 inputColor = tex2D(_InputTex, i.uv);
				float4 color = paintColor * (1.0f - inputColor.a) + float4(inputColor.r, inputColor.g, inputColor.b, 1) * inputColor.a;
				return color;
			}
			ENDCG
        }
        Pass
        {
        	//Erase
        	Blend Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag2

			float4 frag2 (v2f i) : SV_Target
			{
				float4 paintColor = tex2D(_PaintTex, i.uv);
				float4 inputColor = tex2D(_InputTex, i.uv);
				paintColor.a -= inputColor.a;
				return paintColor;
			}
			ENDCG
        }
        Pass
        {
        	//Preview
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"
            sampler2D _BrushTex;
            float4 _BrushOffset;
            float4 _Color;
       
            float4 frag (v2f_img i) : SV_Target
            {
                float4 result = tex2D(_BrushTex, float2(i.uv.x / _BrushOffset.z - _BrushOffset.x + 0.5f, i.uv.y / _BrushOffset.w - _BrushOffset.y + 0.5f)) * _Color;
                return result;
            }
            ENDCG
        }
    }
}