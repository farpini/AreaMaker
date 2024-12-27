Shader "Custom/StripesShaderUnlit"
{
    Properties
    {
        _Width ("Width", Float) = 0.2
        _Speed ("Speed", Float) = 1.0
        _MainColor ("MainColor", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

        Pass
        {
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float _Speed;
            float _Width;
            float4 _MainColor;
            
            Varyings vert (Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS);
                output.uv = input.uv;  // Use UV coordinates directly
                return output;
            }

            float4 frag (Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                float speedMultiplied = _Speed * 1000;

                // Diagonal stripes
                float diagonalCoord = (input.uv.x + input.uv.y + _Time * speedMultiplied) / _Width;
                float stripe = fmod(diagonalCoord, 2.0);

                float4 colorAlpha = float4(0, 0, 0, 0);

                float4 color = stripe < 1.0 ? _MainColor : colorAlpha;

                return color;
            }
            ENDHLSL
        }
    }
}