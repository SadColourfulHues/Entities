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

	private float _health;

	public override void _Ready()
	{
		_health = MaxHealth;
	}

	#region Main Functions

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
	public float TakeDamage(float damage, float modifier = 1.0f)
	{
		if (IsInvulnerable || _health <= 0)
			return 0.0f;

		Debug.Assert(
			condition: damage >= 0.0f,
			message: "TakeDamage: attempted to use a negative value in an unsigned operation."
		);

		float totalDamage = CalculateDamage(damage, modifier);
		_health = Mathf.Max(0.0f, _health - totalDamage);

		FireOnDamagedEvents();
		return totalDamage;
	}

	/// <summary>
	/// Reduces the entity's health by a fixed amount.
	/// If called on an overridden health component, its custom damage calculation
	/// will never be applied when using this method.
	/// </summary>
	/// <param name="damage">The amount of damage to inflict on the entity.</param>
	public void TakePureDamage(float damage)
	{
		if (IsInvulnerable)
			return;

		Debug.Assert(
			condition: damage >= 0.0f,
			message: "TakeDamage: attempted to use a negative value in an unsigned operation."
		);

		_health = Mathf.Max(0.0f, _health - damage);

		FireOnDamagedEvents();
	}

	/// <summary>
	/// Changes the entity's max health cap.
	/// </summary>
	/// <param name="maxHealth"></param>
	public void SetMaxHealth(float maxHealth)
	{
		float currentHealthFac = GetHealthAsFac();
		MaxHealth = maxHealth;

		_health = maxHealth * currentHealthFac;
	}

	private void FireOnDamagedEvents()
	{
		if (_health > 0.0f) {
			EmitSignal(SignalName.HealthChanged, _health);
			return;
		}

		EmitSignal(SignalName.Death);
	}

	#endregion

	#region Health State Getters

	/// <summary>
	/// Restores the entity's health by a specified amount.
	/// </summary>
	/// <param name="amount">The amount of health to restore.</param>
	/// <param name="force">If set to true, the controller will bypass the must-be-above-zero health rule.</param>
	public void RestoreHealth(float amount, bool force = false)
	{
		if (!force && _health <= 0.0f)
			return;

		Debug.Assert(
			condition: amount >= 0.0f,
			message: "RestoreHealth: attempted to use a negative value in an unsigned operation."
		);

		_health = Mathf.Min(MaxHealth, _health + amount);
		EmitSignal(SignalName.HealthChanged, GetHealth());
	}

	/// <summary>
	/// Returns whether or not the entity is alive.
	/// </summary>
	/// <returns></returns>
	public bool IsAlive() {
		return _health > 0.001f;
	}

	/// <summary>
	/// Returns the entity's remaining health.
	/// </summary>
	/// <returns></returns>
	public float GetHealth() {
		return _health;
	}

	/// <summary>
	/// Returns the entity's remaining health as a fraction (0.0 - 1.0).
	/// </summary>
	/// <returns></returns>
	public float GetHealthAsFac() {
		return _health / MaxHealth;
	}

	#endregion
}
