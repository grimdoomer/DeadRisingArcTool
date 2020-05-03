
#include "XfGlobal.fx"



struct VOut
{
	float4 position : SV_POSITION;
	float4 color : COLOR;
};

VOut VShader(float3 position : POSITION, float4 color : COLOR)
{
	VOut output;

	output.position = mul(float4(position, 1), gXfViewProj);
	output.color = color;

	return output;
}


float4 PShader(float4 position : SV_POSITION, float4 color : COLOR) : SV_TARGET
{
	return color;
}