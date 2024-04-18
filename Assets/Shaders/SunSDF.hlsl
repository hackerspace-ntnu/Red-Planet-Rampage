#include "Assets/Shaders/StarSdf.hlsl"
float ndot_float(float2 a, float2 b) { return a.x * b.x - a.y * b.y; }
float sdRhombus_float(float2 p, float2 b)
{
	p = abs(p);

	float h = clamp(ndot_float(b - 2.0 * p, b) / dot(b, b), -1.0, 1.0);
	float d = length(p - 0.5 * b * float2(1.0 - h, 1.0 + h));

	return d * sign(p.x * b.y + p.y * b.x - b.x * b.y);
}

float repeatedRhombus_float(float2 position, float2 rhombusSize)
{
	const int   n = 8;
	const float b = 6.283185 / float(n);
	float a = atan2(position.y, position.x);
	float i = floor(a / b);

	float c1 = b * (i + 0.0); 
	float2 p1 = mul(float2x2(cos(c1), -sin(c1), sin(c1), cos(c1)), position);
	float c2 = b * (i + 1.0); 
	float2 p2 = mul(float2x2(cos(c2), -sin(c2), sin(c2), cos(c2)), position);

	return min(sdRhombus_float(p1 / (sin(c1 + iTime / 7.)), rhombusSize), sdRhombus_float(p2 / (sin(c2 + iTime / 7.)), rhombusSize));
}

float2 wobble_float(float2 position)
{
	const float frequency = 80;
	const float amount = 0.012;
	float offset = iTime/10.;
	offset = fmod(offset, 6.283185 / frequency);
	position += offset;
	float2 wobble = sin(position.y * frequency) * amount;
	position = position + wobble;
	position -= offset;
	return position;
}

float solarSdf_float(float2 position, inout float4 sunColor)
{
	float d = length(wobble_float(position)) - 0.27;
	sunColor = (d > 0.0) ? sunColor : float4(1., 0.8, 0., 1.);
	float dShine = min(length(position) - 0.2, repeatedRhombus_float(position, float2(0.6f, 0.05f)));
	sunColor = (dShine > 0.0) ? sunColor : float4(1., 1., 1., 1.);

	return min(d, dShine);
}

void SunSDF_float(float2 UV, float2 SunPosition, float4 Color, float Time, out float Distance, out float4 FragmentColor)
{
	iTime = Time;

	Distance = solarSdf_float(UV + SunPosition, Color);
	FragmentColor = Color;
}

