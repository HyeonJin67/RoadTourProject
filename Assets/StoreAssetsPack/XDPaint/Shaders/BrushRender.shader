Shader "XD Paint/Brush Render"
{
    Properties
    {
        _MainTex ("Main", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
		_RadiusX ("Radius X", Range(0, 1)) = 1
        _RadiusY ("Radius Y", Range(0, 1)) = 1
        _Hardness ("Hardness", Range(-20, 1)) = 0.9
        _Offset ("Offset", Float) = 0
        _ScaleUV ("Scale UV", Float) = 0
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
            uniform float4 _MainTex_TexelSize;
            float4 _Color;
		    float _RadiusX;
            float _RadiusY;
            float _Hardness;
            float _ScaleUV;
            float _Offset;

            float4 frag (v2f_img i) : SV_Target
            {
                float2 uv = i.uv.xy * float2(_ScaleUV, _ScaleUV) - float2(_Offset, _Offset);
                float4 color = tex2D(_MainTex, uv) * _Color;
                float uvX = uv.x / _ScaleUV;
                float uvY = uv.y / _ScaleUV;
                if (uvX + _MainTex_TexelSize.x <= _MainTex_TexelSize.x || //left
                    uvX + _MainTex_TexelSize.x >= 1.0f / _ScaleUV + _MainTex_TexelSize.x || //right 
                    uvY + _MainTex_TexelSize.y <= _MainTex_TexelSize.y || //bottom
                    uvY + _MainTex_TexelSize.y >= 1.0f / _ScaleUV + _MainTex_TexelSize.y) //top
                {
                    color.a = 0;
                }   
                else if (_RadiusX > 0.0001 && _RadiusY > 0.0001 && _Hardness < 1.0) 
                {
                    float x = 2 * (uv.x - 0.5) / _RadiusX;
                    float y = 2 * (uv.y - 0.5) / _RadiusY;
                    float value = x * x + y * y;
                    color.a *= smoothstep(1.0, _Hardness, value);
                }
			    return color;
            }
            ENDCG
        }
    }
}