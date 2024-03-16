using Godot;

namespace SadChromaLib.Specialisations.Entities;

/// <summary>
/// A basic implementation of IStatus that enables the fast creation of status effect handlers.
/// </summary>
public abstract partial class BaseStatus : RefCounted, IStatus
{
	protected float _lifetime;

	protected BaseStatus()
	{
	}

	protected BaseStatus(float duration)
	{
		_lifetime = duration;
	}

    public abstract StringName StatusGetIdentifier();

	protected virtual void OnStatusAdded(IEntity entityRef) {}
	protected virtual void OnStatusRemoved(IEntity entityRef) {}
	protected virtual void OnStatusTick(IEntity entityRef, float delta) {}

	public void SetLifetime(float lifetime)
	{
		_lifetime = lifetime;
	}

	#region IStatus

	public float StatusGetRemainingDuration()
		=> _lifetime;

    public void StatusSetDuration(float duration)
    {
        _lifetime = duration;
    }

	public void StatusAdded(IEntity entityRef)
	{
		OnStatusAdded(entityRef);
	}

	public void StatusRemoved(IEntity entityRef)
	{
		OnStatusRemoved(entityRef);
	}

	public bool StatusIsActive(IEntity entityRef)
	{
		return _lifetime > 0.01f;
	}

	public void StatusTick(IEntity entity, float delta)
	{
		_lifetime = Mathf.Max(0.0f, _lifetime - delta);
		OnStatusTick(entity, delta);
	}

    #endregion
}