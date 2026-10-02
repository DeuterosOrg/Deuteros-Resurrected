extends RefCounted

# Godot 4.2.2 debug builds keep the last parsed events after C# teardown.
# On a later frame, replace them with an event that never gets a C# wrapper.
# FlushBufferedEvents alone does not clear the separate per-frame cache.
func release_managed_events(viewport: Viewport) -> void:
	# Paused overlays can still receive input; prevent the replacement event
	# from reaching any managed callback or activating a control during exit.
	viewport.propagate_call("set_process_input", [false])
	viewport.propagate_call("set_process_unhandled_input", [false])
	viewport.propagate_call("set_process_unhandled_key_input", [false])
	viewport.propagate_call("set_process_shortcut_input", [false])
	viewport.gui_disable_input = true
	Input.parse_input_event(InputEventMouseMotion.new())
	Input.flush_buffered_events()
