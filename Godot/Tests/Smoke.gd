extends SceneTree

# Exercise the configured entry scene and shut down explicitly. Godot 4.2's
# --quit-after exits with code 1, which cannot distinguish success from failure.
func _initialize() -> void:
	call_deferred("_run")

func _run() -> void:
	# Source hosts can hide filename-case errors that break the embedded pack.
	if load("res://Sprites/Items/Research/bandaid.png") == null:
		push_error("Canonical Bandaid research image failed to load")
		quit(1)
		return
	var scene := load(ProjectSettings.get_setting("application/run/main_scene")) as PackedScene
	var game := scene.instantiate()
	root.add_child(game)
	current_scene = game
	for frame in range(120):
		await process_frame
	print("SMOKE OK")
	quit(0)
