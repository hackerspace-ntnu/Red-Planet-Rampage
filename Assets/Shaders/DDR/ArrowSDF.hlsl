//UNITY_SHADER_NO_UPGRADE
#ifndef MYHLSLINCLUDE_INCLUDED
#define MYHLSLINCLUDE_INCLUDED

// Source: https://www.shadertoy.com/view/slj3Dd, translated to HLSL
// p = position, a = origin, b = destination
// w1 = line width, w2 = arrowhead size
// k = arrow head ratio
float sdArrow_float( float2 p, float2 a, float2 b, float w1, float w2, float k )
{
  // constant setup
	float2  ba = b - a;
  float l2 = dot(ba,ba);
  float l = sqrt(l2);

  // pixel setup
  p = p-a;
  p = mul(float2x2(ba.x,-ba.y,ba.y,ba.x),p/l);
  p.y = abs(p.y);
  float2 pz = p-float2(l-w2*k,w2);

  // === distance (four segments) === 

  float2 q = p;
  q.x -= clamp( q.x, 0.0, l-w2*k );
  q.y -= w1;
  float di = dot(q,q);
  //----
  q = pz;
  q.y -= clamp( q.y, w1-w2, 0.0 );
  di = min( di, dot(q,q) );
  //----
  if( p.x<w1 ) // conditional is optional
  {
  q = p;
  q.y -= clamp( q.y, 0.0, w1 );
  di = min( di, dot(q,q) );
  }
  //----
  if( pz.x>0.0 ) // conditional is optional
  {
  q = pz;
  q -= float2(k,-1.0)*clamp( (q.x*k-q.y)/(k*k+1.0), 0.0, w2 );
  di = min( di, dot(q,q) );
  }
  
  // === sign === 
  
  float si = 1.0;
  float z = l - p.x;
  if( min(p.x,z)>0.0 ) //if( p.x>0.0 && z>0.0 )
  {
    float h = (pz.x<0.0) ? w1 : z/k;
    if( p.y<h ) si = -1.0;
  }
  return si*sqrt(di);
}

void ArrowSDF_float(float2 UV, float4 Color, float2 Start, float2 End, float LineSize, float ArrowSize, float ArrowRatio, out float Distance, out float4 FragmentColor)
{
	Distance = sdArrow_float(UV, Start, End, LineSize, ArrowSize, ArrowRatio);
	FragmentColor = lerp(float4(0., 0., 0., 0.), Color, smoothstep(0., -.03, Distance));
}

void HollowArrowSDF_float(float2 UV, float4 Color, float2 Start, float2 End, float LineSize, float ArrowSize, float ArrowRatio, float ShellThickness, out float Distance, out float4 FragmentColor)
{
	Distance = abs(sdArrow_float(UV, Start, End, LineSize, ArrowSize, ArrowRatio)) - ShellThickness;
	FragmentColor = lerp(float4(0., 0., 0., 0.), Color, smoothstep(0., -.03, Distance));
}
#endif //MYHLSLINCLUDE_INCLUDED
