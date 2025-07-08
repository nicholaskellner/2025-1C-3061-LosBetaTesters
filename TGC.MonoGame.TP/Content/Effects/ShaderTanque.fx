#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define SV_POSITION SV_Position
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

uniform float4x4 World;
uniform float4x4 View;
uniform float4x4 Projection;

uniform float3 lightPosition;
uniform float3 cameraPosition;

uniform float3 ambientColor;
uniform float KAmbient;
uniform float3 diffuseColor;
uniform float3 specularColor;
uniform float shininess;

Texture2D Texture;

sampler2D TextureSampler = sampler_state
{
    Texture = <Texture>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Normal : TEXCOORD0;
    float2 TexCoord : TEXCOORD1;
    float3 WorldPos : TEXCOORD2;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPos = mul(input.Position, World);
    output.Position = mul(mul(worldPos, View), Projection);

    output.Normal = normalize(mul(float4(input.Normal, 0.0), World).xyz);
    output.WorldPos = worldPos.xyz;
    output.TexCoord = input.TexCoord;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 normal = normalize(input.Normal);
    float3 lightDir = normalize(lightPosition - input.WorldPos);
    float3 viewDir = normalize(cameraPosition - input.WorldPos);
    float3 halfVec = normalize(lightDir + viewDir);

    float diff = max(dot(normal, lightDir), 0);
    float spec = pow(max(dot(normal, halfVec), 0), shininess);

    float3 lighting =
        ambientColor * KAmbient +
        diffuseColor * diff +
        specularColor * spec;

    float4 texColor = tex2D(TextureSampler, input.TexCoord);
    return float4(texColor.rgb * lighting, texColor.a);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};


