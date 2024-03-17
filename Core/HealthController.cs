using Godot;
using System.Diagnostics;

namespace SadChromaLib.Specialisations.Entities;

/// <summary>
/// A controller that provides a common interface for entity life state.
/// </summary>
public sealed partial class HealthController
{
    public delegate float DamageCalculationDelegate(float damageBase);
	public delegate void HealthChangedDelegate(float newHealth);
	public delegate void DeathDelegate();

    public DamageCalculationDelegate OnCalculateDamage = null;
    public HealthChangedDelegate OnHealthChanged = null;
    public DeathDelegate OnDeath = null;

	public bool IsInvulnerable;

	private float _health;
	private float _maxHealth;

	public HealthController(float maxHealth = 100.0f, bool invulerable = false)
    {
        IsInvulnerable = invulerable;

        _maxHealth = maxHealth;
		_health = _maxHealth;
	}

    #region Status Info

    /// <summary>
    /// (Property) The health controller's maximum health value
    /// </summary>
    /// <value></value>
    public float MaxHealth
    {
        get => GetMaxHealth();
        set => SetMaxHealth(value);
    }

    public float GetMaxHealth() {
        return _maxHealth;
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
		return _health / _maxHealth;
	}

    #endregion

	#region Main Functions

    /// <summary>
	/// Changes the entity's max health cap.
	/// </summary>
	/// <param name="maxHealth"></param>
	public void SetMaxHealth(float maxHealth)
	{
		float currentHealthFac = GetHealthAsFac();
		_maxHealth = maxHealth;

		_health = maxHealth * currentHealthFac;
	}

	/// <summary>
	/// Applies a set amount of damage to the target entity.
    /// If a damage calculation delegate is assigned, it will pipe the base damage to that,
    /// and use its output instead.
	/// </summary>
	/// <param name="damage">The amount of damage to inflict the entity. (Must be positive.)</param>
	/// <returns></returns>
	public float TakeDamage(float damage)
	{
		if (IsInvulnerable)
			return 0.0f;

		Debug.Assert(
			condition: damage >= 0.0f,
			message: "TakeDamage: attempted to use a negative value in an unsigned operation."
		);

        float finalDamage = OnCalculateDamage?.Invoke(damage) ?? damage;
		_health = Mathf.Max(0.0f, _health - finalDamage);

		FireOnDamagedEvents();

        return finalDamage;
	}

    /// <summary>
	/// Applies a fixed amount of damage to the target entity.
    /// If damage calculation should be ensured, use 'TakeDamage', instead.
	/// </summary>
	/// <param name="damage">The amount of damage to inflict the entity. (Must be positive.)</param>
	/// <returns></returns>
	public float TakeFixedDamage(float damage)
	{
		if (IsInvulnerable)
			return 0.0f;

		Debug.Assert(
			condition: damage >= 0.0f,
			message: "TakeDamage: attempted to use a negative value in an unsigned operation."
		);

		_health = Mathf.Max(0.0f, _health - damage);

		FireOnDamagedEvents();
        return damage;
	}

	/// <summary>
	/// Restores the entity's health by a specified amount.
	/// </summary>
	/// <param name="amount">The amount of health to restore.</param>
	public float RestoreHealth(float amount)
	{
		Debug.Assert(
			condition: amount >= 0.0f,
			message: "RestoreHealth: attempted to use a negative value in an unsigned operation."
		);

		_health = Mathf.Min(_maxHealth, _health + amount);

		OnHealthChanged?.Invoke(GetHealth());
		return amount;
	}

	#endregion

    #region Helpers

    private void FireOnDamagedEvents()
	{
		if (_health > 0.0f) {
			OnHealthChanged?.Invoke(_health);
			return;
		}

		OnDeath?.Invoke();
	}

    #endregion
}
