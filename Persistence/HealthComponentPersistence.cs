using SadChromaLib.Persistence;

namespace SadChromaLib.Specialisations.Entities;

public partial class HealthComponent: ISerialisableComponent
{
	public void Serialise(PersistenceWriter writer) {
		_controller.Serialise(writer);
	}

	public void Deserialise(PersistenceReader reader) {
		_controller.Deserialise(reader);
	}
}