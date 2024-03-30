using SadChromaLib.Persistence;

namespace SadChromaLib.Specialisations.Entities;

public sealed partial class StatusHandlerComponent: ISerialisableComponent
{
	public void Serialise(PersistenceWriter writer) {
		_controller.Serialise(writer);
	}

	public void Deserialise(PersistenceReader reader) {
		_controller.Deserialise(reader);
	}
}