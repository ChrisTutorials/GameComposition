## Generic service scope for GDScript (5.1 backlayer).
##
## Caches scoped services per-scope.
class_name GCServiceScope
extends RefCounted

var _registry: Variant
var _scoped: Dictionary = {}
var _disposed: bool = false

func _init(registry: Variant) -> void:
	setup(registry)

func setup(registry: Variant) -> void:
	_registry = registry

func get_service(key: Variant) -> Variant:
	if _disposed:
		push_error("GCServiceScope is disposed")
		assert(false)
	if _scoped.has(key):
		return _scoped[key]
	var created := _registry._create_scoped_service(key)
	if created == null:
		push_error("Scoped service not registered: %s" % [key])
		assert(false)
	_scoped[key] = created
	return created

func dispose() -> void:
	if _disposed:
		return
	_scoped.clear()
	_disposed = true
