using Godot;
using System;

public partial class TestCharacter : CharacterBody3D
{
	PackedScene projectile = ResourceLoader.Load<PackedScene>("res://test_projectile.tscn");
	[Export]
	public int Speed { get; set; } = 14;
	[Export]
	public float rotationSpeed = 1f;

	private Vector3 _targetVelocity = Vector3.Zero;
	public override void _PhysicsProcess(double delta)
	{
		var direction = Vector3.Zero;

		if (Input.IsActionPressed("move_right"))
		{
			direction.X += 1.0f;
		}
		if (Input.IsActionPressed("move_left"))
		{
			direction.X -= 1.0f;
		}
		if (Input.IsActionPressed("move_back"))
		{
			direction.Z += 1.0f;
		}
		if (Input.IsActionPressed("move_forward"))
		{
			direction.Z -= 1.0f;
		}
		if (Input.IsActionPressed("move_up"))
		{
			direction.Y += 1.0f;
		}
		if (Input.IsActionPressed("move_down"))
		{
			direction.Y -= 1.0f;
		}
		if (Input.IsActionJustPressed("fire"))
		{
			FireProjectile();
		}

		if (direction != Vector3.Zero)
		{
			direction = direction.Normalized();
		}

		_targetVelocity.X = direction.X * Speed;
		_targetVelocity.Z = direction.Z * Speed;
		_targetVelocity.Y = direction.Y * Speed;

		Velocity = _targetVelocity;
		MoveAndSlide();
	}

	private void FireProjectile()
	{
		BaseHitbox bullet = projectile.Instantiate<BaseHitbox>();
		AddChild(bullet);
		bullet.Position = GetNode<Node3D>("ProjectileOrigin").Position;
		bullet.GlobalRotation = GlobalRotation;
		bullet.Reparent(GetTree().Root);
	}

/*
	public override void _UnhandledInput(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotionEvent)
		{
			RotateY(mouseMotionEvent.Relative.X * rotationSpeed);
		}
	}
*/

}
