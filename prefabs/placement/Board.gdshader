shader_type canvas_item;

uniform vec4 normal_color : source_color;
uniform vec4 highlight_color : source_color;
uniform float highlight_shrink : hint_range(1.0, 3.0, 0.01);
uniform vec4 highlight_range;
uniform int columns : hint_range(1, 20);
uniform int rows : hint_range(1, 20);
uniform sampler2D edge_curve : repeat_disable, filter_linear;

bool bothLargerOrEqual(vec2 v, vec2 than) {
	return v.x > than.x && v.y > than.y;
}


void fragment() {
    vec2 local_pos = UV * vec2(float(columns), float(rows));
	bool in_highlight = bothLargerOrEqual(local_pos, highlight_range.xy) && bothLargerOrEqual(highlight_range.zw, local_pos);
	vec2 pos_in_cell = mod(local_pos, 1.0);
	pos_in_cell = in_highlight ? pos_in_cell * highlight_shrink - vec2(highlight_shrink * 0.5f - 0.5f) : pos_in_cell; // hopefully ? is a bit faster than if
	vec2 dist_to_edge = vec2(pos_in_cell.x > 0.5 ? 1.0 - pos_in_cell.x : pos_in_cell.x, pos_in_cell.y > 0.5 ? 1.0 - pos_in_cell.y : pos_in_cell.y);
	float alpha = texture(edge_curve, vec2(min(dist_to_edge.x, dist_to_edge.y), 0.0)).r;


	COLOR = in_highlight ? highlight_color : normal_color;
	COLOR.a = alpha;
}