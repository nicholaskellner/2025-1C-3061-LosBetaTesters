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
uniform float4x4 WorldInverseTranspose;

uniform float4 color; // color base del objeto
uniform float3 ambientColor;
uniform float KAmbient;

uniform float3 lightPosition;
uniform float3 lightColor;
uniform float KDiffuse;
uniform float KSpecular;
uniform float shininess;

uniform float3 cameraPosition;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float3 Normal   : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float3 WorldPos : TEXCOORD0;
    float3 Normal   : TEXCOORD1;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    output.Position = mul(mul(worldPosition, View), Projection);
    output.WorldPos = worldPosition.xyz;

    output.Normal = normalize(mul(input.Normal, (float3x3)WorldInverseTranspose));

    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
    float3 normal = normalize(input.Normal);
    float3 toLight = normalize(lightPosition - input.WorldPos);
    float3 toView = normalize(cameraPosition - input.WorldPos);
    float3 halfwayDir = normalize(toLight + toView);

    float3 ambient = ambientColor * KAmbient;

    float diff = max(dot(normal, toLight), 0.0);
    float3 diffuse = lightColor * KDiffuse * diff;

    float spec = 0.0;
    if (diff > 0.0)
    {
        spec = pow(max(dot(normal, halfwayDir), 0.0), shininess);
    }
    float3 specular = lightColor * KSpecular * spec;

    float3 finalColor = (ambient + diffuse + specular) * color.rgb;
    return float4(saturate(finalColor), color.a);
}

technique BasicColorDrawing
{
    pass P0
    {
        VertexShader = compile VS_SHADERMODEL MainVS();
        PixelShader = compile PS_SHADERMODEL MainPS();
    }
};


