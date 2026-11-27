Shader "Custom/URP/BloodFullScreen_Anime"
{
    Properties
    {
        _BloodColor ("Blood Color", Color) = (0.30, 0.002, 0.008, 1)
        _BottomStrength ("Bottom Blood Strength", Range(0, 2)) = 1.25
        _BottomHeight ("Bottom Blood Height", Range(0.05, 0.65)) = 0.30
        _EdgeStrength ("Side and Top Edge Strength", Range(0, 1)) = 0.28
        _SpotStrength ("Random Blood Spots Strength", Range(0, 2)) = 0.85
        _SpotScale ("Blood Spot Scale", Range(3, 30)) = 13
        _OverallStrength ("Overall Strength", Range(0, 2)) = 1
        _Health ("Health", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }
        ZWrite Off ZTest Always Cull Off

        Pass
        {
            Name "BloodFullscreen"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BloodColor;
                float _BottomStrength;
                float _BottomHeight;
                float _EdgeStrength;
                float _SpotStrength;
                float _SpotScale;
                float _OverallStrength;
                float _Health;
            CBUFFER_END

            // These two values are sent globally by HealthScreenEffect.cs.
            float _HitFlash;

            float Hash21(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            float2 Hash22(float2 p)
            {
                float n = Hash21(p);
                return float2(n, Hash21(p + n + 17.17));
            }

            float Smooth01(float a, float b, float x)
            {
                float t = saturate((x - a) / max(0.0001, b - a));
                return t * t * (3.0 - 2.0 * t);
            }

            // Procedural irregular blood droplets, generated from a Voronoi-like cell pattern.
            float BloodSpots(float2 uv, float scale)
            {
                float2 p = uv * scale;
                float2 cell = floor(p);
                float2 local = frac(p);
                float nearest = 10.0;
                float second = 10.0;
                float chosen = 0.0;

                [unroll]
                for (int y = -1; y <= 1; y++)
                {
                    [unroll]
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 offset = float2((float)x, (float)y);
                        float2 id = cell + offset;
                        float2 randomPoint = Hash22(id);
                        randomPoint = 0.18 + randomPoint * 0.64;
                        float2 delta = offset + randomPoint - local;
                        float d = length(delta);

                        if (d < nearest)
                        {
                            second = nearest;
                            nearest = d;
                            chosen = Hash21(id + 9.7);
                        }
                        else if (d < second)
                        {
                            second = d;
                        }
                    }
                }

                // Different cell sizes and thresholds create uneven splashes.
                float radius = lerp(0.10, 0.30, chosen);
                float spot = 1.0 - Smooth01(radius * 0.55, radius, nearest);
                float rim = 1.0 - Smooth01(0.025, 0.085, second - nearest);
                return saturate(spot * (0.72 + rim * 0.28));
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float2 uv = input.texcoord;
                half4 scene = SAMPLE_TEXTURE2D_X(_BlitTexture, sampler_LinearClamp, uv);

                float health = saturate(_Health);
                float damage = 1.0 - health;
                float hit = saturate(_HitFlash);

                // At full health, persistent blood is absent. It grows as HP falls.
                float lowHealth = Smooth01(0.02, 0.95, damage);

                // Main edge vignette, kept subtle on top and sides.
                float edgeDistance = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));
                float edgeMask = 1.0 - Smooth01(0.015, lerp(0.10, 0.22, lowHealth), edgeDistance);
                float sideTopBlood = edgeMask * _EdgeStrength * lowHealth;

                // Dense red pool along the bottom, with a soft uneven upper boundary.
                float xWave = sin(uv.x * 19.0 + sin(uv.x * 43.0) * 0.8) * 0.018;
                float bottomY = _BottomHeight * lerp(0.45, 1.0, lowHealth);
                float bottomDistance = uv.y - (bottomY + xWave);
                float bottomMask = 1.0 - Smooth01(-0.035, 0.10, bottomDistance);
                float bottomCore = 1.0 - Smooth01(0.0, bottomY * 0.72 + 0.001, uv.y);
                float bottomBlood = saturate(max(bottomMask * 0.72, bottomCore)) * _BottomStrength * lowHealth;

                // A few thin downward/upward streak-like extensions around the lower region.
                float streakSeed = Hash21(float2(floor(uv.x * 31.0), 4.0));
                float streakWidth = lerp(0.003, 0.012, streakSeed);
                float streakCenter = frac(uv.x * 31.0 + streakSeed * 0.37);
                float streakLine = 1.0 - Smooth01(streakWidth, streakWidth + 0.012, abs(streakCenter - 0.5));
                float streakLength = lerp(0.05, 0.22, Hash21(float2(floor(uv.x * 31.0), 8.0)));
                float streak = streakLine * (1.0 - Smooth01(streakLength * 0.7, streakLength, uv.y));
                bottomBlood = saturate(bottomBlood + streak * lowHealth * 0.20);

                // Random scattered spots. UV distortion makes them less grid-like.
                float2 spotUV = uv + float2(sin(uv.y * 17.0) * 0.008, cos(uv.x * 13.0) * 0.006);
                float spotsLarge = BloodSpots(spotUV, _SpotScale);
                float spotsSmall = BloodSpots(spotUV + 0.31, _SpotScale * 1.85);
                float spotMask = saturate(spotsLarge * 0.75 + spotsSmall * 0.28);
                float spots = spotMask * _SpotStrength * lowHealth;

                // Hit flash temporarily boosts the whole blood layer.
                float hitAmount = hit * 0.68;
                float bloodAmount = saturate((sideTopBlood + bottomBlood + spots) * _OverallStrength + hitAmount);

                // Make the bottom and random marks darker/thicker than the soft vignette.
                float darkBlood = saturate((bottomBlood * 0.72 + spots * 0.82 + sideTopBlood * 0.28) * _OverallStrength + hitAmount);
                float3 bloodColor = _BloodColor.rgb;

                float3 darkenedScene = scene.rgb * (1.0 - darkBlood * 0.78);
                float3 tintedScene = lerp(darkenedScene, bloodColor, saturate(darkBlood * 0.82));
                float3 result = lerp(scene.rgb, tintedScene, saturate(bloodAmount));

                return half4(saturate(result), scene.a);
            }
            ENDHLSL
        }
    }
}
