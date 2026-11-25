class_name EnemyCollisionSolverManager
extends Node

@export var default_solver: SolverTypeEnum = SolverTypeEnum.AUTO

var solver_type: SolverTypeEnum
var current_solver: AbstractEnemyCollisionSolver = null

enum SolverTypeEnum {
	NONE = 0,
	AUTO,
	CPU,
	GPU,
}

const SOLVER_TYPE_NAMES: Array[String] = ["NONE", "AUTO", "CPU", "GPU"]

@export_category("Internal")
@export var nav_map: NavMap = null
@export var cpu_solver: PackedScene
@export var gpu_solver: PackedScene
@export var monitor: PerformanceMonitor


func _ready() -> void:
	change_solver(default_solver)


func change_solver(type: SolverTypeEnum) -> void:
	var selection_cause := ""
	if type < 0 or type >= SolverTypeEnum.size():
		CustomLogger.log_error("solver type value out of bounds")
		return
	if type == SolverTypeEnum.NONE:
		if current_solver != null:
			if monitor != null:
				if not monitor.has_method("RemoveTarget"):
					push_error("PerformanceMonitor has no method named RemoveTarget")
					return
				monitor.call_deferred("RemoveTarget", current_solver)
			current_solver.queue_free()
			current_solver = null
		return
	if type != SolverTypeEnum.AUTO:
		selection_cause = "forced"
		set_solver(get_solver(type), selection_cause)
		return

	var selected_solver := SolverTypeEnum.CPU

	var adapter := RenderingServer.get_video_adapter_type()
	match adapter:
		RenderingDevice.DeviceType.DEVICE_TYPE_DISCRETE_GPU:
			selected_solver = SolverTypeEnum.GPU
		RenderingDevice.DeviceType.DEVICE_TYPE_INTEGRATED_GPU:
			selected_solver = SolverTypeEnum.CPU
			selection_cause = "No discrete GPU found. Using CPU solver"
		RenderingDevice.DeviceType.DEVICE_TYPE_CPU:
			selected_solver = SolverTypeEnum.CPU
			selection_cause = "No discrete GPU found. Using CPU solver"
		_:
			selected_solver = SolverTypeEnum.CPU
			selection_cause = "unsupported video adapter: %s" % adapter

	var method := RenderingServer.get_current_rendering_method()
	if method == "gl_compatibility":
		selected_solver = SolverTypeEnum.CPU
		selection_cause = "incompatible rendering method: %s" % method

	set_solver(get_solver(selected_solver), selection_cause)
	solver_type = type


func get_solver(type: SolverTypeEnum) -> PackedScene:
	match type:
		SolverTypeEnum.CPU:
			return cpu_solver
		SolverTypeEnum.GPU:
			return gpu_solver
		_:
			return cpu_solver


func set_solver(type: PackedScene, cause: String):
	if current_solver != null:
		monitor.call_deferred("RemoveTarget", current_solver)

	var node := type.instantiate() as AbstractEnemyCollisionSolver
	node.NavMap = nav_map
	add_child(node)
	current_solver = node
	CustomLogger.log_debug("using %s: %s" % [node.get_script().get_global_name(), cause])
	if monitor != null:
		if not monitor.has_method("AddTarget"):
			push_error("PerformanceMonitor has no method named AddTarget")
			return
		monitor.call_deferred("AddTarget", node)
