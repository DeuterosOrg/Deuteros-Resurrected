extends SceneTree

# Exercise the configured entry scene and shut down explicitly. Godot 4.2's
# --quit-after exits with code 1, which cannot distinguish success from failure.
func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	var scene := load(ProjectSettings.get_setting("application/run/main_scene")) as PackedScene
	var game := scene.instantiate()
	root.add_child(game)
	current_scene = game
	for frame in range(120):
		await process_frame
	print("SMOKE OK")
	quit(0)
