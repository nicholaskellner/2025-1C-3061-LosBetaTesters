#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0
    #define PS_SHADERMODEL ps_4_0
#endif

uniform float2 Tiling;
uniform float4x4 World;
uniform float4x4 View;
uniform float4x4 Projection;

uniform float4x4 LightViewProjection;
uniform float3 ambientColor;
uniform float KAmbient;

uniform float3 lightDirection;
uniform float3 lightColor;
uniform float3 cameraPosition;

Texture2D Texture;
Texture2D Texture2;
Texture2D ShadowMap;

sampler2D TextureSampler = sampler_state {
    Texture = <Texture>;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler2D TextureSampler2 = sampler_state {
    Texture = <Texture2>;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler2D ShadowMapSampler = sampler_state {
    Texture = <ShadowMap>;
    AddressU = Clamp;
    AddressV = Clamp;
    MinFilter = Point;
    MagFilter = Point;
    MipFilter = None;
};

struct VertexShaderInput {
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput {
    float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float3 WorldPos : TEXCOORD2;
    float4 ShadowPos : TEXCOORD3;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    float4 worldPosition = mul(input.Position, World);
    output.WorldPos = worldPosition.xyz;
    output.Normal = normalize(mul(float4(input.Normal, 0), World).xyz);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);
    output.TextureCoordinate = input.TextureCoordinate * Tiling;

    // Posición en espacio de la luz (proyección para el shadow map)
    output.ShadowPos = mul(worldPosition, LightViewProjection);

    return output;
}

float SampleShadow(float4 shadowPos)
{
    // Convertimos a coordenadas de textura [0,1]
    float2 shadowTexCoord = shadowPos.xy / shadowPos.w * 0.5 + 0.5;

    // Profundidad del fragmento actual en la luz
    float currentDepth = shadowPos.z / shadowPos.w;

    // Leer la profundidad del shadow map
    float shadowDepth = tex2D(ShadowMapSampler, shadowTexCoord).r;

    // Factor de sombra con bias para evitar shadow acne
    float bias = 0.005;
    float shadow = currentDepth - bias > shadowDepth ? 0.5 : 1.0;

    return shadow;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float4 texCol = tex2D(TextureSampler, input.TextureCoordinate);
    float4 texCol2 = tex2D(TextureSampler2, input.TextureCoordinate);

    float heightFactor = saturate(input.WorldPos.y / 10);
    float4 baseColor = lerp(texCol2, texCol, heightFactor);

    float3 ambient = ambientColor * KAmbient;
    float3 N = normalize(input.Normal);
    float3 L = normalize(-lightDirection);
    float3 V = normalize(cameraPosition - input.WorldPos);
    float3 H = normalize(L + V);

    float NdotL = saturate(dot(N, L));
    float3 diffuse = lightColor * NdotL;

    float shininess = 32.0f;
    float NdotH = saturate(dot(N, H));
    float3 specular = lightColor * pow(NdotH, shininess);

    // Aplicar sombra
    float shadowFactor = SampleShadow(input.ShadowPos);
    float3 finalLighting = ambient + shadowFactor * (diffuse + specular);
    float3 finalColor = baseColor.rgb * finalLighting;

    return float4(finalColor, baseColor.a);
}

technique BasicColorDrawing {
    pass P0 {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};


