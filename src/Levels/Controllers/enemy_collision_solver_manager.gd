class_name EnemyCollisionSolverManager
extends Node

@export var solver_type: SolverTypeEnum = SolverTypeEnum.AUTO:
	get:
		return solver_type
	set(value):
		solver_type = value
		call_deferred("change_solver")
var current_solver: AbstractEnemyCollisionSolver = null
var selection_cause: String = ""

@export var nav_map: NavMap = null

enum SolverTypeEnum { AUTO, NONE, CPU, GPU }

@export_group("Internal")
@export var cpu_solver: PackedScene
@export var gpu_solver: PackedScene
@export var monitor: PerformanceMonitor

func _ready() -> void:
	change_solver()

func change_solver() -> void:
	if solver_type == SolverTypeEnum.NONE:
		return
	if solver_type != SolverTypeEnum.AUTO:
		set_solver(get_solver(solver_type))
		selection_cause = "forced"
		return
		
	var selected_solver = SolverTypeEnum.CPU
	
	var adapter = RenderingServer.get_video_adapter_type()
	match adapter:
		RenderingDevice.DeviceType.DEVICE_TYPE_DISCRETE_GPU:
			selected_solver = SolverTypeEnum.GPU
		_:
			selected_solver = SolverTypeEnum.CPU
			selection_cause = "unsupported video adapter: %s" % adapter
			
	var method =  RenderingServer.get_current_rendering_method()
	if method == "gl_compatibility":
		selected_solver = SolverTypeEnum.CPU
		selection_cause = "incompatible rendering method: %s" % method

	set_solver(get_solver(selected_solver))

func get_solver(type: SolverTypeEnum) -> PackedScene:
	match type:
		SolverTypeEnum.CPU:
			return cpu_solver
		SolverTypeEnum.GPU:
			return gpu_solver
		_:
			return cpu_solver

func set_solver(type: PackedScene):
	if current_solver != null:
		monitor.call_deferred("RemoveTarget", current_solver)
	
	var node = type.instantiate() as AbstractEnemyCollisionSolver
	node.NavMap = nav_map
	add_child(node)
	current_solver = node
	CustomLogger.log_debug("using %s: %s" % [node.name, selection_cause])
	if monitor != null:
		if not monitor.has_method("AddTarget"):
			push_error("PerformanceMonitor has no method named AddTarget")
			return
		monitor.call_deferred("AddTarget", node)
