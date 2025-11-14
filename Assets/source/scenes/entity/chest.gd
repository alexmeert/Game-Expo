extends StaticBody2D

var opened := false


func _on_area_2d_body_entered(body):
	if body is Player and !opened:
		opened=true
		$animation_player.play("open")
		var tmp=preload("res://scenes/entity/key.tscn").instance()
		tmp.position.y-=4
		add_child(tmp)
		
