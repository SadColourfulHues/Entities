using Godot;
using Godot.Collections;

using System;

using SadChromaLib.Persistence;

using SerialisedData = Godot.Collections.Dictionary<Godot.StringName, Godot.Variant>;

namespace SadChromaLib.Specialisations.Entities;

public sealed partial class StatusHandlerComponent : ISerialisableComponent
{
	private static StringName KeyStatuses => "status";
	private static StringName KeyStatusId => "statusId";
	private static StringName KeyStatusDuration => "statusDuration";

	public SerialisedData Serialise()
	{
		ReadOnlySpan<IStatus> statuses = _statuses;
		Array<SerialisedData> serialisedStatuses = new();

		for (int i = 0; i < MaxStatuses; ++ i) {
			if (statuses[i] == null) {
				serialisedStatuses.Add(null);
				continue;
			}

			serialisedStatuses.Add(new() {
				[KeyStatusId] = statuses[i].StatusGetIdentifier(),
				[KeyStatusDuration] = statuses[i].StatusGetRemainingDuration()
			});
		}

		return new() {
			[KeyStatuses] = serialisedStatuses
		};
	}

	public void Deserialise(SerialisedData data)
	{
		Array<SerialisedData> statuses = (Array<SerialisedData>) data[KeyStatuses];
		_isLocked = false;

		for (int i = 0; i < statuses.Count; ++ i) {
			if (statuses[i] == null ||
				statuses[i].Count < 1)
			{
				if (_statuses[i] != null) {
					EmitSignal(
						SignalName.StatusRemoved,
						_statuses[i].StatusGetIdentifier()
					);
				}

				_statuses[i] = null;
				continue;
			}

			StringName statusId = (string) statuses[i][KeyStatusId];
			float duration = (float) statuses[i][KeyStatusDuration];

			_statuses[i] = _registryRef.CreateStatus(statusId, duration);
			EmitSignal(SignalName.StatusAdded, statusId);
		}
	}
}