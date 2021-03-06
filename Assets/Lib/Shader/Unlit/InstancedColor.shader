﻿// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

Shader "Custom/Unlit/InstancedColor"
{
    Properties
    {
        _Color ("Color", Color) = (0, 0, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        CGINCLUDE
        #include "UnityCG.cginc"

        struct appdata
        {
            float4 vertex : POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

            
        struct v2f
        {
            float4 vertex : SV_POSITION;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
#define _Color_arr Props
        UNITY_INSTANCING_BUFFER_END(Props)
            
        v2f vert (appdata v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_TRANSFER_INSTANCE_ID(v, o);
            o.vertex = UnityObjectToClipPos(v.vertex);
            return o;
        }
            
        fixed4 frag (v2f i) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(i);
            return UNITY_ACCESS_INSTANCED_PROP(_Color_arr, _Color);
        }
        ENDCG

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            ENDCG
        }
    }
}
