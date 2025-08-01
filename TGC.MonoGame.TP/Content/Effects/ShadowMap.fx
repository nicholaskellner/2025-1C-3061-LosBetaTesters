#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define SV_POSITION SV_Position
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

uniform float4x4 LightViewProjection; // View * Projection desde la luz
uniform float4x4 World;

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    float4 worldPos = mul(input.Position, World);
    output.Position = mul(worldPos, LightViewProjection);
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Simplemente usamos la profundidad generada por el pipeline
    return float4(1.0, 1.0, 1.0, 1.0); // El valor no importa mucho si usás DepthFormat
}

technique ShadowPass
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader  = compile PS_SHADERMODEL MainPS();
    }
};
