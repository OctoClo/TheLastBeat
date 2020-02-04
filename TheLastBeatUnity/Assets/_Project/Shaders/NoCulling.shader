Shader "Custom/No Culling" {
    Properties {
        _Color ("Color", Color) = (1,1,1,1)
        [NoScaleOffset] _MainTex ("Albedo (RGB)", 2D) = "white" {}
        [Toggle] _UseMetallicMap ("Use Metallic Map", Float) = 0.0
        [NoScaleOffset] _MetallicGlossMap("Metallic", 2D) = "black" {}
        [Gamma] _Metallic ("Metallic", Range(0,1)) = 0.0
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _BumpScale("Scale", Float) = 1.0
        [NoScaleOffset] _BumpMap("Normal Map", 2D) = "bump" {}
    }
    SubShader {
        Tags { "Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="Fade" }
        LOD 200
        ZWrite On
        Cull Off

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade nolightmap addshadow
        #pragma shader_feature _USEMETALLICMAP_ON
        #pragma target 3.0
        sampler2D _MainTex;
        sampler2D _MetallicGlossMap;
        sampler2D _BumpMap;
        struct Input {
            float2 uv_MainTex;
            fixed facing : VFACE;
        };
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        half _BumpScale;
        
        void surf (Input IN, inout SurfaceOutputStandard o) {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
 
            #ifdef _USEMETALLICMAP_ON
            fixed4 mg = tex2D(_MetallicGlossMap, IN.uv_MainTex);
            o.Metallic = mg.r;
            o.Smoothness = mg.a;
            #else
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            #endif
 
            o.Normal = UnpackScaleNormal(tex2D(_BumpMap, IN.uv_MainTex), _BumpScale);
            o.Normal.z *= IN.facing; // flip Z based on facing
        }
        ENDCG
    }
    FallBack "Diffuse"
}