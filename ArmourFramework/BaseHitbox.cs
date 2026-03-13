using Godot;
using System;

	
public partial class BaseHitbox : RigidBody3D
{
	//how much damage this hitbox does on hit
	[Export]
	public int damage = 0;
	//how much this object can penetrate, higher values mean more penetration
	[Export]
	int hitType = 0;
	[Export]
	public float speed = 15;
	[Export]
	int projectileLife = 3;

	//stores collision data for use in deflection
	KinematicCollision3D collision = new();

	//setup
	public override void _Ready()
	{
		BodyEntered += (body) => GD.Print("collided");
		BodyEntered += body => OnHit((Node3D)body);
		BodyEntered += (body) => GD.Print("collided2");
		Timer lifetime = new();
		AddChild(lifetime);
		lifetime.WaitTime = projectileLife;
		lifetime.Timeout += () => QueueFree();
		lifetime.Start();
		ApplyCentralImpulse(GlobalBasis * Vector3.Forward * speed);
	}

	//physcics handling
	public override void _PhysicsProcess(double delta)
	{
		collision = MoveAndCollide(LinearVelocity * (float)delta);
	}

	//how to behave on colliding with an object
	//first checks if it should bounce
	//then if not makes sure the object can be damaged
	//then calls the object's damage method
	public virtual bool OnHit(Node3D body)
	{
		double relativeAngle = double.RadiansToDegrees(Basis.Z.AngleTo(collision.GetNormal()));

		if (ShouldBounce(relativeAngle))
		{
			Deflect();
			GD.Print("deflected");
			return false;
		} 
		else if (!body.HasMethod("WasHit"))
		{
			GD.Print("invalid target");
			QueueFree();
			return false;
		}
		else 
		{
			bool didHit = (bool)body.Call("WasHit", hitType, damage);
			GD.Print("successful hit? " + didHit);
			if (didHit && ShouldPenetrate((BaseHurtbox)body))
			{
				Penetrate();
				GD.Print("penetrated");
			}else
			{
				GD.Print("stopped");
				QueueFree();
			}
			
			return didHit;
		}
	}

	//checks the relative angle to the colliding object's normal vector, if it is greater than 60 degress, the projectile bounces
	private bool ShouldBounce (double relativeAngle)
	{
		GD.Print(relativeAngle);
		if (Math.Abs(relativeAngle) <= 40)
		{
			return false;
		}
		return true;
	}

	//bounces the projectile along it's new deflection vector
	private void Deflect ()
	{
		Vector3 reflect = collision.GetRemainder().Bounce(collision.GetNormal());
		LinearVelocity = LinearVelocity.Bounce(collision.GetNormal());
		LookAt(LinearVelocity);
		MoveAndCollide(reflect);
	}

	private bool ShouldPenetrate (BaseHurtbox body3D)
	{
		
		bool hasSufficientForce = collision.GetRemainder().Length() > body3D.GetThickness(collision.GetPosition(), collision.GetRemainder(), GlobalRotation);
		GD.Print("remaining force = " + collision.GetRemainder().Length());
		GD.Print("has sufficient force? " + hasSufficientForce);
		bool hasDamageLeft = damage > body3D.currentHealth;
		GD.Print("has sufficient damage? " + hasDamageLeft);
		if ( hasSufficientForce && hasDamageLeft)
		{
			damage -= body3D.currentHealth/2;
			GD.Print("should have penetrated");
			return true;
		}
		return false;
	}

	private void Penetrate ()
	{
		SetCollisionMaskValue(1, false);
	}
}
