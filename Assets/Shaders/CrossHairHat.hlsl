float sphereSDFHat_float(float2 pos)
{
    return abs(length(pos) - 1.)-0.05;
}

float sdCrossHat_float( in float2 p, in float2 b, float r ) 
{
    p = abs(p); p = (p.y>p.x) ? p.yx : p.xy;
    float2  q = p - b;
    float k = max(q.y,q.x);
    float2  w = (k>0.0) ? q : float2(b.y-p.x,-k);
    return sign(k)*length(max(w,0.0)) + r;
}

float sdRoundedXHat_float( float2 p, float w, float r )
{
    p = abs(p);
    return max(length(p-min(p.x+p.y,w)*0.5) - r, -1. * (length(p) - 0.5));
}

float crossSDFHat_float(float2 pos, float r)
{
    return abs(sdCrossHat_float(pos, float2(0.5 * r, 0.1 * r), 0.1*r))-0.05*r;
}


float sdBoxHat_float( float2 p, float2 b )
{
    float2 d = abs(p)-b;
    return length(max(d,0.0)) + min(max(d.x,d.y),0.0);
}
float sdHexagonHat_float( float2 p, in float r )
{
    float3 k = float3(-0.866025404,0.5,0.577350269);
    p = abs(p);
    p -= 2.0*min(dot(k.xy,p),0.0)*k.xy;
    p -= float2(clamp(p.x, -k.z*r, k.z*r), r);
    return length(p)*sign(p.y);
}

float sdArcHat_float( float2 p, float2 sc, in float ra, float rb )
{
    p.x = abs(p.x);
    return ((sc.y*p.x>sc.x*p.y) ? length(p-sc*ra) : 
                                  abs(length(p)-ra)) - rb;
}

float hat_float(float2 pos)
{
    return 
        max( 
            min(
                max(sdBoxHat_float(pos-float2(0.,0.5), float2(.5, .8)),sdHexagonHat_float(pos- float2(0.,-0.1), 0.4)-0.1),
                sdArcHat_float(float2(pos.x, pos.y*-1. + 3.7), float2(sin(0.2),cos(0.2)), 4., 0.04)
                ),
            -1. * max(sdBoxHat_float(pos*1.1-float2(0.,0.5), float2(.5, .8)), sdHexagonHat_float(pos*1.1- float2(0.,-0.1), 0.4)-0.1)
        );
}

void CrossHairHat_float(float2 UV, float crossSize, float circleRadius, float hitMarkerRadius, out float Distance)
{
	Distance = min(min(hat_float(UV * circleRadius * crossSize + float2(0., 0.05)), crossSDFHat_float(UV*2., 0.6)), sdRoundedXHat_float(UV, hitMarkerRadius, 0.1));
}
