float2 rotateUVDynamite_float(float2 position, float degrees)
{
    float sinX = sin(degrees);
    float cosX = cos(degrees);
    float sinY = sin(degrees);
    float2x2 rotationMatrix = float2x2(cosX, -sinX, sinY, cosX);
    return mul(position, rotationMatrix);
}

float sphereSDFDynamite_float(float2 pos)
{
    return abs(length(pos) - 1.)-0.05;
}

float sdCrossDynamite_float(in float2 p, in float2 b, float r)
{
    p = abs(p); p = (p.y>p.x) ? p.yx : p.xy;
    float2  q = p - b;
    float k = max(q.y,q.x);
    float2  w = (k>0.0) ? q : float2(b.y-p.x,-k);
    return sign(k)*length(max(w,0.0)) + r;
}

float sdRoundedXDynamite_float(float2 p, float w, float r)
{
    p = abs(p);
    return max(length(p-min(p.x+p.y,w)*0.5) - r, -1. * (length(p) - 0.5));
}

float crossSDFDynamite_float(float2 pos, float r)
{
    return abs(sdCrossDynamite_float(pos, float2(0.5 * r, 0.1 * r), 0.1 * r)) - 0.05 * r;
}


float sdBoxDynamite_float(float2 p, float2 b)
{
    float2 d = abs(p)-b;
    return abs(length(max(d, 0.0)) + min(max(d.x, d.y), 0.0)) - 0.045;
}

float sdStringDynamite_float(float2 p, float2 b)
{
    float2 d = abs(p) - b;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

float sdHexagonDynamite_float(float2 p, in float r)
{
    float3 k = float3(-0.866025404,0.5,0.577350269);
    p = abs(p);
    p -= 2.0*min(dot(k.xy,p),0.0)*k.xy;
    p -= float2(clamp(p.x, -k.z*r, k.z*r), r);
    return length(p)*sign(p.y);
}

float sdArcDynamite_float(float2 p, float2 sc, in float ra, float rb)
{
    p.x = abs(p.x);
    return ((sc.y*p.x>sc.x*p.y) ? length(p-sc*ra) : 
                                  abs(length(p)-ra)) - rb;
}

float dynamite_float(float2 pos)
{
    float2 rotatedPos = rotateUVDynamite_float(pos, -0.1);
    return min(
        min(sphereSDFDynamite_float(pos), sdBoxDynamite_float(rotateUVDynamite_float(pos + float2(0.6, -1.), 0.5), 0.2)),
            sdStringDynamite_float(float2(rotatedPos.x + sin((rotatedPos -0.15).y * 1.), (rotatedPos.y - 1.85) * 0.1) * 10.8, 0.8));

}

void CrossHairDynamite_float(float2 UV, float crossSize, float circleRadius, float hitMarkerRadius, out float Distance)
{
    Distance = min(dynamite_float(UV * circleRadius * 1.8), crossSDFDynamite_float(UV * 2.8, crossSize));
}
