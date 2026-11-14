@tool
extends EditorPlugin

func _enter_tree() -> void:
	if OS.get_environment("DEUTEROS_IMPORT_ONLY") == "1":
		_finish_import.call_deferred()

func _finish_import() -> void:
	var filesystem := EditorInterface.get_resource_filesystem()
	await get_tree().process_frame
	while filesystem.is_scanning():
		await get_tree().process_frame
	await get_tree().process_frame
	print("IMPORT OK")
	get_tree().quit(0)
