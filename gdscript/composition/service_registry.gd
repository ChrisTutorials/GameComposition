## Generic service registry for GDScript (5.1 backlayer).
##
## Supports:
## - Singleton registrations
## - Factory registrations (transient)
## - Scoped registrations (resolved via ServiceScope)
class_name GCServiceRegistry
extends RefCounted

var _singletons: Dictionary = {}
var _factories: Dictionary = {}
var _scoped_factories: Dictionary = {}
var _disposed: bool = false

func _throw_if_disposed() -> void:
if _disposed:
push_error("GCServiceRegistry is disposed")
assert(false)

func register_singleton(key: Variant, instance: Variant) -> void:
_throw_if_disposed()
if _singletons.has(key):
push_error("Singleton service already registered: %s" % [key])
assert(false)
_singletons[key] = instance

func register_factory(key: Variant, factory: Callable) -> void:
_throw_if_disposed()
if _factories.has(key) or _scoped_factories.has(key):
push_error("Factory service already registered: %s" % [key])
assert(false)
_factories[key] = factory

func register_scoped(key: Variant, factory: Callable) -> void:
_throw_if_disposed()
if _factories.has(key) or _scoped_factories.has(key):
push_error("Scoped service already registered: %s" % [key])
assert(false)
_scoped_factories[key] = factory

func is_registered(key: Variant) -> bool:
_throw_if_disposed()
return _singletons.has(key) or _factories.has(key) or _scoped_factories.has(key)

func try_get_service(key: Variant) -> Variant:
_throw_if_disposed()
if _singletons.has(key):
return _singletons[key]
if _factories.has(key):
return _factories[key].call()
if _scoped_factories.has(key):
push_error("Scoped service requested from registry without a scope: %s" % [key])
return null
return null

func get_service(key: Variant) -> Variant:
var value := try_get_service(key)
if value == null:
push_error("Service not registered: %s" % [key])
assert(false)
return value

func create_scope() -> GCServiceScope:
_throw_if_disposed()
return GCServiceScope.new(self)

func _create_scoped_service(key: Variant) -> Variant:
_throw_if_disposed()
if _scoped_factories.has(key):
return _scoped_factories[key].call()
if _factories.has(key):
return _factories[key].call()
return null

func clear() -> void:
_throw_if_disposed()
_singletons.clear()
_factories.clear()
_scoped_factories.clear()

func dispose() -> void:
if _disposed:
return
_singletons.clear()
_factories.clear()
_scoped_factories.clear()
_disposed = true
