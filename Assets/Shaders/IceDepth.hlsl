void IceDepth_half(
    in UnityTexture2D MainTex,
    in float2 UV,
    in UnitySamplerState SS,
    in float Samples,
    in float Offset,
    in float3 WPOS,
    in float3 NormalWS,
    in float Lerp,
    in int LOD,
    out float4 Out)
{
    float3 viewDir = normalize(_WorldSpaceCameraPos - WPOS);

    // Project view direction onto surface
    float3 tangentView = normalize(viewDir - NormalWS * dot(viewDir, NormalWS));

    //  convert to 2D direction (approximation!)
    float3 up = float3(0, 1, 0);

    float3 tangent = normalize(cross(up, NormalWS));
    float3 bitangent = cross(NormalWS, tangent);

    float2 uvDir = float2(
    dot(tangentView, tangent),
    dot(tangentView, bitangent)
);
    float4 col = float4(0, 0, 0, 0);

    for (int s = 0; s < Samples; s++)
    {
        float t = (float) s / Samples;

        float2 offsetUV = UV + uvDir * t * Offset;

        col += SAMPLE_TEXTURE2D_LOD(MainTex, SS, offsetUV, LOD);
    }

    float4 render = col / Samples;

    Out = lerp(SAMPLE_TEXTURE2D(MainTex, SS, UV), render, Lerp);
}