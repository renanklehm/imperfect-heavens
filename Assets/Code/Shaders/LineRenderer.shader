
Shader "Custom/LineRenderer"
{
    Properties
    {
        _Thickness ("Thickness", Range (0, 1000)) = 1
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Cull Off
            CGPROGRAM
            #pragma vertex vert
            #pragma exclude_renderers gles xbox360 ps3
            #pragma fragment frag
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : POSITION;
                UNITY_FOG_COORDS(1)
            };

            float _Thickness;
            float4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                // Transform the vertex position to clip space
                o.pos = UnityObjectToClipPos(v.vertex);

                // Calculate the screen width for the line based on camera distance
                float width = (1.0 / o.pos.w) * _Thickness * 0.5;

                // Calculate the world space view direction
                float3 viewDir = UnityObjectToWorldNormal(v.normal);

                // Adjust the vertex position based on the view direction and screen width
                o.pos.xyz += viewDir * width;

                // Output the position for the fragment shader
                UNITY_TRANSFER_FOG(o, o.pos);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = _Color;
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}