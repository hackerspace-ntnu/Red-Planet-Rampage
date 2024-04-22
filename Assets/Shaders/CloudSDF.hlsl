#include "Assets/Shaders/StarSdf.hlsl"
float CloudGround_float(float pX)
{
	return 0.6 * (0.5 * sin(0.1 * pX) + 0.5 * sin(0.553 * pX) + 0.7 * sin(1.2 * pX));
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

float cloudSdf_float(in float2 position, inout float4 cloudColor)
{
	float i = 0.5;
	float Lt = iTime * (0.5 + 2.0 * i) * (1.0 + 0.1 * sin(226.0 * i)) + 17.0 * i;
	float2 Lp = float2(0.0, 0.3 + 1.5 * (i - 0.5));
	float d = limitedRepeatedSkies_float(position + Lp, Lt);
	cloudColor = d > 0.95 ? cloudColor : float4(0.5, 0.5, 0.5, 0.5);
	cloudColor = d > 0.0 ? cloudColor : float4(1., 1., 1., 1.);
	return d;
}

void CloudSDF_float(float2 UV, float4 Color, float Time, out float Distance, out float4 FragmentColor)
{
	iTime = Time;
	
	Distance = cloudSdf_float(UV * float2(5.0, 1.0) * 20.0 - float2(0.0, 1.3), Color);
	FragmentColor = Color;
}

