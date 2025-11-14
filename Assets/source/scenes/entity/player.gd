extends Entity

class_name Player

func _ready():
	speed=96

func _input(event):
	
	if event.is_action_pressed("move_down"):
		velocity_direction.y=1
	elif event.is_action_released("move_down"):
		if velocity_direction.y==1:
			velocity_direction.y=0
	if event.is_action_pressed("move_up"):
		velocity_direction.y=-1
	elif event.is_action_released("move_up"):
		if velocity_direction.y==-1:
			velocity_direction.y=0
	if event.is_action_pressed("move_left"):
		velocity_direction.x=-1
	elif event.is_action_released("move_left"):
		if velocity_direction.x==-1:
			velocity_direction.x=0
	if event.is_action_pressed("move_right"):
		velocity_direction.x=1
	elif event.is_action_released("move_right"):
		if velocity_direction.x==1:
			velocity_direction.x=0
	update_look_direction()
func animation():
#	match look_direction:
#		Vector2.UP:
#			sprite.play("idle_back")
#		Vector2.DOWN:
#			sprite.play("idle_front")
#		Vector2.LEFT:
#			sprite.play("idle_left")
#		Vector2.RIGHT:
#			sprite.play("idle_right")
	pass
