#version 330 core
out vec4 FragColor;
in vec2 texCoord;

uniform sampler2D texture0;
uniform float lgbtqa_speed;
uniform float time;

void main()
{
    float calc_time = time*lgbtqa_speed;
    float r = (sin(calc_time)+1)/2;
    float g = ((sin(calc_time - 3.1415 * 1.3)+1))/2;
    float b = (sin((calc_time + 3.1415 * 1.3)+1))/2;
    FragColor = vec4((texture(texture0, vec2(texCoord.x, -texCoord.y)).xyz * vec3(r,g,b)), 1);
}
