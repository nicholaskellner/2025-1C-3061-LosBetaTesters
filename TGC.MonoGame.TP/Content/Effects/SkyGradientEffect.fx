struct VS_IN
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
};

struct VS_OUT
{
    float4 Position : POSITION;
    float2 TexCoord : TEXCOORD0;
};

VS_OUT MainVS(VS_IN input)
{
    VS_OUT output;
    output.Position = input.Position;
    output.TexCoord = input.TexCoord;
    return output;
}

float4 MainPS(VS_OUT input) : COLOR
{
    float t = saturate(input.TexCoord.y);

    float3 topColor = float3(0.529f, 0.808f, 0.922f);
    float3 bottomColor = float3(1, 1, 1);

    float3 color = lerp(bottomColor, topColor, t);

    return float4(color, 1);
}

technique Technique1
{
    pass Pass1
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 MainPS();
    }
}

