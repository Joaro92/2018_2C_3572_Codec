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

float time = 0;
float time2 = 0;

/**************************************************************************************/
/* Explosion */
/**************************************************************************************/

//Input del Vertex Shader
struct VS_INPUT
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 Texcoord : TEXCOORD0;
};

//Output del Vertex Shader
struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float3 Normal : TEXCOORD2;
};


VS_OUTPUT VertexShaderFunction(VS_INPUT Input, float3 Normal : NORMAL)
{
    VS_OUTPUT output;
 
    // Just some random sin/cos equation to make things look random 
    float angle = (time % 360) * 2;
    float freqx = 3.4f + sin(time) * 1.0f;
    float freqy = 4.0f + sin(time * 1.3f) * 2.0f;
    float freqz = 2.1f + sin(time * 1.1f) * 3.0f;
    float amp = 1.0f + sin(time * 3.4) * 2.0f;
     
    float f = sin(Normal.x * freqx + time * 1.8) * sin(Normal.y * freqy + time * 1.5) * sin(Normal.z * freqz + time * 2) * 0.95;
    Input.Position.z += Normal.z * freqz * amp * f;
    Input.Position.x += Normal.x * freqx * amp * f;
    Input.Position.y += Normal.y * freqy * amp * f;

    Input.Position.x *= max(sin(time2 * 2.2), 0);
    Input.Position.y *= max(sin(time2 * 2.2), 0);
    Input.Position.z *= max(sin(time2 * 2.2), 0);

    output.Position = mul(Input.Position, matWorldViewProj);
    float3 normal = normalize(mul(Normal, matWorld));
    output.Normal = normal;
    output.Texcoord = Input.Texcoord;
    //output.View = normalize(float4(EyePosition, 1.0) - worldPosition);
 
    return output;
}

float4 ps_main(VS_OUTPUT Input) : COLOR0
{
    return tex2D(diffuseMap, Input.Texcoord);
}


/**************************************************************************************/
/* Ring */
/**************************************************************************************/

//Output del Vertex Shader
struct VS_OUTPUT2
{
    float4 Position : POSITION0;
    float2 Texcoord : TEXCOORD0;
    float Min : TEXCOORD1;
};


VS_OUTPUT2 vs_ring(VS_INPUT Input, float3 Normal : NORMAL)
{
    VS_OUTPUT2 output;

    Input.Position.x *= min(max(sin(time2 * 2.2) * 1.11, 0.28), 1.03);
    Input.Position.y *= min(max(sin(time2 * 2.2) * 1.11, 0.28), 1.03);
    Input.Position.z *= min(max(sin(time2 * 2.2) * 1.11, 0.28), 1.03);

    output.Position = mul(Input.Position, matWorldViewProj);
    float3 normal = normalize(mul(Normal, matWorld));
    output.Min = max(sin(time2 * 2.2) * 1.11, 0.28);
    output.Texcoord = Input.Texcoord;
    //output.View = normalize(float4(EyePosition, 1.0) - worldPosition);
 
    return output;
}

float4 ps_main2(VS_OUTPUT2 Input) : COLOR0
{
    //Input.Texcoord.y = y / screen_dy;
    float4 asd = tex2D(diffuseMap, Input.Texcoord);
    asd.a *= min((max(sin(time2 * 4.4), 0) + 0.1), 1);

    if (max(sin(time2 * 4.4), 0) < 0.01)
        discard;
    if (Input.Min < 0.281)
        discard;

    return asd;
}

// ------------------------------------------------------------------

technique Explosion
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 ps_main();
    }
}

technique Ring
{
    pass Pass_0
    {
        VertexShader = compile vs_3_0 vs_ring();
        PixelShader = compile ps_3_0 ps_main2();
    }
}