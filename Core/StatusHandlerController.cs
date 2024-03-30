using System;
using System.Diagnostics;

namespace SadChromaLib.Specialisations.Entities;

/// <summary>
/// A controller object that can be used to apply status effect on an entity
/// </summary>
public sealed partial class StatusHandlerController
{
	public delegate void StatusAddedDelegate(string statusId);

	public delegate void StatusTickDelegate(string statusId, float duration);

	public delegate void StatusRemovedDelegate(string statusId);

	public StatusAddedDelegate OnStatusAdded = null;
	public StatusTickDelegate OnStatusTick = null;
	public StatusRemovedDelegate OnStatusRemoved = null;

	public readonly int MaxStatuses = 8;
	private readonly StatusRegistry _registry;

	private readonly IEntity _entity;
	private readonly IStatus[] _statuses;
	private bool _isLocked;

    public StatusHandlerController(IEntity entityTarget, StatusRegistry registry, int maxStatuses = 8)
    {
		Debug.Assert(
			condition: entityTarget is not null,
			message: "StatusHandlerComponent must be attached to a valid entity object."
		);

		MaxStatuses = maxStatuses;
        _statuses = new IStatus[MaxStatuses];

		_entity = entityTarget;
		_registry = registry;
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

			string statusId = _statuses[i].StatusGetIdentifier();

			if (!_statuses[i].StatusIsActive(_entity)) {
				_statuses[i].StatusRemoved(_entity);
				_statuses[i] = null;

				OnStatusRemoved?.Invoke(statusId);
				continue;
			}

			_statuses[i].StatusTick(_entity, delta);

			OnStatusTick?.Invoke(
				statusId,
				_statuses[i].StatusGetRemainingDuration()
			);
		}
	}

	/// <summary>
	/// Returns whether or not the entity has a status effect.
	/// </summary>
	/// <param name="statusId">The ID of the status effect.</param>
	/// <returns></returns>
	public bool HasStatus(string statusId)
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
	/// Sets whether or not new status effects can be added to the entity.
	/// </summary>
	/// <param name="locked"></param>
	public void SetLockState(bool locked) {
		_isLocked = locked;
	}

	/// <summary>
	/// Directly adds a status into the status controller.
	/// </summary>
	/// <param name="status">The status effect processor to add.</param>
	public void AddStatus(IStatus status)
	{
		if (_isLocked)
			return;

		string statusId = status.StatusGetIdentifier();
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
		_statuses[slot.Value].StatusAdded(_entity);

		OnStatusAdded?.Invoke(statusId);
	}

	/// <summary>
	/// Adds a status effect processor by name through a registry object
	/// </summary>
	/// <param name="id">The ID of the status effect</param>
	/// <param name="duration">How long the status effect should last.</param>
	public void AddStatus(string id, float duration) {
		AddStatus(_registry.CreateStatus(id, duration));
	}

	/// <summary>
	/// Removes a status effect from the entity.
	/// </summary>
	/// <param name="statusId">The ID of the status effect.</param>
	public void RemoveStatus(string statusId)
	{
		ReadOnlySpan<IStatus> statuses = _statuses;

		for (int i = 0; i < MaxStatuses; ++ i) {
			if (statuses[i] == null)
				continue;

			string id = statuses[i].StatusGetIdentifier();

			if (id != statusId)
				continue;

			_statuses[i].StatusRemoved(_entity);
			_statuses[i] = null;

			OnStatusRemoved?.Invoke(id);
			return;
		}
	}

	/// <summary>
	/// Removes all status effects from the entity.
	/// </summary>
	public void ClearStatuses()
	{
		for (int i = 0; i < MaxStatuses; ++ i) {
			_statuses[i].StatusRemoved(_entity);
			_statuses[i] = null;

			OnStatusRemoved?.Invoke(_statuses[i].StatusGetIdentifier());
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