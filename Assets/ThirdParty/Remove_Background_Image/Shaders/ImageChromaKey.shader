Shader "Custom/ImageChromaKey"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _KeyColor1 ("Key Color 1", Color) = (0,0,0,0)
        _KeyColor2 ("Key Color 2", Color) = (0,0,0,0)
        _KeyColor3 ("Key Color 3", Color) = (0,0,0,0)
        _KeyColor4 ("Key Color 4", Color) = (0,0,0,0)
        _KeyColor5 ("Key Color 5", Color) = (0,0,0,0)
        _Threshold ("Ngưỡng xóa (Threshold)", Range(0,1)) = 0.5
        _Softness ("Độ mềm viền (Softness)", Range(0,1)) = 0.2
        
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            
            float4 _KeyColor1;
            float4 _KeyColor2;
            float4 _KeyColor3;
            float4 _KeyColor4;
            float4 _KeyColor5;
            
            float _Threshold;
            float _Softness;

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.texcoord) * i.color;

                float minDistance = 1000.0;
                bool hasColor = false;

                if (_KeyColor1.a > 0) { minDistance = min(minDistance, distance(col.rgb, _KeyColor1.rgb)); hasColor = true; }
                if (_KeyColor2.a > 0) { minDistance = min(minDistance, distance(col.rgb, _KeyColor2.rgb)); hasColor = true; }
                if (_KeyColor3.a > 0) { minDistance = min(minDistance, distance(col.rgb, _KeyColor3.rgb)); hasColor = true; }
                if (_KeyColor4.a > 0) { minDistance = min(minDistance, distance(col.rgb, _KeyColor4.rgb)); hasColor = true; }
                if (_KeyColor5.a > 0) { minDistance = min(minDistance, distance(col.rgb, _KeyColor5.rgb)); hasColor = true; }

                if (!hasColor) minDistance = 1000.0;

                float alpha = smoothstep(_Threshold - _Softness, _Threshold + _Softness, minDistance);

                col.a *= alpha;
                return col;
            }
            ENDCG
        }
    }
}
