using Godot;

namespace SadChromaLib.Specialisations.Entities;

/// <summary>
/// An interface that describes an object that implements common entity methods.
/// </summary>
public interface IEntity
{
	Node EntityGetReference();
	public float EntityTakeDamage(float damage, float multiplier);
	public void EntityTakeFixedDamage(float damage);
	public float EntityRestoreHealth(float amount);
}