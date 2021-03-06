#version 400

in vec2 TexCoordO;
in vec3 NormalO;
in vec3 WorldPosO;

struct PointLight
{
vec3 Color ;
float AmbientIntensity;
float DiffuseIntensity;
float Constant ; 
float Linear ;
float Exp;
vec3 Position;
};

uniform PointLight Pointlight;
uniform sampler2D ColorMap;
uniform samplerCube ShadowMap;
uniform vec3 EyeWorldPos;
uniform float MatSpecularIntensity;
uniform float SpecularPower;
uniform float FarPlane;
uniform bool IsWhite;


out vec4 Color;


struct Data {
	vec2 TexCoord; 
	vec3 Normal ;
	vec3 WorldPos;
};



float CalcShadowFactor(vec3 lightDir){

lightDir.y = -lightDir.y;
float sampledDepth = (texture(ShadowMap, lightDir).r)*FarPlane;


if (sampledDepth+0.05 >FarPlane)
return 1.0;

float currentDepth = length(lightDir);

//Color = vec4(vec3(sampledDepth / FarPlane), 1.0);  

if (currentDepth<= sampledDepth+0.05 )
return 1.0;
else  return 0.0;

}


vec4 CalcPointLight(PointLight l  ,Data In ){
vec3 lightDirection  = In.WorldPos - l.Position;
// calc shadowFactor 
float shadowFactor = CalcShadowFactor(lightDirection);

float  lightDistance = length(lightDirection);
lightDirection = normalize(lightDirection);

// ambient Diffuse specular Calculations
vec4 ambientColor = vec4(l.Color * l.AmbientIntensity,1.0);
float diffuseFactor =max(0.0,dot(In.Normal,-lightDirection)); 
vec4 diffuseColor = vec4(l.Color*l.DiffuseIntensity*diffuseFactor,1.0);

vec3 vertexToEye = normalize(EyeWorldPos-In.WorldPos);
vec3 lightReflect = normalize(reflect(lightDirection,In.Normal));
float specularFactor = max(0.0,dot(vertexToEye,lightReflect));
specularFactor = pow(specularFactor,SpecularPower);
vec4 specularColor = vec4(l.Color*MatSpecularIntensity*specularFactor,1.0);


/// calc attenuation for point light
float attenuation = l.Constant + l.Linear*lightDistance + l.Exp * lightDistance*lightDistance;

return (ambientColor+shadowFactor*(diffuseColor+specularColor))/attenuation;
}

void main(){
Data d ;
d.TexCoord = TexCoordO;
d.Normal = normalize(NormalO);
d.WorldPos = WorldPosO;

vec4 TotalLight =CalcPointLight(Pointlight,d); 
vec4 tex = texture(ColorMap,TexCoordO.xy);
if(!IsWhite)
Color =tex * TotalLight ;
else {
Color=vec4(1.0,1.0,1.0,1.0);
}
//vec3 lightDirection  = WorldPosO - Pointlight.Position;
//Color =texture(ShadowMap,lightDirection);
}