Shader "Custom/UIBlur"
{
    Properties
    {
        // Mipmapped RenderTexture fed in by PopupManager at runtime
        _MainTex ("Mipped Screenshot", 2D) = "white" {}

        // Which mip level to sample.
        // 0 = full res (sharp), 3 = 1/8 res (blurry), 5 = 1/32 res (very blurry).
        // Fractional values (e.g. 3.5) trilinearly blend between mip levels — no pixelation!
        _MipLevel ("Mip Level (Blur)", Range(0, 8)) = 3.5

        // Tint color drawn on top of the blurred image (use alpha for opacity)
        _TintColor ("Tint Color", Color) = (0, 0, 0, 0.5)

        [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
        [HideInInspector] _Stencil ("Stencil ID", Float) = 0
        [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
        [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
        [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
        [HideInInspector] _ColorMask ("Color Mask", Float) = 15
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

            sampler2D _MainTex;
            float     _MipLevel;
            fixed4    _TintColor;

            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f    { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // ── Mipmap LOD Blur ────────────────────────────────────────────────
                // tex2Dlod samples the texture at an explicit LOD (mip level).
                // Because the RenderTexture uses FilterMode.Trilinear, fractional
                // _MipLevel values trilinearly interpolate between adjacent mip levels.
                // This gives hardware-native C1-smooth blur at any strength — zero
                // custom kernel code, zero pixelation, one texture fetch per pixel.
                fixed4 col = tex2Dlod(_MainTex, float4(i.uv, 0, _MipLevel));

                // Blend tint on top
                col.rgb = lerp(col.rgb, _TintColor.rgb, _TintColor.a);
                col.a   = 1.0;
                return col;
            }
            ENDCG
        }
    }
}
