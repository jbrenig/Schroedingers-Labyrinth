Shader "UnityLibrary/2D/Effects/ColorCycle"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _GradientTex ("Gradient", 2D) = "white" {}
        _MaskTex ("Mask", 2D) = "white" {}
        _Speed("Speed",float) = 1
        
 
        // ui renderer stuff
        _StencilComp("Stencil Comparison", Float) = 8
        _Stencil("Stencil ID", Float) = 0
        _StencilOp("Stencil Operation", Float) = 0
        _StencilWriteMask("Stencil Write Mask", Float) = 255
        _StencilReadMask("Stencil Read Mask", Float) = 255
 
        _ColorMask("Color Mask", Float) = 15
 
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip", Float) = 0
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
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest[unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask[_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            sampler2D _GradientTex;
            sampler2D _MaskTex;
            float4 _MainTex_ST;
            float _Speed;

            v2f vert(appdata_t IN)
            {
                v2f OUT;

                UNITY_SETUP_INSTANCE_ID (IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 mainColor = tex2D(_MainTex, i.texcoord);
                // mask color (only alpha channel is used)
                float mask = tex2D(_MaskTex, i.texcoord).a;

                // get scrolling
                float scroll = frac((_Time.x) * _Speed);

                // get gradient color from texture
                fixed4 origCol = (1-mask) * mainColor;
                fixed4 col = tex2D(_GradientTex, float2(scroll + i.texcoord.x, 0.5)) * mainColor * mask + origCol;
                col.rgb *= col.a;
                return col;
            }
            ENDCG
        }
    }
}