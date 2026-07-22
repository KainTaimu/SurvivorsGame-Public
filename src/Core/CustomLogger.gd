class_name CustomLogger
extends Object

static func log_info(... args: Array) -> void:
	send_log(print_rich, "[color=white][Info  :  %s] %s[/color]" % [get_caller_name(), construct_string(args)])

static func log_debug(... args: Array) -> void:
	send_log(print_rich, "[color=darkgray][Debug  :  %s] %s[/color]" % [get_caller_name(), construct_string(args)])

static func log_warning(... args: Array) -> void:
	send_log(print_rich, "[color=yellow][Warning  :  %s] %s[/color]" % [get_caller_name(), construct_string(args)])

static func log_error(... args: Array) -> void:
	push_error("[Error  :  %s] %s" % [get_caller_name(), construct_string(args)])

static func send_log(method: Callable, message: String) -> void:
	method.call(message)

static func construct_string(s: Array) -> String:
	var result = ""
	for x in s:
		result += "%s " % [x]
	return result

static func get_caller_name() -> String:
	var stack: Array[Dictionary] = get_stack()
	if stack.size() >= 2:
		return "%s:%s" % [get_caller_src(stack[2].get("source")), stack[2].get("line", "?")]
	return ""

static func get_caller_src(src: String) -> String:
	if src == "?":
		return "?"
	var fn = src.get_file()
	var comma = src.find(":")
	if comma == -1:
		return fn
	
	return fn.get_slice(":", 0)
