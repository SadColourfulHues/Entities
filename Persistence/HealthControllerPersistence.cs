using SadChromaLib.Persistence;

namespace SadChromaLib.Specialisations.Entities;

public partial class HealthController : ISerialisableComponent
{
	public void Serialise(PersistenceWriter writer)
	{
		writer.Write(_health);
		writer.Write(_maxHealth);
		writer.Write(IsInvulnerable);
	}

	public void Deserialise(PersistenceReader reader)
	{
		_health = reader.ReadFloat();
		_maxHealth = reader.ReadFloat();
		IsInvulnerable = reader.ReadBool();

		OnHealthChanged?.Invoke(GetHealth());
	}
}