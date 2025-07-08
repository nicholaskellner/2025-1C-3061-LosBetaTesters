#if OPENGL
    #define SV_POSITION POSITION
    #define VS_SHADERMODEL vs_3_0
    #define PS_SHADERMODEL ps_3_0
#else
    #define VS_SHADERMODEL vs_4_0
    #define PS_SHADERMODEL ps_4_0
#endif

uniform float4x4 World;
uniform float4x4 View;
uniform float4x4 Projection;

uniform float3 ambientColor;
uniform float KAmbient;

uniform float3 lightDirection; // dirección de luz normalizada, en espacio mundo
uniform float3 lightColor;

uniform float3 cameraPosition;

Texture2D Texture;
Texture2D Texture2;

sampler2D TextureSampler = sampler_state
{
    Texture = <Texture>;
    AddressU = Wrap;
    AddressV = Wrap;
};

sampler2D TextureSampler2 = sampler_state
{
    Texture = <Texture2>;
    AddressU = Wrap;
    AddressV = Wrap;
};

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float2 TextureCoordinate : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float3 WorldPos : TEXCOORD2;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;

    // Transformar posición y normal a espacio mundo
    float4 worldPosition = mul(input.Position, World);
    output.WorldPos = worldPosition.xyz;

    // Normalizar normal transformada
    output.Normal = normalize(mul(float4(input.Normal, 0), World).xyz);

    // Transformar posición a espacio clip
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.TextureCoordinate = input.TextureCoordinate;

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    // Samplea las dos texturas
    float4 texCol = tex2D(TextureSampler, input.TextureCoordinate);
    float4 texCol2 = tex2D(TextureSampler2, input.TextureCoordinate);

    // Mezcla las dos texturas (puedes modificar cómo mezclarlas)
    float heightFactor = saturate(input.WorldPos.y / 10);
    float4 baseColor = lerp(texCol2, texCol, heightFactor);

    // Ambient
    float3 ambient = ambientColor * KAmbient;

    // Normal de pixel normalizada
    float3 N = normalize(input.Normal);

    // Dirección de luz normalizada (ya deberías pasarla normalizada)
    float3 L = normalize(-lightDirection); // luz hacia el objeto

    // Dirección a cámara
    float3 V = normalize(cameraPosition - input.WorldPos);

    // Vector half (para Blinn-Phong)
    float3 H = normalize(L + V);

    // Componente difusa (Lambert)
    float NdotL = saturate(dot(N, L));
    float3 diffuse = lightColor * NdotL;

    // Componente especular (Blinn-Phong)
    float shininess = 32.0f;
    float NdotH = saturate(dot(N, H));
    float3 specular = lightColor * pow(NdotH, shininess);

    // Combinamos la iluminación
    float3 finalLighting = ambient + diffuse + specular;

    // Aplica la iluminación al color base
    float3 finalColor = baseColor.rgb * finalLighting;

    return float4(finalColor, baseColor.a);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};

