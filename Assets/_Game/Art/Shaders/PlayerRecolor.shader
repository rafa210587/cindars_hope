// fable_87 — Recolor por cor-fonte (palette swap) para sprites do player.
// 4 slots de regiao: cada pixel proximo de _SrcN (< _TolN) e remapeado para _DstN
// preservando a luminancia (sombreado intacto). Slot off (_EnaN=0) ou pixel fora de
// toda regiao passa adiante. Aplicado via MaterialPropertyBlock (sem instanciar material).
// Base: shader de sprite transparente built-in (premultiplied).
Shader "CindarsHope/PlayerRecolor"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0

        _Src0 ("Src 0 (Hair)", Color)  = (0.600,0.302,0.071,1)
        _Dst0 ("Dst 0", Color)         = (0.600,0.302,0.071,1)
        _Tol0 ("Tol 0", Range(0,1))    = 0.16
        _Ena0 ("Ena 0", Float)         = 0

        _Src1 ("Src 1 (Shirt)", Color) = (0.914,0.737,0.427,1)
        _Dst1 ("Dst 1", Color)         = (0.914,0.737,0.427,1)
        _Tol1 ("Tol 1", Range(0,1))    = 0.16
        _Ena1 ("Ena 1", Float)         = 0

        _Src2 ("Src 2 (Pants)", Color) = (0.392,0.227,0.067,1)
        _Dst2 ("Dst 2", Color)         = (0.392,0.227,0.067,1)
        _Tol2 ("Tol 2", Range(0,1))    = 0.16
        _Ena2 ("Ena 2", Float)         = 0

        _Src3 ("Src 3 (Metal)", Color) = (0.478,0.451,0.420,1)
        _Dst3 ("Dst 3", Color)         = (0.478,0.451,0.420,1)
        _Tol3 ("Tol 3", Range(0,1))    = 0.16
        _Ena3 ("Ena 3", Float)         = 0
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
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            fixed4 _Color;
            fixed4 _Src0, _Dst0, _Src1, _Dst1, _Src2, _Dst2, _Src3, _Dst3;
            float  _Tol0, _Ena0, _Tol1, _Ena1, _Tol2, _Ena2, _Tol3, _Ena3;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex   = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color    = IN.color * _Color;
                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap(OUT.vertex);
                #endif
                return OUT;
            }

            sampler2D _MainTex;

            float lum(float3 c) { return dot(c, float3(0.299, 0.587, 0.114)); }

            // Recolore px se proximo de src; preserva sombra pela razao de luminancia.
            float3 tryRegion(float3 px, float3 outc, inout bool done,
                             fixed4 src, fixed4 dst, float tol, float ena)
            {
                if (done || ena < 0.5) return outc;
                if (distance(px, src.rgb) > tol) return outc;
                float sl = max(lum(src.rgb), 0.001);
                done = true;
                return saturate(dst.rgb * (lum(px) / sl));
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord);
                float3 px = c.rgb;
                float3 outc = px;
                bool done = false;
                // casa contra a cor ORIGINAL (px); aplica o primeiro slot que bate.
                outc = tryRegion(px, outc, done, _Src0, _Dst0, _Tol0, _Ena0);
                outc = tryRegion(px, outc, done, _Src1, _Dst1, _Tol1, _Ena1);
                outc = tryRegion(px, outc, done, _Src2, _Dst2, _Tol2, _Ena2);
                outc = tryRegion(px, outc, done, _Src3, _Dst3, _Tol3, _Ena3);

                c.rgb = outc * IN.color.rgb;
                c.a  *= IN.color.a;
                c.rgb *= c.a; // premultiplied alpha (Blend One OneMinusSrcAlpha)
                return c;
            }
        ENDCG
        }
    }
}
