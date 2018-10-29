/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

//Textura para DiffuseMap
texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
    Texture = (texDiffuseMap);
    ADDRESSU = WRAP;
    ADDRESSV = WRAP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//Textura para Lightmap
texture texLightMap;
sampler2D lightMap = sampler_state
{
    Texture = (texLightMap);
};

float3 materialDiffuseColor = float3(1, 1, 0.999); //Color RGB
float3 lightColor = float3(1, 1, 1); //Color RGB
float3 lightPosition = float3(400, 900, -80); //Posicion de la luz
float lightIntensity = 165; //Intensidad de la luz
float lightAttenuation = 0.29; //Factor de atenuacion de la luz

/**************************************************************************************/
/* Función auxiliar */
/**************************************************************************************/

float3 computeDiffuseComponent1(float3 surfacePosition, float3 N)
{
    float distAtten = length(lightPosition.xyz - surfacePosition);
    float3 Ln = (lightPosition.xyz - surfacePosition) / distAtten;
    distAtten = distAtten * lightAttenuation;
    float intensity = lightIntensity / distAtten;

    float LdotN = max(0.0, dot(N, Ln));
    float aux;

    if (LdotN > 0.942)
        aux = 1.69;
    else if (LdotN > 0.896)
        aux = 1.65;
    else if (LdotN > 0.816)
        aux = 1.62;
    else if (LdotN > 0.5)
        aux = 1.57;
    else if (LdotN > 0.2998)
        aux = 1.46;
    else if (LdotN > 0.2985)
        aux = 1.41;
    else if (LdotN > 0.281)
        aux = 1.37;
    else if (LdotN > 0.277)
        aux = 1.33;
    else if (LdotN > 0.255)
        aux = 1.299;
    else if (LdotN > 0.20)
        aux = 1.26;
    else if (LdotN > 0.1)
        aux = 1.21;
    else if (LdotN > 0.03)
        aux = 1.15;
    else
        aux = 1.07;

    return intensity * lightColor.rgb * materialDiffuseColor * aux * 1.17;
}

float3 computeDiffuseComponent2(float3 surfacePosition, float3 N)
{
    float distAtten = length(lightPosition.xyz - surfacePosition);
    float3 Ln = (lightPosition.xyz - surfacePosition) / distAtten;
    distAtten = distAtten * lightAttenuation;
    float intensity = lightIntensity / distAtten;

    float LdotN = max(0.0, dot(N, -Ln));
    float aux;

    if (LdotN > 0.942)
        aux = 1.69;
    else if (LdotN > 0.896)
        aux = 1.65;
    else if (LdotN > 0.816)
        aux = 1.62;
    else if (LdotN > 0.5)
        aux = 1.57;
    else if (LdotN > 0.2998)
        aux = 1.46;
    else if (LdotN > 0.2985)
        aux = 1.41;
    else if (LdotN > 0.281)
        aux = 1.37;
    else if (LdotN > 0.277)
        aux = 1.33;
    else if (LdotN > 0.255)
        aux = 1.299;
    else if (LdotN > 0.20)
        aux = 1.26;
    else if (LdotN > 0.1)
        aux = 1.21;
    else if (LdotN > 0.03)
        aux = 1.15;
    else
        aux = 1.07;

    return intensity * lightColor.rgb * materialDiffuseColor * aux;
}

/**************************************************************************************/
/* Definición Vertex Shader y Pixel Shader */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT
{
    float4 Position : POSITION0;
    float3 Normal : NORMAL0;
    float4 Color : COLOR;
    float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 WorldPosition : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
};

//Vertex Shader
VS_OUTPUT vs_general(VS_INPUT input)
{
    VS_OUTPUT output;

    output.Position = mul(input.Position, matWorldViewProj);
    output.Texcoord = input.Texcoord;
    output.WorldPosition = mul(input.Position, matWorld);
    output.WorldNormal = mul(input.Normal, matInverseTransposeWorld).xyz;

    return output;
}

//Input del Pixel Shader
struct PS_INPUT
{
    float2 Texcoord : TEXCOORD0;
    float3 WorldPosition : TEXCOORD1;
    float3 WorldNormal : TEXCOORD2;
};

//Pixel Shader para el Vehiculo
float4 ps_toon_vehicle(PS_INPUT input) : COLOR0
{
    float3 Nn = normalize(input.WorldNormal);
    float3 diffuseLighting = float3(0, 0, 0);

    diffuseLighting += computeDiffuseComponent1(input.WorldPosition, Nn);

    float4 texelColor = tex2D(diffuseMap, input.Texcoord);
    texelColor.rgb *= diffuseLighting;

    return texelColor;
}

//Pixel Shader para todo lo demas
float4 ps_toon(PS_INPUT input) : COLOR0
{
    float3 Nn = normalize(input.WorldNormal);
    float3 diffuseLighting = float3(0, 0, 0);

    diffuseLighting += computeDiffuseComponent2(input.WorldPosition, Nn);

    float4 texelColor = tex2D(diffuseMap, input.Texcoord);
    texelColor.rgb *= diffuseLighting;

    return texelColor;
}

// SEGUNDA PASADA
float4 LineColor = float4(0, 0, 0, 1);
float LineThickness = .008;

// The vertex shader that does the outlines
VS_OUTPUT OutlineVertexShader(VS_INPUT input)
{
    VS_OUTPUT output = (VS_OUTPUT) 0;
 
    // Calculate where the vertex ought to be.  This line is equivalent
    // to the transformations in the CelVertexShader.
    float4 original = mul(input.Position, matWorldViewProj);
 
    // Calculates the normal of the vertex like it ought to be.
    float4 normal = mul(input.Normal, matWorldViewProj);
 
    // Take the correct "original" location and translate the vertex a little
    // bit in the direction of the normal to draw a slightly expanded object.
    // Later, we will draw over most of this with the right color, except the expanded
    // part, which will leave the outline that we want.
    output.Position = original + (mul(LineThickness, normal));
 
    return output;
}
 
// The pixel shader for the outline.  It is pretty simple:  draw everything with the
// correct line color.
float4 OutlinePixelShader(VS_OUTPUT input) : COLOR0
{
    return LineColor;
}

// ------------------------------------------------------------------

technique ToonShadingWithBorder
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_general();
        PixelShader = compile ps_3_0 ps_toon_vehicle();
        CullMode = CW;
    }

    pass Pass_1
    {
        VertexShader = compile vs_3_0 OutlineVertexShader();
        PixelShader = compile ps_3_0 OutlinePixelShader();
        CullMode = CCW;
    }
}

technique ToonShading
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_general();
        PixelShader = compile ps_3_0 ps_toon();
        CullMode = CW;
    }
}