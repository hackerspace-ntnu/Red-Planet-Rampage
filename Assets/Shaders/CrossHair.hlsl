float sphereSDF_float(float2 pos)
{
    return abs(length(pos) - 1.)-0.05;
}

float sdCross_float( in float2 p, in float2 b, float r ) 
{
    p = abs(p); p = (p.y>p.x) ? p.yx : p.xy;
    float2  q = p - b;
    float k = max(q.y,q.x);
    float2  w = (k>0.0) ? q : float2(b.y-p.x,-k);
    return sign(k)*length(max(w,0.0)) + r;
}

float sdRoundedX_float( float2 p, float w, float r )
{
    p = abs(p);
    return max(length(p-min(p.x+p.y,w)*0.5) - r, -1. * (length(p) - 0.5));
}

float crossSDF_float(float2 pos)
{
    return abs(sdCross_float(pos, float2(0.5, 0.1), 0.1))-0.05;
}

void CrossHair_float(float2 UV, float crossSize, float circleRadius, float hitMarkerRadius, out float Distance)
{
	Distance = min(min(sphereSDF_float(UV * 2.5 * circleRadius), crossSDF_float(UV * crossSize)), sdRoundedX_float(UV, hitMarkerRadius, 0.1));
}
