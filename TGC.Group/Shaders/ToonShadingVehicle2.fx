/*
* Shader utilizado por el ejemplo "Lights/EjemploMultiDiffuseLights.cs"
* Permite aplicar iluminación dinámica con PhongShading a nivel de pixel.
* Soporta hasta 4 luces por objeto en la misma pasada.
* Las luces tienen atenuación por distancia.
* Solo se calcula el componente Diffuse para acelerar los cálculos. Se ignora
* el Specular.
*/

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

//Material del mesh
float3 materialEmissiveColor; //Color RGB
float3 materialDiffuseColor; //Color RGB

//Variables de las 4 luces
float3 lightColor[4]; //Color RGB de las 4 luces
float4 lightPosition[4]; //Posicion de las 4 luces
float lightIntensity[4]; //Intensidad de las 4 luces
float lightAttenuation[4]; //Factor de atenuacion de las 4 luces

/**************************************************************************************/
/* MultiDiffuseLightsTechnique */
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

	//Proyectar posicion
    output.Position = mul(input.Position, matWorldViewProj);

	//Enviar Texcoord directamente
    output.Texcoord = input.Texcoord;

	//Posicion pasada a World-Space (necesaria para atenuación por distancia)
    output.WorldPosition = mul(input.Position, matWorld);

	/* Pasar normal a World-Space
	Solo queremos rotarla, no trasladarla ni escalarla.
	Por eso usamos matInverseTransposeWorld en vez de matWorld */
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

//Funcion para calcular color RGB de Diffuse
float3 computeDiffuseComponent(float3 surfacePosition, float3 N, int i)
{
	//Calcular intensidad de luz, con atenuacion por distancia
    float distAtten = length(lightPosition[i].xyz - surfacePosition);
    float3 Ln = (lightPosition[i].xyz - surfacePosition) / distAtten;
    distAtten = distAtten * lightAttenuation[i];
    float intensity = lightIntensity[i] / distAtten; //Dividimos intensidad sobre distancia

	//Calcular Diffuse (N dot L)
    float LdotN = max(0.0, dot(N, Ln));
    float aux;

    if(LdotN > 0.942)
        aux = 1.69;
    else if(LdotN > 0.896)
        aux = 1.65;
    else if(LdotN > 0.816)
        aux = 1.62;
    else if(LdotN > 0.5)
        aux = 1.57;
    else if (LdotN > 0.2998)
        aux = 1.46;
    else if(LdotN > 0.2985)
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
    //else if (LdotN > 0.000001)
    //    aux = 0.7;
    else
        aux = 1.07;

    return intensity * lightColor[i].rgb * materialDiffuseColor * aux * 1.17;
}

//Pixel Shader para Point Light
float4 point_light_ps(PS_INPUT input) : COLOR0
{
    float3 Nn = normalize(input.WorldNormal);

	//Emissive + Diffuse de 4 luces PointLight
    float3 diffuseLighting = float3(0, 0, 0); //materialEmissiveColor;

	//Diffuse 0
    diffuseLighting += computeDiffuseComponent(input.WorldPosition, Nn, 0);

	////Diffuse 1
    //diffuseLighting += computeDiffuseComponent(input.WorldPosition, Nn, 1);

	////Diffuse 2
    //diffuseLighting += computeDiffuseComponent(input.WorldPosition, Nn, 2);

	////Diffuse 3
    //diffuseLighting += computeDiffuseComponent(input.WorldPosition, Nn, 3);

	//Obtener texel de la textura
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

technique RenderScene
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_general();
        PixelShader = compile ps_3_0 point_light_ps();
        CullMode = CW;
    }
    pass Pass_1
    {
        VertexShader = compile vs_3_0 OutlineVertexShader();
        PixelShader = compile ps_3_0 OutlinePixelShader();
        CullMode = CCW;
    }
}