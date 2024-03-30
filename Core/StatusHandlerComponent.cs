using Godot;

namespace SadChromaLib.Specialisations.Entities;

/// <summary>
/// A component that handles status effect processing for an entity. (Must be attached to an object implementing IEntity.)
/// </summary>
[GlobalClass]
public sealed partial class StatusHandlerComponent : Node
{
	[Signal]
	public delegate void StatusAddedEventHandler(string statusId);

	[Signal]
	public delegate void StatusTickEventHandler(string statusId, float duration);

	[Signal]
	public delegate void StatusRemovedEventHandler(string statusId);

    [Export]
	private StatusRegistry _registryRef;

	[Export]
	private int _maxStatusCount = 8;

	private StatusHandlerController _controller;

    public override void _EnterTree()
	{
		Node parentRef = GetParentOrNull<Node>();

		if (!IsInstanceValid(parentRef) || parentRef is not IEntity entity) {
			GD.PrintErr("StatusHandlerComponent: This node must be attached to an entity implementing IEntity.");
			SetProcess(false);
			return;
		}

		_controller = new(entity, _registryRef, _maxStatusCount);

		_controller.OnStatusAdded = OnStatusAddedDelegate;
		_controller.OnStatusTick = OnStatusTickDelegate;
		_controller.OnStatusRemoved = OnStatusRemovedDelegate;
	}

    public override void _Process(double delta) {
        _controller.Evaluate((float) delta);
    }

	/// <summary>
	/// Returns the reference of this component's internal controller.
	/// </summary>
	/// <returns></returns>
	public StatusHandlerController GetController() {
		return _controller;
	}

    #region Convenience Forward Methods

	public int MaxStatuses => _controller.MaxStatuses;

    /// <summary>
    /// Returns a reference of a status instance at the specified index
    /// Normally shouldn't be used other than for displaying status info.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public IStatus GetStatusRefAt(int index) {
		return _controller.GetStatusRefAt(index);
	}

	/// <summary>
	/// Sets whether or not new status effects can be added to the entity.
	/// </summary>
	/// <param name="locked"></param>
	public void SetLockState(bool locked) {
		_controller.SetLockState(locked);
	}

	/// <summary>
	/// Returns whether or not the entity has a status effect.
	/// </summary>
	/// <param name="statusId">The ID of the status effect.</param>
	/// <returns></returns>
	public bool HasStatus(string statusId) {
		return _controller.HasStatus(statusId);
	}

	/// <summary>
	/// Adds a status effect processor by name through a registry object
	/// </summary>
	/// <param name="id">The ID of the status effect</param>
	/// <param name="duration">How long the status effect should last.</param>
	public void AddStatus(string id, float duration) {
		_controller.AddStatus(id, duration);
	}

	/// <summary>
	/// Removes a status effect from the entity.
	/// </summary>
	/// <param name="statusId">The ID of the status effect.</param>
	public void RemoveStatus(string statusId) {
		_controller.RemoveStatus(statusId);
	}

	/// <summary>
	/// Removes all status effects from the entity.
	/// </summary>
	public void ClearStatuses() {
		_controller.ClearStatuses();
	}

	#endregion

	#region Event to Signal

	private void OnStatusAddedDelegate(string statusId) {
		EmitSignal(SignalName.StatusAdded, statusId);
	}

	private void OnStatusTickDelegate(string statusId, float duration) {
		EmitSignal(SignalName.StatusTick, statusId, duration);
	}

	private void OnStatusRemovedDelegate(string statusId) {
		EmitSignal(SignalName.StatusRemoved, statusId);
	}

	#endregion
}
