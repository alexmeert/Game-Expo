extends Node2D

var vp_size:Vector2

var c_current_pos:Vector2=Vector2(0,0)

onready var player=$y_sort/player
onready var camera=$camera
onready var tween=$tween

func _ready():
	vp_size=get_viewport_rect().size

func _process(delta):
	move_camera()

func move_camera():
	var p_pos:Vector2=(player.position/vp_size)
	var c_true_pos:Vector2=camera.position
	var c_pos:=Vector2(int(p_pos.x),int(p_pos.y))*vp_size
	if c_pos!=c_current_pos:
		tween.interpolate_property(camera,"position",c_true_pos,c_pos,0.3,Tween.TRANS_CUBIC,Tween.EASE_IN_OUT)
		tween.start()
		c_current_pos=c_pos
