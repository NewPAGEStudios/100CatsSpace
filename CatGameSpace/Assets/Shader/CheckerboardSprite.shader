Shader "Custom/CheckerboardSprite"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _PixelSize ("Box Size (in pixels)", Float) = 16
        _Color1 ("Color 1", Color) = (0,0,0,1)
        _Color2 ("Color 2", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize; // (1/width, 1/height, width, height)
            float _PixelSize;
            fixed4 _Color1;
            fixed4 _Color2;

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 texResolution = float2(_MainTex_TexelSize.z, _MainTex_TexelSize.w);
                float2 pixelCoord = i.uv * texResolution;

                float2 tileCoord = floor(pixelCoord / _PixelSize);
                float checker = fmod(tileCoord.x + tileCoord.y, 2);

                fixed4 col = lerp(_Color1, _Color2, checker);
                fixed4 spriteColor = tex2D(_MainTex, i.uv);
                col.a *= spriteColor.a;

                return col;
            }
            ENDCG
        }
    }
}
