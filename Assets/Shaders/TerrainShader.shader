Shader "Custom/TerrainShader"
{
    Properties
    {
        _Alphamap0("Alphamap0", 2D) = "white" {}
        _Alphamap1("Alphamap1", 2D) = "white" {}
        _diffuse0("_diffuse0", 2D) = "white" {}
        _diffuse1("_diffuse1", 2D) = "white" {}
        _diffuse2("_diffuse2", 2D) = "white" {}
        _diffuse3("_diffuse3", 2D) = "white" {}
        _diffuse4("_diffuse4", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0
        sampler2D _Alphamap0;
        sampler2D _Alphamap1;

        struct Input
        {
            float2 uv_Alphamap0;
        };

        sampler2D _diffuse0;
        sampler2D _diffuse1;
        sampler2D _diffuse2;
        sampler2D _diffuse3;
        sampler2D _diffuse4;
        half _Glossiness;
        half _Metallic;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = fixed4(0, 0, 0, 0);
            c += tex2D(_diffuse0, float2(0, 0)) * tex2D(_Alphamap0, IN.uv_Alphamap0).r;
            c += tex2D(_diffuse1, float2(0, 0)) * tex2D(_Alphamap0, IN.uv_Alphamap0).g;
            c += tex2D(_diffuse2, float2(0, 0)) * tex2D(_Alphamap0, IN.uv_Alphamap0).b;
            c += tex2D(_diffuse3, float2(0, 0)) * tex2D(_Alphamap0, IN.uv_Alphamap0).a;
            c += tex2D(_diffuse4, float2(0, 0)) * tex2D(_Alphamap1, IN.uv_Alphamap0).r;
            c /= c.a;

            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = 1.0;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
