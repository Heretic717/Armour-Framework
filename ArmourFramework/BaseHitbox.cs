using Godot;
using System;


public partial class BaseHitbox : RayCast3D
{
	//how much damage this hitbox does on hit
	[Export]
	public int damage = 0;
	//how much this object can penetrate, higher values mean more penetration
	[Export]
	int hitType = 0;
	[Export]
	public float speed = 30;
	[Export]
	public float bounceAngle = 45;
	[Export]
	public float rotation = 5;
	[Export]
	int projectileLife = 2;

	private Node3D collisionBuffer;

	private Vector3 LinearVelocity { get; set; }
	private Vector3 AngularVelocity { get; set; }
	[Export]
	public float velocityDamp = .01f;
	[Export]
	public float rotationDamp = .01f;

	Timer lifetime = new();

	//setup
	public override void _Ready()
	{
		AddChild(lifetime);
		lifetime.Timeout += QueueFree;
		lifetime.Start(projectileLife);
		LinearVelocity = GlobalBasis * Vector3.Forward * speed;
		AngularVelocity = new Vector3(0, 0, rotation);
	}

	//physcics handling
	public override void _PhysicsProcess(double delta)
	{
		Move((float) delta);
		if(IsColliding())
		{
			OnHit((Node3D)GetCollider());
		}
	}

	//how to behave on colliding with an object
	//first checks if it should bounce
	//then if not makes sure the object can be damaged
	//then calls the object's damage method
	public virtual bool OnHit(Node3D body)
	{
		bool didHit = false;
			if (ShouldBounce())
			{
				GD.Print("deflected");
			}
			else if (!body.HasMethod("WasHit"))
			{
				GD.Print("invalid target");
				QueueFree();
			}
			else
			{
				didHit = (bool)body.Call("WasHit", hitType, damage);
				GD.Print("successful hit? " + didHit);
				if (!ShouldPenetrate((BaseHurtbox)body))
				{
					GD.Print("stopped");
					QueueFree();
				}

			}
			AddException((CollisionObject3D)body);
		return didHit;
	}

	//checks the relative angle to the colliding object's normal vector, if it is greater than x degress, the projectile bounces
	private bool ShouldBounce()
	{
		Vector3 collisionNormal = GetCollisionNormal();
		double relativeAngle = double.RadiansToDegrees(GlobalBasis.Z.AngleTo(collisionNormal));
		if (Math.Abs(relativeAngle) <= bounceAngle)
		{
			return false;
		}
		Deflect(collisionNormal);
		return true;
	}

	//bounces the projectile along it's new deflection vector
	private void Deflect(Vector3 normal)
	{
		LinearVelocity = LinearVelocity.Bounce(normal) * .5f;
		GD.Print("reflection vector is" + LinearVelocity);
		lifetime.Start(.5);
	}

	private bool ShouldPenetrate(BaseHurtbox body3D)
	{
		bool shouldPenetrate = hitType >= body3D.healthType;

		if (shouldPenetrate)
		{
			double thickness = body3D.GetThickness(GetCollisionPoint(), LinearVelocity.Normalized() * .5f);
			double force = LinearVelocity.Length() - (thickness * body3D.healthType / hitType);
			bool hasSufficientForce = force > thickness;
			bool hasDamageLeft = damage > body3D.currentHealth;

			shouldPenetrate = hasSufficientForce && hasDamageLeft;
			if (shouldPenetrate)
			{
				damage -= body3D.currentHealth / 2;
				Penetrate(LinearVelocity * (float)(force / LinearVelocity.Length()));
				shouldPenetrate = true;
			}

		}

		return shouldPenetrate;
	}

	private void Penetrate(Vector3 exitVelocity)
	{
		LinearVelocity = exitVelocity;
	}

	public virtual void Move(float delta)
	{
		Vector3 linear = LinearVelocity * delta;
		Vector3 angular = AngularVelocity * delta;
		GlobalPosition += linear;
		TargetPosition = linear;

		if (angular != Vector3.Zero)
		{
			Rotation += angular;
		}
		//LinearVelocity += Vector3.Down * (float)PhysicsServer3D.AreaGetParam(GetViewport().FindWorld3D().Space, PhysicsServer3D.AreaParameter.Gravity) * delta;
		LinearVelocity -= LinearVelocity * velocityDamp;
		AngularVelocity -= AngularVelocity * rotationDamp;
	}

}
