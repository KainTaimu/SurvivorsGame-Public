#[compute]
#version 450

// Pass 3 of the grid build: scatter entity slots into entityIndices
// grouped by cell. cellCount still holds the per-cell counts from the
// count pass; it is used as a decrement cursor, which drains it back
// to all zero for the next substep's count pass. Entities land in
// reverse order within their cell, which is irrelevant to the solver.

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

layout(set = 0, binding = 3, std430) restrict readonly buffer CellStart {
    int data[];
}
cellStart;

layout(set = 0, binding = 4, std430) restrict buffer EntityIndices {
    int data[];
}
entityIndices;

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

    // Old value runs from count down to 1, so position within the cell
    // is old - 1. Ends at zero for every cell.
    int old = atomicAdd(cellCount.data[cell], -1);
    entityIndices.data[cellStart.data[cell] + old - 1] = int(slot);
}
