using Godot;
using System;

public partial class BaseHurtbox : StaticBody3D
{
	[Export]
	public int healthType = 0;
	[Export]
	public int healthPool = 1;
	[Export]
	public int currentHealth = 1;

	//to be called by the hitter
	public virtual bool WasHit(int hitterType = 0, int hitterDamage = 0)
	{
		if (hitterType < healthType)
		{
			return false;
		}
		return true;
	}

	public double GetThickness (Vector3 origin, Vector3 target, Vector3 rotation)
	{
		RayCast3D thickRay = new();
		AddChild(thickRay);
		thickRay.Position = origin;
		thickRay.CollideWithAreas = true;
		thickRay.CollideWithBodies = false;
		thickRay.ExcludeParent = false;
		thickRay.TargetPosition = target;
		thickRay.GlobalRotation = rotation;

		thickRay.ForceRaycastUpdate();

		double thickness = thickRay.GetCollisionPoint().DistanceTo(origin);
		GD.Print("distance to penetrate " + thickness);
		return thickness;
	}
}
