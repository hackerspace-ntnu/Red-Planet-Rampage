#include "Assets/Shaders/StarSdf.hlsl"
float CloudGround_float(float pX)
{
	return 0.6 * (0.5 * sin(0.1 * pX) + 0.5 * sin(0.553 * pX) + 0.7 * sin(1.2 * pX));
}

float2 tiledHash_float(float2 p)
{
	p.x = sin(p.x*2.); // tile
	p.y = sin(p.y*2.);
	p = float2(dot(p, float2(127.1, 311.7)),
		dot(p, float2(269.5, 183.3)));
	return frac(sin(p) * 18.5453);
}

float CloudRound_float(float pX)
{
	return 0.5 + 0.25 * (1.0 + sin(fmod(40.0 * pX, 6.283)));
}

float sdCutDisk_float(in float2 p, in float r, in float h)
{
	float w = sqrt(r * r - h * h);

	p.x = abs(p.x);

	// select circle or segment
	float s = max((h - r) * p.x * p.x + w * w * (h + r - 2.0 * p.y), h * p.x - w * p.y);

	return (s < 0.0) ? length(p) - r :        // circle
		(p.x < w) ? h - p.y :        // segment line
		length(p - float2(w, h)); // segment corner
}


float limitedRepeatedSkies_float(float2 p, float offset)
{
	float2 Qt = 3.5 * p;
	offset *= 0.5;
	Qt.x += offset;

	float Xi = floor(Qt.x);
	float Xf = Qt.x - Xi - 0.5;

	float2 C;
	float Yi;
	float D = 1. - step(Qt.y, CloudGround_float(Qt.x));

	// Disk:
	Yi = CloudGround_float(Xi + 0.5);
	C = float2(Xf, Qt.y - Yi);
	D = min(D, length(C) - CloudRound_float(Xi + offset / 80.0));

	// Previous disk:
	Yi = CloudGround_float(Xi + 1.0 + 0.5);
	C = float2(Xf - 1.0, Qt.y - Yi);
	D = min(D, length(C) - CloudRound_float(Xi + 1.0 + offset / 80.0));

	// Next Disk:
	Yi = CloudGround_float(Xi - 1.0 + 0.5);
	C = float2(Xf + 1.0, Qt.y - Yi);
	D = min(D, length(C) - CloudRound_float(Xi - 1.0 + offset / 80.0));

	return min(1.0, D);
}

float sMax_float(float x, float y)
{
	return log(exp(x * 20.) + exp(y * 10.)) / 20.;
}

float cloudSdf_float(in float2 position, float2 hashedId, inout float4 cloudColor)
{
	float i = 0.5;
	float Lt = (iTime/4.0 + hashedId) * (0.5 + 2.0 * i) * (1.0 + 0.1 * sin(226.0 * i)) + 17.0 * i;
	float2 Lp = float2(0.0, 0.3 + 1.5 * (i - 0.5));
	float d = sMax_float(
		limitedRepeatedSkies_float(
			position + Lp + hashedId,
			Lt),
		sdCutDisk_float(
			position * 2.0 + float2(0.0, 1.4 + hashedId.y * 0.1),
			2.0 - hashedId.x * 0.002,
			0.2));
	cloudColor = d > 0.1 ? cloudColor : float4(0.5, 0.5, 0.5, 0.5);
	cloudColor = d > 0.0 ? cloudColor : float4(1., 1., 1., 1.);
	return d;
}

float mirroredClouds_float(float2 p, float s, inout float4 color)
{
	p = p * float2(5.0, 1.0) * 0.2;// -float2(iTime / 256, 0.0);
	float2 id = round(p / s);
	float2  o = sign(p - s * id); // neighbor offset direction
	float d = 1e20;
	for (int j = 0; j < 2; j++)
		for (int i = 0; i < 2; i++)
		{
			float2 rid = id + float2(i, j) * o;
			float2 r = p - s * rid;
			float2 hashedId = tiledHash_float(rid);
			d = min(d, cloudSdf_float(r * float2(30., 40.) + float2(0.0, 1.5+ hashedId.y * 1.5), hashedId, color));
		}
	return d;
}

void CloudSDF_float(float2 UV, float4 Color, float Time, out float Distance, out float4 FragmentColor)
{
	iTime = Time;
	
	Distance = mirroredClouds_float(UV + float2(0.15, 0.0), 0.1, Color);
	FragmentColor = Color;
}

