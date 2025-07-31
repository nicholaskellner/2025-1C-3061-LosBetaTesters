#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float4x4 World;
float4x4 View;
float4x4 Projection;

TextureCube SkyBoxTexture;
samplerCUBE SkyBoxSampler = sampler_state
{
    texture = <SkyBoxTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = Clamp;
    AddressV = Clamp;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 TexCoord : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    // Multiplicar solo por World (que es solo escala)
    float4 worldPosition = mul(input.Position, World);

    // En el código C# la matriz View debe tener la traslación eliminada:
    // Ejemplo: ViewWithoutTranslation = View; ViewWithoutTranslation._41 = 0; _42=0; _43=0;

    // Multiplicar por View sin traslación
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    // Las coords para el cubemap son la posición en espacio cámara sin traslación (dirección)
    output.TexCoord = worldPosition.xyz;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    // Muestrea el cubemap con la dirección normalizada
    float3 dir = normalize(input.TexCoord);
    float4 color = texCUBE(SkyBoxSampler, dir);
    return color;
}

technique Skybox
{
    pass Pass1
    {
        VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
        PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
    }
}


