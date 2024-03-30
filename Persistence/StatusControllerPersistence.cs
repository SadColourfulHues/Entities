using System;
using System.Diagnostics;

using SadChromaLib.Persistence;

namespace SadChromaLib.Specialisations.Entities;

public sealed partial class StatusHandlerController: ISerialisableComponent
{
	public void Serialise(PersistenceWriter writer)
	{
		ReadOnlySpan<IStatus> statuses = _statuses.AsSpan();

		writer.Write(MaxStatuses);

		for (int i = 0; i < MaxStatuses; ++ i) {
			if (statuses[i] is null) {
				writer.Write(false);
				continue;
			}

			writer.Write(true);
			writer.Write(statuses[i].StatusGetIdentifier());
			writer.Write(statuses[i].StatusGetRemainingDuration());
		}
	}

	public void Deserialise(PersistenceReader reader)
	{
		// Clear previous entries
		for (int i = 0; i < MaxStatuses; ++i) {
			if (_statuses[i] is not null) {
				OnStatusRemoved?.Invoke(_statuses[i].StatusGetIdentifier());
			}

			_statuses[i] = null;
		}

		int count = reader.ReadInt();

		// Malformed data
		Debug.Assert(
			condition: count <= MaxStatuses,
			message: "StatusController: deserialisation failed => max status count mismatch"
		);

		_isLocked = false;

		for (int i = 0; i < MaxStatuses; ++ i) {
			if (!reader.ReadBool())
				continue;

			string statusId = reader.ReadString();

			_statuses[i] = _registry.CreateStatus(
				statusId: statusId,
				duration: reader.ReadFloat()
			);

			OnStatusAdded?.Invoke(statusId);
		}
	}
}