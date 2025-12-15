class_name GCServiceRegistry
extends RefCounted

var _singletons: Dictionary = {}
var _factories: Dictionary = {}
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
    if _factories.has(key):
        push_error("Factory service already registered: %s" % [key])
        assert(false)
    _factories[key] = factory


func is_registered(key: Variant) -> bool:
    _throw_if_disposed()
    return _singletons.has(key) or _factories.has(key)


func try_get_service(key: Variant) -> Variant:
    _throw_if_disposed()
    if _singletons.has(key):
        return _singletons[key]
    if _factories.has(key):
        var factory_value: Variant = _factories[key]
        if factory_value is Callable:
            var factory: Callable = factory_value
            return factory.call()
        push_error("Factory is not a Callable for key: %s" % [key])
        assert(false)
    return null


func get_service(key: Variant) -> Variant:
    var value: Variant = try_get_service(key)
    if value == null:
        push_error("Service not registered: %s" % [key])
        assert(false)
    return value


func clear() -> void:
    _throw_if_disposed()
    _singletons.clear()
    _factories.clear()


func dispose() -> void:
    if _disposed:
        return
    _singletons.clear()
    _factories.clear()
    _disposed = true
