Shader "Custom/StencilDifferences"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _NeutralColor("Neutral color", Color) = (.75, .75, .75, 1)
        _ModifiedColor("Modified color", Color) = (.0, 1.0, .0, 1)
    }
    
    SubShader
    {
        //ZWrite On
        Tags { "RenderType" = "Opaque" }
        LOD 100

        // Difference detected: in this area the objects do not overlap, should be rendered as outline
        Pass // #1
        {
            Stencil {
                Ref 0
                Comp Equal
                Pass IncrSat
                Fail IncrSat
            }
            
            ZTest Less
            ZWrite On
            Cull Back

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            fixed4 _ModifiedColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = _ModifiedColor;
                return col;
            }
            ENDCG
        }

        // BASE CASE: in this area the object overlaps, it should not be rendered at all, or as phantom object, writing depth but nothing else (or solid black)
        Pass // #2
        {
            Stencil {
                Ref 1
                Comp Less
            }

            ZTest Equal
            ZWrite On
            Cull Back

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            fixed4 _NeutralColor;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = _NeutralColor;
                return col;
            }
            ENDCG
        }

        /*Pass
        {
            Stencil {
                Ref 2
                Comp Less
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = fixed4(1.0, 0.0, 0.0, 1.0);
                return col;
            }
            ENDCG
        }*/
    }
}