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
	public float Armour;

	[Export]
	public bool IsInvulnerable;

	private float _health;

	public float BonusArmour;

	public override void _Ready()
	{
		_health = MaxHealth;
	}

	#region Main Functions

	/// <summary>
	/// Reduces the entity's health by a specified amount. Total damage may be affected by its armour value.
	/// </summary>
	/// <param name="damage">The amount of damage to inflict the entity. (Must be positive.)</param>
	/// <param name="multiplier">The base damage modifier.</param>
	/// <returns></returns>
	public float TakeDamage(float damage, float multiplier = 1.0f)
	{
		if (IsInvulnerable || _health <= 0)
			return 0.0f;

		Debug.Assert(
			condition: damage >= 0.0f,
			message: "TakeDamage: attempted to use a negative value in an unsigned operation."
		);

		float totalDamage = CalculateDamage(damage, multiplier);
		_health = Mathf.Max(0.0f, _health - totalDamage);

		FireOnDamagedEvents();
		return totalDamage;
	}

	/// <summary>
	/// Reduces the entity's health by a specified amount, ignoring its armour value.
	/// </summary>
	/// <param name="damage">The amount of damage to inflict the entity.</param>
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
	/// Calculates the total damage taken by the entity. (Overrideable.)
	/// </summary>
	/// <param name="damage">The base damage value.</param>
	/// <param name="multiplier">The damage modifier.</param>
	/// <returns></returns>
	public virtual float CalculateDamage(float damage, float multiplier = 1.0f)
	{
		return damage * multiplier * (damage / (damage + Armour + BonusArmour));
	}

	/// <summary>
	/// Returns the entity's remaining health.
	/// </summary>
	/// <param name="asFac">If set to true, it will return the remaining health as a fraction.</param>
	/// <returns></returns>
	public float GetHealth(bool asFac = true)
	{
		if (asFac)
			return _health / MaxHealth;

		return _health;
	}

	/// <summary>
	/// Changes the entity's max health cap.
	/// </summary>
	/// <param name="maxHealth"></param>
	public void SetMaxHealth(float maxHealth)
	{
		float currentHealthFac = GetHealth();
		MaxHealth = maxHealth;

		_health = maxHealth * currentHealthFac;
	}

	/// <summary>
	/// Returns whether or not the entity is alive.
	/// </summary>
	/// <returns></returns>
	public bool IsAlive()
	{
		return _health > 0.001f;
	}

	private void FireOnDamagedEvents()
	{
		if (_health > 0.0f) {
			EmitSignal(SignalName.HealthChanged, GetHealth());
			return;
		}

		EmitSignal(SignalName.Death);
	}

	#endregion
}
