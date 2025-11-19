#version 330 core
layout (location = 0) in vec3 aPosition;   // the position variable has attribute position 0
layout (location = 1) in vec2 aTexCoord; // the texture variable has attribute position 1

uniform float aspect;

out vec2 texCoord;
void main()
{
    // Setup texCoord
    texCoord = aTexCoord;

    // Scale pos with aspect
    vec3 p = aPosition;
    p.x /= aspect;
    gl_Position = vec4(p, 1.0);
}