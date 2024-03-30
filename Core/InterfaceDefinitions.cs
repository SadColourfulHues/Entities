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

/// <summary>
/// An interface describing an object for processing a status effect
/// </summary>
public interface IStatus
{
	void StatusSetDuration(float duration);
	bool StatusIsActive(IEntity entity);
	void StatusAdded(IEntity entity);
	void StatusRemoved(IEntity entity);
	void StatusTick(IEntity entity, float delta);

	string StatusGetIdentifier();
	float StatusGetRemainingDuration();
}