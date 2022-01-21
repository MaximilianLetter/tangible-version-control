Shader "Custom/Wireframe With Vertex Colors"
{
    Properties
    {
        _Color("Color", color) = (0.0, 1.0, 0.0, 1.0)
        _WireframeColor("Wireframe Color", color) = (1.0, 1.0, 1.0, 1.0)
        _WireThickness("Wire Thickness", Range(0.001, 0.8)) = 0.02

        _ZOffsetFactor("Depth Offset Factor", Float) = 0                                             // "Zero"
        _ZOffsetUnits("Depth Offset Units", Float) = 0                                               // "Zero"
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Source Blend", Float) = 1                 // "One"
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Destination Blend", Float) = 0            // "Zero"
        [Enum(UnityEngine.Rendering.BlendOp)] _BlendOp("Blend Operation", Float) = 0                 // "Add"

        // Added options.
        [Toggle(_STENCIL)] _Stencil("Enable Stencil Testing", Float) = 0.0
        _StencilReference("Stencil Reference", Range(0, 255)) = 0
        [Enum(UnityEngine.Rendering.CompareFunction)]_StencilComparison("Stencil Comparison", Int) = 0
        [Enum(UnityEngine.Rendering.StencilOp)]_StencilOperation("Stencil Operation", Int) = 0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Offset[_ZOffsetFactor],[_ZOffsetUnits]
        Blend[_SrcBlend][_DstBlend]
        BlendOp[_BlendOp]

        Pass
        {
            Stencil
            {
                Ref[_StencilReference]
                Comp[_StencilComparison]
                Pass[_StencilOperation]
            }

            CGPROGRAM
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag

            #if defined(SHADER_API_D3D11)
            #pragma target 5.0
            #endif

            #include "UnityCG.cginc"

            float4 _Color;
            float4 _WireframeColor;
            float _WireThickness;

            struct v2g
            {
                float4 viewPos : SV_POSITION;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2g vert(appdata_base v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                v2g o;
                o.viewPos = UnityObjectToClipPos(v.vertex);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                return o;
            }

            struct g2f
            {
                float4 viewPos : SV_POSITION;
                float2 barycentric : BARYCENTRIC;
                float4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            [maxvertexcount(3)]
            void geom(triangle v2g i[3], triangle float4 color[3] : COLOR, inout TriangleStream<g2f> stream)
            {
                g2f o;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                for (uint idx = 0; idx < 3; idx++)
                {
                    o.viewPos = i[idx].viewPos;
                    o.barycentric = float2(fmod(idx, 2.0), step(2.0, idx));
                    o.color = color[idx];
                    UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i[idx], o);
                    stream.Append(o);
                }
                stream.RestartStrip();
            }

            float4 frag(g2f i) : SV_Target
            {
                float3 coord = float3(i.barycentric, 1.0 - i.barycentric.x - i.barycentric.y);
                coord = smoothstep(fwidth(coord) * 0.1, fwidth(coord) * 0.1 + fwidth(coord), coord);
                return float4(lerp(_WireframeColor, _Color, min(coord.x, min(coord.y, coord.z)).xxxx));
            }
            ENDCG
        }
    }
}
