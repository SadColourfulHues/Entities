using Godot;

using SadChromaLib.Persistence;

using SerialisedData = Godot.Collections.Dictionary<Godot.StringName, Godot.Variant>;

namespace SadChromaLib.Specialisations.Entities;

public partial class HealthComponent : ISerialisableComponent
{
	private static StringName KeyHealth => "health";
	private static StringName KeyMaxhealth => "maxHealth";
	private static StringName KeyArmour => "armour";
	private static StringName KeyInvulnerable => "isInvulnerable";

	public SerialisedData Serialise()
	{
		return new() {
			[KeyHealth] = _health,
			[KeyMaxhealth] = MaxHealth,
			[KeyArmour] = Armour,
			[KeyInvulnerable] = IsInvulnerable
		};
	}

	public void Deserialise(SerialisedData data)
	{
		IsInvulnerable = (bool) data[KeyInvulnerable];
		MaxHealth = (float) data[KeyMaxhealth];
		Armour = (float) data[KeyArmour];
		_health = (float) data[KeyHealth];

		EmitSignal(SignalName.HealthChanged, GetHealth());
	}
}