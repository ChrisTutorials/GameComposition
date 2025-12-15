## Settings provider pattern for 5.1 GDScript backlayer.
class_name GCSettingsProvider
extends RefCounted

func get_settings() -> Variant:
	push_error("GCSettingsProvider.get_settings is abstract")
	assert(false)
	return null


class GCStaticSettingsProvider:
	extends GCSettingsProvider
	var _settings: Variant

	func _init(settings: Variant) -> void:
		_settings = settings

	func get_settings() -> Variant:
		return _settings
