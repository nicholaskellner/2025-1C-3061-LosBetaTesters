#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define SV_POSITION SV_Position
    #define VS_SHADERMODEL vs_4_0_level_9_1
    #define PS_SHADERMODEL ps_4_0_level_9_1
#endif

// Matrices estándar
uniform float4x4 World;
uniform float4x4 View;
uniform float4x4 Projection;
uniform float3x3 WorldInverseTranspose;

// Iluminación
uniform float3 lightPosition;
uniform float3 cameraPosition;
uniform float3 ambientColor;
uniform float KAmbient;
uniform float3 diffuseColor;
uniform float3 specularColor;
uniform float shininess;

// Textura base
Texture2D Texture;

sampler2D TextureSampler = sampler_state
{
    Texture = <Texture>;
    MipFilter = LINEAR;
    MinFilter = LINEAR;
    MagFilter = LINEAR;
};

// Shadow mapping
uniform float4x4 LightViewProjection;
Texture2D ShadowMap;

sampler2D ShadowMapSampler = sampler_state
{
    Texture = <ShadowMap>;
    MinFilter = POINT;
    MagFilter = POINT;
    MipFilter = NONE;
    AddressU = CLAMP;
    AddressV = CLAMP;
};

// Entrada del VS
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

// Salida del VS
struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 Normal   : TEXCOORD0;
    float2 TexCoord : TEXCOORD1;
    float3 WorldPos : TEXCOORD2;
    float4 ShadowCoord : TEXCOORD3;
};

// Vertex shader
VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPos = mul(input.Position, World);
    output.Position = mul(mul(worldPos, View), Projection);
    output.WorldPos = worldPos.xyz;
    output.Normal = normalize(mul(input.Normal, WorldInverseTranspose));
    output.TexCoord = input.TexCoord;

    output.ShadowCoord = mul(worldPos, LightViewProjection);
    return output;
}

// Pixel shader con sombras
float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 normal = normalize(input.Normal);
    float3 lightDir = normalize(lightPosition - input.WorldPos);
    float3 viewDir = normalize(cameraPosition - input.WorldPos);
    float3 halfVec = normalize(lightDir + viewDir);

    float distance = length(lightPosition - input.WorldPos);
    float attenuation = 1.0 / (1.0 + 0.05 * distance + 0.01 * distance * distance);

    float diff = max(dot(normal, lightDir), 0.0);
    float spec = pow(max(dot(normal, halfVec), 0.0), shininess);

    float4 texColor = tex2D(TextureSampler, input.TexCoord);

    float3 ambient = ambientColor * KAmbient * texColor.rgb;
    float3 diffuse = diffuseColor * diff * texColor.rgb;
    float3 specular = specularColor * spec * attenuation;

    // Shadow mapping
    float2 shadowTexCoords = input.ShadowCoord.xy / input.ShadowCoord.w * 0.5 + 0.5;
    float shadowDepth = tex2D(ShadowMapSampler, shadowTexCoords).r;
    float currentDepth = input.ShadowCoord.z / input.ShadowCoord.w;

    float bias = 0.005;
    float shadow = currentDepth - bias > shadowDepth ? 0.4 : 1.0;

    float3 finalColor = ambient + shadow * (diffuse + specular);
    return float4(finalColor, texColor.a);
}

// Técnica
technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader  = compile PS_SHADERMODEL MainPS();
    }
};





