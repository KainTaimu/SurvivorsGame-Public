#[compute]
#version 450

// Pass 1 of the grid build: count entities per cell.
// cellCount must be all zero on entry (invariant held by the scatter
// pass of the previous substep, and by zero-initialized buffers on the
// very first frame).

layout(local_size_x = 64, local_size_y = 1, local_size_z = 1) in;

struct GpuEntity {
    vec2 pos;
    float radius;
    uint frameStamp;
};

layout(set = 0, binding = 0, std430) restrict readonly buffer EntitiesIn {
    GpuEntity data[];
}
entitiesIn;

layout(set = 0, binding = 2, std430) restrict buffer CellCount {
    int data[];
}
cellCount;

layout(push_constant, std430) uniform Params {
    vec2 topLeft;
    float cellSize;
    int dimX;
    int dimY;
    int entityCount;
    float distBeforeShove;
    float pushAmount;
    int cramLimit;
    float cramFactor;
    uint frameStamp;
    int pad;
}
params;

// Returns the flat cell index for a world position, or -1 when the
// position lies outside the grid window.
int cellIndexOf(vec2 pos) {
    vec2 local = pos - params.topLeft;
    ivec2 cell = ivec2(floor(local / params.cellSize));
    if (cell.x < 0 || cell.y < 0 || cell.x >= params.dimX || cell.y >= params.dimY) {
        return -1;
    }
    return cell.y * params.dimX + cell.x;
}

void main() {
    uint slot = gl_GlobalInvocationID.x;
    if (slot >= uint(params.entityCount)) {
        return;
    }

    GpuEntity e = entitiesIn.data[slot];
    if (e.frameStamp != params.frameStamp) {
        return;
    }

    int cell = cellIndexOf(e.pos);
    if (cell < 0) {
        return;
    }

    atomicAdd(cellCount.data[cell], 1);
}
