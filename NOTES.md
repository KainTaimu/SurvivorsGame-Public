### Current limitations
Fodder spritesheets MUST be horizontal, 3-sprites wide. See `enemy.gdshader`

### Notes
Fodder enemies uses MultiMeshInstance2D for rendering. Each instance uses custom data (stored in a Color Godot type) to pass to `enemy.gdshader`. Currently ordered as:
- X: Flash (ranged from 0f -1f)
- Y: Frame index (from 0 - 2)
- Z: Flip (0 or 1, where 1 flips the sprite horizontally)
- W: Opacity (from 0f - 1f)

Flash is used to whiten spritee
