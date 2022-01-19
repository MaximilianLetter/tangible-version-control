Shader "Custom/Wireframe With Vertex Colors"
{
    Properties
    {
        _Color("Color", color) = (0.0, 1.0, 0.0, 1.0)
        _WireframeColor("Wireframe Color", color) = (1.0, 1.0, 1.0, 1.0)
        _Thickness("Thickness", Range(0.001, 0.8)) = 0.02

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
        Pass
        {
            Offset[_ZOffsetFactor],[_ZOffsetUnits]
            Blend[_SrcBlend][_DstBlend]
            BlendOp[_BlendOp]

            Stencil
            {
                Ref[_StencilReference]
                Comp[_StencilComparison]
                Pass[_StencilOperation]
            }

            CGPROGRAM
            #pragma vertex VSMain
            #pragma geometry GSMain
            #pragma fragment PSMain
            #pragma target 4.0

            float _Thickness;
            float4 _WireframeColor;
            float4 _Color;

            struct Data
            {
                float4 vertex : SV_Position;
                float2 barycentric : BARYCENTRIC;
                float4 color : COLOR;
            };

            //UNITY_INSTANCING_BUFFER_START(Props)
            //UNITY_DEFINE_INSTANCED_PROP(float4, _Color)

            void VSMain(inout float4 vertex:POSITION, inout float4 color : COLOR) { }

            [maxvertexcount(3)]
            void GSMain(triangle float4 patch[3]:SV_Position, triangle float4 color[3] : COLOR, inout TriangleStream<Data> stream)
            {
                Data GS;
                for (uint i = 0; i < 3; i++)
                {
                    GS.vertex = UnityObjectToClipPos(patch[i]);
                    GS.barycentric = float2(fmod(i,2.0), step(2.0,i));
                    GS.color = color[i];
                    stream.Append(GS);
                }
                stream.RestartStrip();
            }

            float4 PSMain(Data PS) : SV_Target
            {
                float3 coord = float3(PS.barycentric, 1.0 - PS.barycentric.x - PS.barycentric.y);
                coord = smoothstep(fwidth(coord) * 0.1, fwidth(coord) * 0.1 + fwidth(coord), coord);
                //return float4(lerp(_WireframeColor, PS.color, min(coord.x, min(coord.y, coord.z)).xxx), 1.0);
                return float4(lerp(_WireframeColor, _Color, min(coord.x, min(coord.y, coord.z)).xxx), _Color.a);
            }
            ENDCG
        }
    }
}
