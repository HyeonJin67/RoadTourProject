Shader "XD Paint/Brush Sampler" {
    Properties {
        _MainTex ("Main", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
        _BrushTex ("Brush", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _BrushOffset ("Brush offset", Vector) = (0, 0, 0, 0)
    }
    
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"}
        Cull Off Lighting Off ZTest Off ZWrite Off Fog { Color (0, 0, 0, 0) }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma fragmentoption ARB_precision_hint_fastest
            #include "UnityCG.cginc"
 
            sampler2D _MainTex;
            sampler2D _MaskTex;
            uniform float4 _BrushTex_TexelSize;
            float4 _BrushOffset;
            float4 _Color;

            float4 frag (v2f_img i) : SV_Target
            {
                float2 uv = float2(
                    i.uv.x * _BrushOffset.z + _BrushOffset.x * _BrushOffset.z - _BrushOffset.z / 2,
                    i.uv.y * _BrushOffset.w + _BrushOffset.y * _BrushOffset.w - _BrushOffset.w / 2
                );
                float4 color = tex2D(_MainTex, uv) * _Color;
                if (i.uv.x <= _BrushTex_TexelSize.x || i.uv.x >= 1.0f - _BrushTex_TexelSize.x || 
                    i.uv.y <= _BrushTex_TexelSize.y || i.uv.y >= 1.0f - _BrushTex_TexelSize.x)
                {
                    color.a = 0;
                }
                float4 mask = tex2D(_MaskTex, i.uv) * _Color;
                color.a *= mask.a;
			    return color;
            }
            ENDCG
        }
    }
}