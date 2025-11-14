extends Area2D

onready var sprite=$sprite

func _ready():
	match rotation_degrees:
		180.0:
			sprite.normal_map=load("res://asset/map/door/door_spritesheet_normalmap_back.png")
		90.0:
			sprite.normal_map=load("res://asset/map/door/door_spritesheet_normalmap_right.png")
		270.0:
			sprite.normal_map=load("res://asset/map/door/door_spritesheet_normalmap_left.png")
		_:
			sprite.normal_map=load("res://asset/map/door/door_spritesheet_normalmap_front.png")
