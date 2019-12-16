Shader "Custom/BlinkShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _CoeffDissolve("Dissolve", Range(0,1)) = 0
        _CenterUV("CenterUV", Vector) = (0,0,0, 0)
        _ToBorder("ToBorderUV" , Vector) = (0.5 , 0.5, 0, 0)
        _BlurRatio("BlurRatio", Range(0,1)) = 0.25
        [MaterialToggle] _ExtToInt("extToInt", Float) = 0
    }
    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent"}
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows alpha:fade

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        fixed4 _Color;
        float _CoeffDissolve;
        float _ExtToInt;
        float2 _CenterUV;
        float2 _ToBorder;
        float _BlurRatio;

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = 0;
            o.Smoothness = 0;
            float dist = distance(_CenterUV, IN.uv_MainTex) / distance(_CenterUV, _CenterUV + _ToBorder);
            o.Alpha = c.a;

            float minusBlurRatio = 1 - _BlurRatio;

            if (_ExtToInt != 0)
            {
                if (dist < _CoeffDissolve && dist > _CoeffDissolve * minusBlurRatio)
                {
                    o.Alpha = lerp(0, o.Alpha, (dist - (_CoeffDissolve * minusBlurRatio)) / (_CoeffDissolve * _BlurRatio));
                }
                else
                {
                    if (dist < _CoeffDissolve * minusBlurRatio)
                        discard;
                }      
            }

            if (_ExtToInt == 0)
            {
                if (dist > _CoeffDissolve && dist < _CoeffDissolve + _BlurRatio)
                {
                    o.Alpha = lerp(o.Alpha,0, (dist - _CoeffDissolve) / _BlurRatio);
                }
                else
                {
                    if (dist > _CoeffDissolve + _BlurRatio)
                    {
                        discard;
                    }
                }            
            }

            o.Emission = float3(1, 1, 1) * 0.0125;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
