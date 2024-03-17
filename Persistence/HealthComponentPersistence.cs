using Godot;

using SadChromaLib.Persistence;

using SerialisedData = Godot.Collections.Dictionary<Godot.StringName, Godot.Variant>;

namespace SadChromaLib.Specialisations.Entities;

public partial class HealthComponent : ISerialisableComponent
{
	public SerialisedData Serialise() {
		return _controller.Serialise();
	}

	public void Deserialise(SerialisedData data) {
		_controller.Deserialise(data);
	}
}