extends KinematicBody2D

class_name Entity


onready var sprite:=$sprite

export var speed:=64


var look_direction:Vector2
var velocity_direction:Vector2 setget set_velocity_direction


func _physics_process(delta):
	move_and_slide(speed*velocity_direction.normalized())

func set_velocity_direction(new_velocity_direction:Vector2):
	
	velocity_direction=new_velocity_direction
	update_look_direction()

func update_look_direction():
	if velocity_direction.x!=0:
		if velocity_direction.y==0:
			look_direction.x=velocity_direction.x
	else:
		look_direction.x=0
	if velocity_direction.y!=0:
		if velocity_direction.x==0:
			look_direction.y=velocity_direction.y
	else:
		look_direction.y=0
	animation()

func animation():
	pass
