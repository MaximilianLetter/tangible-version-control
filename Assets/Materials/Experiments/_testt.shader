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

            // inverseW is to counteract the effect of perspective-correct interpolation so that the lines
            // look the same thickness regardless of their depth in the scene.
            struct g2f
            {
                float4 viewPos : SV_POSITION;
                float inverseW : TEXCOORD0;
                float3 dist : TEXCOORD1;
                float2 barycentric : BARYCENTRIC;
                float4 color : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            /*struct Data
            {
                float4 vertex : SV_Position;
                float2 barycentric : BARYCENTRIC;
                float4 color : COLOR;
            };*/

            //UNITY_INSTANCING_BUFFER_START(Props)
            //UNITY_DEFINE_INSTANCED_PROP(float4, _Color)

            //void vert(inout float4 vertex : POSITION, inout float4 color : COLOR) { }

            [maxvertexcount(3)]
            void geom(triangle v2g i[3], triangle float4 color[3] : COLOR, inout TriangleStream<g2f> stream)
            {

                // Calculate the vectors that define the triangle from the input points.
                float2 point0 = i[0].viewPos.xy / i[0].viewPos.w;
                float2 point1 = i[1].viewPos.xy / i[1].viewPos.w;
                float2 point2 = i[2].viewPos.xy / i[2].viewPos.w;

                // Calculate the area of the triangle.
                float2 vector0 = point2 - point1;
                float2 vector1 = point2 - point0;
                float2 vector2 = point1 - point0;
                float area = abs(vector1.x * vector2.y - vector1.y * vector2.x);

                float3 distScale[3];
                distScale[0] = float3(area / length(vector0), 0, 0);
                distScale[1] = float3(0, area / length(vector1), 0);
                distScale[2] = float3(0, 0, area / length(vector2));

                g2f o;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                    //Data GS;
                for (uint idx = 0; idx < 3; idx++)
                {
                    /*o.vertex = UnityObjectToClipPos(patch[idx]);
                    o.barycentric = float2(fmod(idx,2.0), step(2.0, idx));
                    o.color = color[idx];
                    UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i[idx], o);*/
                    o.viewPos = i[idx].viewPos;
                    o.inverseW = 1.0 / o.viewPos.w;
                    o.dist = distScale[idx] * o.viewPos.w * _WireThickness;
                    o.barycentric = float2(fmod(idx, 2.0), step(2.0, idx));
                    o.color = color[idx];
                    UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i[idx], o);
                    stream.Append(o);
                }
                stream.RestartStrip();
            }

            //[maxvertexcount(3)]
            //void geom(triangle v2g i[3], inout TriangleStream<g2f> triStream)
            //{
            //    // Calculate the vectors that define the triangle from the input points.
            //    float2 point0 = i[0].viewPos.xy / i[0].viewPos.w;
            //    float2 point1 = i[1].viewPos.xy / i[1].viewPos.w;
            //    float2 point2 = i[2].viewPos.xy / i[2].viewPos.w;

            //    // Calculate the area of the triangle.
            //    float2 vector0 = point2 - point1;
            //    float2 vector1 = point2 - point0;
            //    float2 vector2 = point1 - point0;
            //    float area = abs(vector1.x * vector2.y - vector1.y * vector2.x);

            //    float3 distScale[3];
            //    distScale[0] = float3(area / length(vector0), 0, 0);
            //    distScale[1] = float3(0, area / length(vector1), 0);
            //    distScale[2] = float3(0, 0, area / length(vector2));

            //    float wireScale = 800 - _WireThickness;

            //    // Output each original vertex with its distance to the opposing line defined
            //    // by the other two vertices.
            //    g2f o;
            //    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

            //    [unroll]
            //    for (uint idx = 0; idx < 3; ++idx)
            //    {
            //        o.viewPos = i[idx].viewPos;
            //        o.inverseW = 1.0 / o.viewPos.w;
            //        o.dist = distScale[idx] * o.viewPos.w * wireScale;
            //        UNITY_TRANSFER_VERTEX_OUTPUT_STEREO(i[idx], o);
            //        triStream.Append(o);
            //    }
            //}

            float4 frag(g2f i) : SV_Target
            {
                float3 coord = float3(i.barycentric, 1.0 - i.barycentric.x - i.barycentric.y);
                coord = smoothstep(fwidth(coord) * 0.1, fwidth(coord) * 0.1 + fwidth(coord), coord);
                //return float4(lerp(_WireframeColor, PS.color, min(coord.x, min(coord.y, coord.z)).xxx), 1.0);
                //return float4(lerp(_WireframeColor, _Color, min(coord.x, min(coord.y, coord.z)).xxx), _Color.a);
                return float4(lerp(_WireframeColor, _Color, min(coord.x, min(coord.y, coord.z)).xxxx));
            }
            ENDCG
        }
    }
}
