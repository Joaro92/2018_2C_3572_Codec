/**************************************************************************************/
/* Variables comunes */
/**************************************************************************************/

//Matrices de transformacion
float4x4 matWorld; //Matriz de transformacion World
float4x4 matWorldView; //Matriz World * View
float4x4 matWorldViewProj; //Matriz World * View * Projection
float4x4 matInverseTransposeWorld; //Matriz Transpose(Invert(World))

float screen_dx = 0;
float screen_dy = 0;
float time = 0;

//Textura para DiffuseMap
texture texDiffuseMap;
sampler2D diffuseMap = sampler_state
{
    Texture = (texDiffuseMap);
    ADDRESSU = CLAMP;
    ADDRESSV = CLAMP;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
    MIPFILTER = LINEAR;
};

//--------------------------- DIFFUSE LIGHT PROPERTIES ------------------------------
// The direction of the diffuse light
float3 DiffuseLightDirection = float3(98, 400, -100);
 
// The color of the diffuse light
float4 DiffuseColor = float4(1, 1, 1, 1);
 
// The intensity of the diffuse light
float DiffuseIntensity = 1.06;
 
//--------------------------- TOON SHADER PROPERTIES ------------------------------
// The color to draw the lines in.  Black is a good default.
float4 LineColor = float4(0, 0, 0, 1);
 
// The thickness of the lines.  This may need to change, depending on the scale of
// the objects you are drawing.
float LineThickness = .015;


//--------------------------- DATA STRUCTURES ------------------------------
//Input del Vertex Shader
struct VS_INPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 Normal : NORMAL0;
};

//Output del Vertex Shader
struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float3 WorldPos : TEXCOORD2;
};


/**************************************************************************************/
/* RenderScene */
/**************************************************************************************/

// The vertex shader that does cel shading.
// It really only does the basic transformation of the vertex location,
// and normal, and copies the texture coordinate over.
VS_OUTPUT CelVertexShader(VS_INPUT input)
{
    VS_OUTPUT output;
 
    // Transform the position
    output.Position = mul(input.Position, matWorldViewProj);
    
    // Transform the normal
    output.Normal = input.Normal;
 
    // Copy over the texture coordinate
    output.Texcoord = input.Texcoord;

    output.WorldPos = mul(input.Position, matWorld);
 
    return output;
}

// The pixel shader that does cel shading.  Basically, it calculates
// the color like is should, and then it discretizes the color into
// one of four colors.
float4 CelPixelShader(VS_OUTPUT input) : COLOR0
{
    float3 light = mul(DiffuseLightDirection, matWorld) - mul(input.WorldPos, matWorld);
    // Calculate diffuse light amount
    float intensity = dot(normalize(light), input.Normal);
    if (intensity < 0)
        intensity = 0;
 
    // Calculate what would normally be the final color, including texturing and diffuse lighting
    float4 color = tex2D(diffuseMap, input.Texcoord) * DiffuseColor * DiffuseIntensity;
    color.w = 1;
 
    // Discretize the intensity, based on a few cutoff points
    if (intensity > 0.9)
        color = float4(0.991 * color.xyz, color.w);
    else if (intensity > 0.56)
        color = float4(0.9 * color.xyz, color.w);
    else if (intensity > 0.24)
        color = float4(0.83 * color.xyz, color.w);
    else if (intensity > 0.11)
        color = float4(0.76 * color.xyz, color.w);
    else if (intensity > 0.03)
        color = float4(0.67 * color.xyz, color.w);
    else
        color = float4(0.54 * color.xyz, color.w);
 
    return color;
}

// SEGUNDA PASADA

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
technique RenderScene
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 CelVertexShader();
        PixelShader = compile ps_3_0 CelPixelShader();
        CullMode = CW;
    }

    pass Pass_1
    {
        VertexShader = compile vs_3_0 OutlineVertexShader();
        PixelShader = compile ps_3_0 OutlinePixelShader();
        CullMode = CCW;
    }
}
