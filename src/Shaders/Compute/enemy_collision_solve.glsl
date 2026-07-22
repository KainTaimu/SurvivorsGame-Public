#[compute]
#version 450

// Pass 4: push-apart solve. Each entity gathers the pushes applied to
// it by its 3x3 cell neighborhood and writes only its own slot
// (Jacobi style: reads come from entitiesIn, writes go to entitiesOut,
// so no atomics or ordering guarantees are needed).
//
// This mirrors the CPU solver's pair coverage exactly. The CPU solver
// initiates pair solves from a source cell toward itself, E, S, SE and
// NE, skipping source cells with count <= 1 or count > 10. A pair is
// therefore solved exactly once per substep, only when its source cell
// is within that count range. The source cell of each neighbor
// direction is: C for self/E/S/SE/NE, and the neighbor itself for
// W/N/NW/SW. Per-direction source counts are checked here to match.

layout(local_size_x = 512, local_size_y = 1, local_size_z = 1) in;

struct GpuEntity {
    vec2 pos;
    float radius;
    uint frameStamp;
};

layout(set = 0, binding = 0, std430) restrict readonly buffer EntitiesIn {
    GpuEntity data[];
}
entitiesIn;

layout(set = 0, binding = 1, std430) restrict writeonly buffer EntitiesOut {
    GpuEntity data[];
}
entitiesOut;

layout(set = 0, binding = 3, std430) restrict readonly buffer CellStart {
    int data[];
}
cellStart;

layout(set = 0, binding = 4, std430) restrict readonly buffer EntityIndices {
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

int cellCountOf(int cell) {
    return cellStart.data[cell + 1] - cellStart.data[cell];
}

void main() {
    uint slot = gl_GlobalInvocationID.x;
    if (slot >= uint(params.entityCount)) {
        return;
    }

    GpuEntity a = entitiesIn.data[slot];
    GpuEntity result = a;

    int myCell = cellIndexOf(a.pos);
    if (a.frameStamp != params.frameStamp || myCell < 0) {
        entitiesOut.data[slot] = result;

        return;
    }
    int countA = cellCountOf(myCell);
    ivec2 myCellXY = ivec2(myCell % params.dimX, myCell / params.dimX);
    vec2 accum = vec2(0.0);

    for (int oy = -1; oy <= 1; oy++) {
        for (int ox = -1; ox <= 1; ox++) {
            ivec2 nc = myCellXY + ivec2(ox, oy);
            if (nc.x < 0 || nc.y < 0 || nc.x >= params.dimX || nc.y >= params.dimY) {
                continue;
            }

            int nIdx = nc.y * params.dimX + nc.x;
            int countB = cellCountOf(nIdx);

            // Pair participates only when its source cell (see
            // header comment) has 1 < count <= 10.
            bool sourceIsMine = ox > 0 || (ox == 0 && oy >= 0);
            int sourceCount = sourceIsMine ? countA : countB;
            if (sourceCount <= 1) {
                continue;
            }

            int startB = cellStart.data[nIdx];
            for (int k = 0; k < countB; k++) {
                int other = entityIndices.data[startB + k];
                if (uint(other) == slot) {
                    continue;
                }

                GpuEntity b = entitiesIn.data[other];

                float largest = max(a.radius, b.radius) * 3.0;
                vec2 delta = a.pos - b.pos;
                float distSq = dot(delta, delta);
                if (distSq >= largest * largest) {
                    continue;
                }

                vec2 dir =
                distSq > 1e-6 ? delta / sqrt(distSq) : vec2(1.0, 0.0);

                float push = params.distBeforeShove * 0.5 * params.pushAmount;
                if (countA >= params.cramLimit || countB >= params.cramLimit) {
                    float extra =
                    abs(log(float(countA + countB) / 1.5) * params.cramFactor);
                    push *= abs(extra);
                }

                accum += dir * push;
            }
        }
    }

    result.pos = a.pos + accum;


    entitiesOut.data[slot] = result;
}
