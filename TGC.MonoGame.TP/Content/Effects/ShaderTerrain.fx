#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0
	#define PS_SHADERMODEL ps_4_0
#endif

// Custom Effects - https://docs.monogame.net/articles/content/custom_effects.html
// High-level shader language (HLSL) - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl
// Programming guide for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-pguide
// Reference for HLSL - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-reference
// HLSL Semantics - https://docs.microsoft.com/en-us/windows/win32/direct3dhlsl/dx-graphics-hlsl-semantics

uniform float4x4 World;
uniform float4x4 View;
uniform float4x4 Projection;

uniform float3 ambientColor;
uniform float KAmbient; 

Texture2D Texture;
Texture2D Texture2;

sampler2D TextureSampler = sampler_state
{
    Texture = <Texture>;
	AddressU = Wrap;   // or Clamp, Mirror
    AddressV = Wrap; 
};

sampler2D TextureSampler2 = sampler_state
{
    Texture = <Texture2>;
	AddressU = Wrap;   // or Clamp, Mirror
    AddressV = Wrap; 
};

struct VertexShaderInput
{
	float4 Position : POSITION0;
	float2 TextureCoordinate : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_Position;
	float2 TextureCoordinate : TEXCOORD0;
	float height : TEXCOORD1;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    // Clear the output
	VertexShaderOutput output = (VertexShaderOutput)0;
    // Model space to World space
    float4 worldPosition = mul(input.Position, World);
    // World space to View space
    float4 viewPosition = mul(worldPosition, View);
	// View space to Projection space
    output.Position = mul(viewPosition, Projection);
	output.TextureCoordinate = worldPosition.xz/2;
	output.height = worldPosition.y/10;
    return output;
}

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 texCol = tex2D(TextureSampler, input.TextureCoordinate);
	float4 texCol2 = tex2D(TextureSampler2, input.TextureCoordinate);
	float4 color = float4(saturate(ambientColor * KAmbient) * texCol.rgb,texCol.a);
	float4 color2 = float4(saturate(ambientColor * KAmbient) * texCol2.rgb,texCol2.a);
	float h = saturate(input.height);
	float4 finalColor = lerp(color2, color, h);
	return finalColor;
}

technique BasicColorDrawing
{
	pass P0
	{
		VertexShader = compile VS_SHADERMODEL MainVS();
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};

