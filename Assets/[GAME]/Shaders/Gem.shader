Shader "Unlit/Gem"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Speed ("Speed", Float) = 1
		_Size ("Size", Float) = 1
		_Diagonal ("Diagonal", Range(-1, 1)) = 1
		_Strenght ("Strenght", Range(0, 1)) = 1
		_Offset ("Offset", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Transparent"}
        LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float _Speed, _Size, _Diagonal, _Strenght, _Offset;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                float4 col = tex2D(_MainTex, i.uv);

				float value = i.uv.x + (i.uv.y * _Diagonal);
				value = max(sin(value + (_Time.y * _Speed + _Offset)) - (1.0 - (1.0/_Size)), 0.0);
				value = value * _Size;

				col.xyz = lerp(col, float4(1, 1, 1, col.a), value * _Strenght);

                return col;
            }
            ENDCG
        }
    }
}
