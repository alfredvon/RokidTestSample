Shader "Custom/QuadOutline"
{
    Properties
    {
        _Color ("Color", Color) = (0,0,0,1)  // ��ɫ
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)  // �����ɫ
        _OutlineWidth ("Outline Width", Range(0.001, 0.1)) = 0.02  // ��߿��
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Name "Outline"
            ZWrite On
            Cull Off  // ���޳��������
            Blend SrcAlpha OneMinusSrcAlpha  // ֧��͸���Ȼ��

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            uniform float _OutlineWidth;
            uniform fixed4 _OutlineColor;
            uniform fixed4 _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // ��Ե��⣺���� UV �������
                float distToEdge = min(i.uv.x, min(i.uv.y, min(1.0 - i.uv.x, 1.0 - i.uv.y)));
                
                if (distToEdge > _OutlineWidth)
                    return _Color;
                // // �������߷�Χ�ڣ���ʾ�����ɫ
                // if (distToEdge < _OutlineWidth)
                // {
                    return _OutlineColor;  // ��ʾ�����ɫ
                // }
               
            }
            ENDCG
        }
    }
    FallBack "Transparent"
}
