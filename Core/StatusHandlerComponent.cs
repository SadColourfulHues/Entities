using Godot;

using System;
using System.Diagnostics;

namespace SadChromaLib.Specialisations.Entities;

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

	StringName StatusGetIdentifier();
	float StatusGetRemainingDuration();
}

/// <summary>
/// A component that handles status effect processing for an entity. (Must be attached to an object implementing IEntity.)
/// </summary>
[GlobalClass]
public sealed partial class StatusHandlerComponent : Node
{
	[Signal]
	public delegate void StatusAddedEventHandler(StringName statusId);

	[Signal]
	public delegate void StatusTickEventHandler(StringName statusId, float duration);

	[Signal]
	public delegate void StatusRemovedEventHandler(StringName statusId);

	[Export]
	private StatusRegistry _registryRef;

	[Export]
	public int MaxStatuses = 8;

	private IStatus[] _statuses;
	private IEntity _owningEntityRef;

	private bool _isLocked;

    public override void _EnterTree()
    {
        _statuses = new IStatus[MaxStatuses];
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		Node ownerRef = GetOwnerOrNull<Node>();

		Debug.Assert(
			condition: ownerRef != null,
			message: "StatusHandlerComponent must be attached to a valid entity object."
		);

		_owningEntityRef = (IEntity) ownerRef;
	}

	#region Main Functions

	/// <summary>
	/// Returns a reference of a status instance at the specified index
	/// Normally shouldn't be used other than for displaying status info.
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public IStatus GetStatusRefAt(int index)
	{
		if (index < 0 || index >= MaxStatuses)
			return null;

		return _statuses[index];
	}

	/// <summary>
	/// Evaluates the current status processing state.
	/// </summary>
	/// <param name="delta">Delta time.</param>
	public void Evaluate(float delta)
	{
		for (int i = 0; i < MaxStatuses; ++ i) {
			if (_statuses[i] == null)
				continue;

			StringName statusId = _statuses[i].StatusGetIdentifier();

			if (!_statuses[i].StatusIsActive(_owningEntityRef)) {
				_statuses[i].StatusRemoved(_owningEntityRef);
				_statuses[i] = null;
				EmitSignal(SignalName.StatusRemoved, statusId);

				continue;
			}

			_statuses[i].StatusTick(_owningEntityRef, delta);

			EmitSignal(
				SignalName.StatusTick,
				statusId,
				_statuses[i].StatusGetRemainingDuration()
			);
		}
	}

	/// <summary>
	/// Sets whether or not new status effects can be added to the entity.
	/// </summary>
	/// <param name="locked"></param>
	public void SetLockState(bool locked)
	{
		_isLocked = locked;
	}

	/// <summary>
	/// Adds a status effect processor by name through a registry object
	/// </summary>
	/// <param name="id">The ID of the status effect</param>
	/// <param name="duration">How long the status effect should last.</param>
	public void AddStatus(StringName id, float duration)
	{
		AddStatus(
			_registryRef.CreateStatus(id, duration)
		);
	}

	/// <summary>
	/// Removes a status effect from the entity.
	/// </summary>
	/// <param name="statusId">The ID of the status effect.</param>
	public void RemoveStatus(StringName statusId)
	{
		ReadOnlySpan<IStatus> statuses = _statuses;

		for (int i = 0; i < MaxStatuses; ++ i) {
			if (statuses[i] == null)
				continue;

			StringName id = statuses[i].StatusGetIdentifier();

			if (id != statusId)
				continue;

			_statuses[i].StatusRemoved(_owningEntityRef);
			_statuses[i] = null;

			EmitSignal(SignalName.StatusRemoved, id);

			return;
		}
	}

	/// <summary>
	/// Returns whether or not the entity has a status effect.
	/// </summary>
	/// <param name="statusId">The ID of the status effect.</param>
	/// <returns></returns>
	public bool HasStatus(StringName statusId)
	{
		ReadOnlySpan<IStatus> statuses = _statuses;

		for (int i = 0; i < MaxStatuses; ++ i) {
			if (statuses[i] == null ||
				statuses[i].StatusGetIdentifier() != statusId)
			{
				continue;
			}

			return true;
		}

		return false;
	}

	/// <summary>
	/// Directly adds a status into the status controller.
	/// </summary>
	/// <param name="status">The status effect processor to add.</param>
	public void AddStatus(IStatus status)
	{
		if (_isLocked)
			return;

		StringName statusId = status.StatusGetIdentifier();
		int? slot = null;

		// Overwrite status if the new duration is greater
		ReadOnlySpan<IStatus> statuses = _statuses;

		for (int i = 0; i < MaxStatuses; ++ i) {
			if (statuses[i] == null) {
				slot ??= i;
				continue;
			}

			if (statuses[i].StatusGetIdentifier() != statusId)
				continue;

			if (statuses[i].StatusGetRemainingDuration() >= status.StatusGetRemainingDuration())
				return;

			_statuses[i] = status;
			return;
		}

		if (slot == null)
			return;

		_statuses[slot.Value] = status;
		_statuses[slot.Value].StatusAdded(_owningEntityRef);
		EmitSignal(SignalName.StatusAdded, statusId);
	}

	/// <summary>
	/// Returns whether or not the entity has a status effect.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <returns></returns>
	public bool HasStatus<T>() where T: IStatus
	{
		ReadOnlySpan<IStatus> statuses = _statuses;

		for (int i = 0; i < MaxStatuses; ++ i) {
			if (statuses[i] == null ||
				statuses[i] is not T)
			{
				continue;
			}

			return true;
		}

		return false;
	}

	/// <summary>
	/// Removes a status effect from the entity.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public void RemoveStatus<T>() where T: IStatus
	{
		ReadOnlySpan<IStatus> statuses = _statuses;

		for (int i = 0; i < MaxStatuses; ++ i) {
			if (statuses[i] == null ||
				statuses[i] is not T)
			{
				continue;
			}

			StringName identifier = statuses[i].StatusGetIdentifier();

			_statuses[i].StatusRemoved(_owningEntityRef);
			_statuses[i] = null;

			EmitSignal(SignalName.StatusRemoved, identifier);

			return;
		}
	}

	/// <summary>
	/// Removes all status effects from the entity.
	/// </summary>
	public void ClearStatuses()
	{
		for (int i = 0; i < MaxStatuses; ++ i) {
			_statuses[i].StatusRemoved(_owningEntityRef);
			_statuses[i] = null;

			EmitSignal(SignalName.StatusRemoved, _statuses[i].StatusGetIdentifier());
		}
	}

	#endregion

	#region Helpers

	private int FindOpenSlot()
	{
		for (int i = 0; i < MaxStatuses; ++ i) {
			if (_statuses[i] != null)
				continue;

			return i;
		}

		return -1;
	}

	private void Reset()
	{
		for (int i = 0; i < MaxStatuses; ++ i) {
			_statuses[i] = null;
		}
	}

	#endregion
}
