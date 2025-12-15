## Engine-friendly user id for 5.1 GDScript backlayer.
##
## Intentionally simple: string-based ids.
class_name GCUserId
extends RefCounted

const EMPTY: String = ""

static func new_id() -> String:
  return "%s_%s" % [str(Time.get_ticks_usec()), str(randi())]

static func is_empty(value: String) -> bool:
  return value == EMPTY
