using Godot;

using SadChromaLib.Persistence;

using SerialisedData = Godot.Collections.Dictionary<Godot.StringName, Godot.Variant>;

namespace SadChromaLib.Specialisations.Entities;

public partial class HealthController : ISerialisableComponent
{
	private static StringName KeyHealth => "health";
	private static StringName KeyMaxhealth => "maxHealth";
	private static StringName KeyArmour => "armour";
	private static StringName KeyInvulnerable => "isInvulnerable";

	public SerialisedData Serialise()
	{
		return new() {
			[KeyHealth] = _health,
			[KeyMaxhealth] = _maxHealth,
			[KeyInvulnerable] = IsInvulnerable
		};
	}

	public void Deserialise(SerialisedData data)
	{
		IsInvulnerable = (bool) data[KeyInvulnerable];
		_maxHealth = (float) data[KeyMaxhealth];
		_health = (float) data[KeyHealth];

		OnHealthChanged?.Invoke(GetHealth());
	}
}