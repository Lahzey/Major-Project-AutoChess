shader_type canvas_item;

uniform sampler2D screen_tex : hint_screen_texture;
uniform float blur_size = 4.0;

void fragment() {
    vec2 size = blur_size / SCREEN_PIXEL_SIZE;
    vec4 col = vec4(0.0);
    col += texture(screen_tex, SCREEN_UV + vec2(-size.x, -size.y));
    col += texture(screen_tex, SCREEN_UV + vec2( size.x, -size.y));
    col += texture(screen_tex, SCREEN_UV + vec2(-size.x,  size.y));
    col += texture(screen_tex, SCREEN_UV + vec2( size.x,  size.y));
    COLOR = col / 4.0;
}