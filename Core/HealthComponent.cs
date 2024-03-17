using Godot;
using System.Diagnostics;

namespace SadChromaLib.Specialisations.Entities;

/// <summary>
/// A component that manages an entity's life state
/// </summary>
[GlobalClass]
public partial class HealthComponent : Node
{
	[Signal]
	public delegate void HealthChangedEventHandler(float healthFac);

	[Signal]
	public delegate void DeathEventHandler();

	[Export]
	public float MaxHealth = 100.0f;

	[Export]
	public bool IsInvulnerable;

	private HealthController _controller;

	public override void _EnterTree()
	{
		_controller = new(MaxHealth, IsInvulnerable);

		_controller.OnHealthChanged = OnHealthChangedDelegate;
		_controller.OnDeath = OnDeathDelegate;
	}

	/// <summary>
	/// Returns the reference of this component's internal controller.
	/// </summary>
	/// <returns></returns>
	public HealthController GetController() {
		return _controller;
	}

	#region Convenience Forward Methods

	/// <summary>
	/// Called when the regular 'TakeDamage' is triggered,
	/// override this to perform custom damage calculation before applying
	/// the damage total.
	/// </summary>
	/// <param name="damage">The total base damage</param>
	/// <param name="modifier">The damage multiplier.</param>
	/// <returns></returns>
	public virtual float CalculateDamage(float damage, float modifier = 1.0f) {
		return damage * modifier;
	}

	/// <summary>
	/// Applies a set amount of damage to the target entity.
	/// Overridden HealthComponent controllers can add custom damage calculation to be applied
	/// when using this function over 'TakePureDamage'.
	/// </summary>
	/// <param name="damage">The amount of damage to inflict the entity. (Must be positive.)</param>
	/// <param name="modifier">The base damage modifier.</param>
	/// <returns></returns>
	public float TakeDamage(float damage, float modifier = 1.0f) {
		_controller.OnCalculateDamage = (float dmg) => CalculateDamage(dmg, modifier);
		return _controller.TakeDamage(damage);
	}

	/// <summary>
	/// Reduces the entity's health by a fixed amount.
	/// If called on an overridden health component, its custom damage calculation
	/// will never be applied when using this method.
	/// </summary>
	/// <param name="damage">The amount of damage to inflict on the entity.</param>
	public float TakeFixedDamage(float damage) {
		return _controller.TakeFixedDamage(damage);
	}

	/// <summary>
	/// Restores the entity's health by a specified amount.
	/// </summary>
	/// <param name="amount">The amount of health to restore.</param>
	public float RestoreHealth(float amount) {
		return _controller.RestoreHealth(amount);
	}

	/// <summary>
	/// Returns whether or not the entity is alive.
	/// </summary>
	/// <returns></returns>
	public bool IsAlive() {
		return _controller.IsAlive();
	}

	/// <summary>
	/// Returns the entity's remaining health.
	/// </summary>
	/// <returns></returns>
	public float GetHealth() {
		return _controller.GetHealth();
	}

	/// <summary>
	/// Returns the entity's remaining health as a fraction (0.0 - 1.0).
	/// </summary>
	/// <returns></returns>
	public float GetHealthAsFac() {
		return _controller.GetHealthAsFac();
	}

	#endregion

	#region Event to Signal

	private void OnHealthChangedDelegate(float newHealth) {
		EmitSignal(SignalName.HealthChanged, newHealth);
	}

	private void OnDeathDelegate() {
		EmitSignal(SignalName.Death);
	}

	#endregion
}
