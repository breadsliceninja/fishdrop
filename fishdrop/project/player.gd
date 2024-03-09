extends CharacterBody2D


const WALK_SPEED = 600.0
const WALK_ACC = 120.0
const AIR_SPEED = 900.0
const AIR_ACC = 160.0
const JUMP_VELOCITY = -980.0
const DROP_VELOCITY = 500.0
const DROP_INITIAL_VELOCITY = -180.0
const SLIDE_SPEED = 1000.0

# Get the gravity from the project settings to be synced with RigidBody nodes.
var gravity = ProjectSettings.get_setting("physics/2d/default_gravity")
var drop_gravity = gravity * 1.8

var is_sliding = false
var is_dropping = false
var was_dropping = false
var direction = 0
var facing = 1


func _physics_process(delta):
	# add gravity
	if not is_on_floor():
		if is_dropping:
			velocity.y += drop_gravity * delta
		else:
			velocity.y += gravity * delta
	else:
		was_dropping = true if is_dropping else false
		is_dropping = false

	direction = Input.get_axis("ui_left", "ui_right")
	if direction: facing = direction

	# start slide
	if (Input.is_action_just_pressed("ui_accept") or was_dropping) and is_on_floor() and velocity.x:
		is_sliding = true;
		velocity.x = SLIDE_SPEED * facing
	if not Input.is_action_pressed("ui_accept"):
		is_sliding = false;

	# jump
	if Input.is_action_just_released("ui_accept") and is_on_floor():
		velocity.y = JUMP_VELOCITY

	# start drop
	if Input.is_action_just_pressed("ui_accept") and not is_on_floor() and not is_dropping:
		is_dropping = true
		velocity.y += DROP_INITIAL_VELOCITY

	# basic movement
	if direction and not is_sliding and (not is_on_floor() or not Input.is_action_pressed("ui_accept")):
		velocity.x += direction * WALK_ACC if is_on_floor() else direction * AIR_ACC
	if not direction and not is_sliding:
		velocity.x = move_toward(velocity.x, 0, WALK_ACC) if is_on_floor() else move_toward(velocity.x, 0, AIR_ACC)

	# clamp to max speeds
	if is_on_floor() and not is_sliding and abs(velocity.x) > WALK_SPEED:
		velocity.x = sign(velocity.x) * WALK_SPEED
	if is_on_floor() and is_sliding and abs(velocity.x) > SLIDE_SPEED:
		velocity.x = sign(velocity.x) * SLIDE_SPEED
	if not is_on_floor() and abs(velocity.x) > AIR_SPEED:
		velocity.x -= sign(velocity.x) * AIR_ACC

	# TODO: turning while sliding (add delay?)

	move_and_slide()
