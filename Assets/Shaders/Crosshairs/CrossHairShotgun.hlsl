float sphereSDFShotgun_float(float2 pos)
{
    return abs(length(pos) - 1.)-0.05;
}

float sdCrossShotgun_float( in float2 p, in float2 b, float r ) 
{
    p = abs(p); p = (p.y>p.x) ? p.yx : p.xy;
    float2  q = p - b;
    float k = max(q.y,q.x);
    float2  w = (k>0.0) ? q : float2(b.y-p.x,-k);
    return sign(k)*length(max(w,0.0)) + r;
}

float sdRoundedXShotgun_float( float2 p, float w, float r )
{
    p = abs(p);
    return max(length(p-min(p.x+p.y,w)*0.5) - r, -1. * (length(p) - 0.5));
}

float crossSDFShotgun_float(float2 pos, float r)
{
    return abs(sdCrossShotgun_float(pos, float2(0.5 * r, 0.1 * r), 0.1*r))-0.05*r;
}


float sdBox_float(float2 p, float2 b )
{
    float2 d = abs(p)-b;
    return length(max(d,0.0)) + min(max(d.x,d.y),0.0);
}

float shotgun_float(float2 pos)
{
    return max(sdBox_float(pos, float2(.5, .5))-0.1, -1. * crossSDFShotgun_float(pos, 8.));
}

void CrossHairShotgun_float(float2 UV, float crossSize, float circleRadius, float hitMarkerRadius, out float Distance)
{
    UV *= 0.9;
	Distance = min(min(shotgun_float(UV * circleRadius), crossSDFShotgun_float(UV * crossSize, 0.4)), sdRoundedXShotgun_float(UV, hitMarkerRadius, 0.1));
}
