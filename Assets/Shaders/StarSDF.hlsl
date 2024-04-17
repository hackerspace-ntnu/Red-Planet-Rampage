//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

float iTime;

float2 hash_float(float2 p)
{
	//p = mod(p, 4.0); // tile
	p = float2(dot(p, float2(127.1, 311.7)),
		dot(p, float2(269.5, 183.3)));
	return frac(sin(p) * 18.5453);
}

// return distance, and cell id
float2 voronoi_float(float2 x)
{
	float2 n = floor(x);
	float2 f = frac(x);

	float3 m = float3(8.0, 0.0, 0.0);
	for (int j = -1; j <= 1; j++)
		for (int i = -1; i <= 1; i++)
		{
			float2  g = float2(float(i), float(j));
			float2  o = hash_float(n + g);
			float2  r = g - f + o;
			float d = dot(r, r);
			if (d < m.x)
				m = float3(d, o);
		}

	return float2(sqrt(m.x), m.y + m.z);
}

float sdStar5_float(float2 p, float2 id, in float r, in float rf)
{
	r += 0.3 - sin(iTime + sin(id.x) - cos(id.x) * 0.3) * 0.2;
	const float2 k1 = float2(0.809016994375, -0.587785252292);
	const float2 k2 = float2(-k1.x, k1.y);
	p.x = abs(p.x);
	p -= 2.0 * max(dot(k1, p), 0.0) * k1;
	p -= 2.0 * max(dot(k2, p), 0.0) * k2;
	p.x = abs(p.x);
	p.y -= r;
	float2 dNoise = voronoi_float(id);
	p += dNoise;
	float2 ba = rf * float2(-k1.y, k1.x) - float2(0, 1);
	float h = clamp(dot(p, ba) / dot(ba, ba), 0.0, r);

	return length(p - ba * h) * sign(p.y * ba.x - p.x * ba.y);
}

float mirroredStars_float(float2 p, float s)
{
	float2 id = round(p / s);
	float2  o = sign(p - s * id); // neighbor offset direction

	float d = 1e20;
	for (int j = 0; j < 2; j++)
		for (int i = 0; i < 2; i++)
		{
			float2 rid = id + float2(i, j) * o;
			float2 r = p - s * rid;
			d = min(d, sdStar5_float(r * 40., rid, 1., 0.5));
		}
	return d;
}

float skyBackground_float(float2 position, inout float4 skyColor)
{
	float2 dNoise = voronoi_float(position);
	float d = mirroredStars_float(position, 0.1);
	skyColor = d > 0.0 ? skyColor : float4(1., 1., 1., 1.);
	return d;
}


void SkySDF_float(float2 UV, float4 Color, float Time, out float Distance, out float4 FragmentColor)
{
	iTime = Time;

	Distance = skyBackground_float(UV, Color);
	FragmentColor = Color;
}
#endif //MYHLSLINCLUDE_INCLUDED
