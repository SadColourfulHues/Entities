using Godot;

namespace SadChromaLib.Specialisations.Entities;

/// <summary>
/// A resource describing a status effect. (Can be configured to use icons.)
/// </summary>
[GlobalClass]
public sealed partial class StatusDefinition : Resource
{
	[Export]
	public string StatusId;

	[Export]
	public string DisplayName;

	[Export(PropertyHint.File)]
	public string IconPath;

	/// <summary>
	/// Retrieves the texture specified at the status definition's icon path.
	/// </summary>
	/// <returns></returns>
	public Texture2D GetIcon()
	{
		return ResourceLoader.Load<Texture2D>(IconPath);
	}
}
