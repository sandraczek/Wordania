#ifndef DYNAMIC_LIGHTING_INCLUDED
#define DYNAMIC_LIGHTING_INCLUDED

uniform float4 _DynamicLightData[32]; 
uniform float4 _DynamicLightColors[32];
uniform int _DynamicLightCount;

void CalculateDynamicLights_float(float2 WorldPos, out float3 AddedLight)
{
    float3 totalDynamicLight = float3(0, 0, 0);

    for(int i = 0; i < _DynamicLightCount; i++) 
    {
        float2 lightPos = _DynamicLightData[i].xy;
        float radius = _DynamicLightData[i].z;
        float intensity = _DynamicLightData[i].w;
        float3 color = _DynamicLightColors[i].rgb;

        float dist = distance(WorldPos, lightPos);
        
        float atten = saturate(1.0 - (dist / radius));
        
        atten = atten * atten; 

        totalDynamicLight += color * atten * intensity;
    }

    AddedLight = totalDynamicLight;
}

#endif