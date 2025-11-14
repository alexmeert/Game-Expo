extends StaticBody2D

export var id:=0

export var enabled:=false

onready var sprite=$sprite

onready var anim=$animation_player

func _ready():
	sprite.frame=2 if enabled else 0
	$light_2d.color.a = 255 if enabled else 0
	
func switch():
	anim.play("off" if enabled else "on")
	enabled = !enabled

func _on_area_2d_body_entered(body):
	if body is Player:
		switch()

func _on_area_2d_body_exited(body):
	if body is Player:
		#switch()
		pass
