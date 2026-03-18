using Godot;
using System;

public partial class BaseHurtbox : AnimatableBody3D
{
	[Export]
	public int healthType = 0;
	[Export]
	public int healthPool = 1;
	[Export]
	public int currentHealth = 1;

	RayCast3D thickRay = new();

	public override void _Ready()
	{
		thickRay = GetNode<RayCast3D>("thickray");
	}

	//to be called by the hitter
	public virtual bool WasHit(int hitterType = 0, int hitterDamage = 0)
	{
		if (hitterType < healthType)
		{
			return false;
		}
		return true;
	}

	public double GetThickness (Vector3 origin, Vector3 target)
	{
		thickRay.Enabled = true;
		thickRay.GlobalPosition = origin + target;
		thickRay.TargetPosition = thickRay.ToLocal(origin);

		while (thickRay.GetCollider() != this)
		{
			if (thickRay.IsColliding())
			{
				thickRay.AddException((CollisionObject3D)thickRay.GetCollider());
			} else
			{
				thickRay.GlobalPosition += target;
				thickRay.TargetPosition = thickRay.ToLocal(origin);
			}
			thickRay.ForceRaycastUpdate();
		}

		GD.Print(thickRay.IsColliding());
		GD.Print(target);
		GD.Print(thickRay.GetCollisionPoint());
		double thickness = thickRay.GetCollisionPoint().DistanceTo(origin);
		GD.Print("distance to penetrate" + thickness);
		//thickRay.GlobalPosition = GlobalPosition;
		//thickRay.TargetPosition = Vector3.Down;
		//thickRay.Enabled = false;
		return thickness;
	}
}
