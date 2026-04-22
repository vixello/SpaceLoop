Shader "UI/WaveGradient"
{
    Properties
    {
        _TopColor ("Top Color", Color) = (1,1,1,1)
        _BottomColor ("Bottom Color", Color) = (0,0,0,1)

        _WaveAmp ("Wave Amp", Range(0,0.5)) = 0.1
        _WaveSize ("Wave Size", Range(1,10)) = 3
        _WaveTimeMul ("Wave Time Mul", Range(0.1,2)) = 1
        _TotalPhases ("Total Phases", Range(2,600)) = 10
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
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
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            float4 _TopColor;
            float4 _BottomColor;
            float _WaveAmp;
            float _WaveSize;
            float _WaveTimeMul;
            float _TotalPhases;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float rand(float n)
            {
                return frac(sin(n) * 43758.5453123);
            }

            float noise(float p)
            {
                float fl = floor(p);
                float fc = frac(p);
                return lerp(rand(fl), rand(fl + 1.0), fc);
            }

            float fmod_custom(float x, float y)
            {
                return x - floor(x / y) * y;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float t = _TotalPhases;
                float effective_wave_amp = min(_WaveAmp, 0.5 / t);

                float d = fmod_custom(i.uv.y, 1.0 / t);
                float idx = floor(i.uv.y * t);
                float vi = floor(i.uv.y * t + t * effective_wave_amp);

                float s = effective_wave_amp *
                          sin((i.uv.x + _Time.y * max(1.0 / t, noise(vi)) *
                          _WaveTimeMul * vi / t) * 6.2831853 * _WaveSize);

                if (d < s) idx--;
                if (d > s + 1.0 / t) idx++;

                idx = clamp(idx, 0.0, t - 1.0);

                float w = idx / (t - 1.0);

                return lerp(_TopColor, _BottomColor, w);
            }
            ENDCG
        }
    }
}
