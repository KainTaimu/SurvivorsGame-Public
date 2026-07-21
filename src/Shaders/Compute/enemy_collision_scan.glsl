#[compute]
#version 450

// Pass 2 of the grid build: exclusive prefix sum of cellCount into
// cellStart. cellStart has cellTotal + 1 entries; the last entry holds
// the total number of gridded entities so the solve pass can derive
// per-cell counts as cellStart[i + 1] - cellStart[i].
//
// Runs as a single workgroup. Each invocation serially scans one chunk
// of cells, then chunk totals are prefix-summed in shared memory and
// added back. cellCount is left untouched; the scatter pass relies on
// the counts and drains them back to zero.

layout(local_size_x = 1024, local_size_y = 1, local_size_z = 1) in;

layout(set = 0, binding = 2, std430) restrict readonly buffer CellCount {
	int data[];
}
cellCount;

layout(set = 0, binding = 3, std430) restrict buffer CellStart {
	int data[];
}
cellStart;

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

shared int chunkTotals[1024];
shared int chunkPrefix[1024];

void main() {
	int cellTotal = params.dimX * params.dimY;
	// Indices 0..cellTotal inclusive are written (cellTotal gets the sum).
	int total = cellTotal + 1;
	int lid = int(gl_LocalInvocationID.x);
	int chunk = (total + 1023) / 1024;
	int chunkBase = lid * chunk;

	int sum = 0;
	for (int k = 0; k < chunk; k++) {
		int idx = chunkBase + k;
		if (idx >= total) {
			break;
		}
		int count = idx < cellTotal ? cellCount.data[idx] : 0;
		cellStart.data[idx] = sum;
		sum += count;
	}
	chunkTotals[lid] = sum;

	barrier();

	if (lid == 0) {
		int running = 0;
		for (int i = 0; i < 1024; i++) {
			chunkPrefix[i] = running;
			running += chunkTotals[i];
		}
	}

	barrier();

	int prefix = chunkPrefix[lid];
	for (int k = 0; k < chunk; k++) {
		int idx = chunkBase + k;
		if (idx >= total) {
			break;
		}
		cellStart.data[idx] += prefix;
	}
}
