using Godot;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;

namespace SadChromaLib.Specialisations.Entities;

/// <summary>
/// A registry object that stores and handles the creation of status effects.
/// </summary>
[GlobalClass]
public sealed partial class StatusRegistry : Resource
{
	[Export]
	private StatusDefinition[] _definitions;

	private readonly Dictionary<string, Type> _statusTypes;
	private bool _isInitialised;

	public StatusRegistry()
	{
		_statusTypes = new();
		_isInitialised = true;

		// Scan for status types
		Assembly assembly = Assembly.GetExecutingAssembly();
		ReadOnlySpan<Type> types = assembly.GetTypes();

		for (int i = 0; i < types.Length; ++ i) {
			BindStatusId statusIdAttribute = types[i].GetCustomAttribute<BindStatusId>();

			if (statusIdAttribute == null)
				continue;

			_statusTypes[statusIdAttribute.StatusId] = types[i];
		}
	}

	#region Main Functions

	/// <summary>
	/// Returns whether or not a status ID is present in the registry.
	/// </summary>
	/// <param name="statusId">The status ID to check.</param>
	/// <returns></returns>
	public bool IsValid(string statusId)
	{
		ReadOnlySpan<StatusDefinition> definitions = _definitions;

		for (int i = 0; i < definitions.Length; ++ i) {
			if (_definitions[i].StatusId != statusId)
				continue;

			return true;
		}

		return false;
	}

	/// <summary>
	/// Returns a status effect definition from the registry.
	/// </summary>
	/// <param name="statusId"></param>
	/// <returns></returns>
	public StatusDefinition GetDefinition(string statusId)
	{
		ReadOnlySpan<StatusDefinition> definitions = _definitions;

		for (int i = 0; i < definitions.Length; ++ i) {
			if (_definitions[i].StatusId != statusId)
				continue;

			return _definitions[i];
		}

		return null;
	}

	/// <summary>
	/// Creates a new status effect processor using its status ID.
	/// </summary>
	/// <param name="statusId">The ID of the status effect.</param>
	/// <param name="duration">How long should the status effect last.</param>
	/// <returns></returns>
	public IStatus CreateStatus(string statusId, float duration)
	{
		Debug.Assert(
			condition: _statusTypes.ContainsKey(statusId),
			message: $"Status type {statusId} is not bound to any class using BindStatusId."
		);

		object statusObject = Activator.CreateInstance(_statusTypes[statusId]);

		Debug.Assert(
			condition: statusObject is IStatus,
			message: $"Status type {statusId} is not bound to a subclass of BaseStatus."
		);

		IStatus status = (IStatus) statusObject;
		status.StatusSetDuration(duration);

		return status;
	}

	#endregion
}

/// <summary>
/// An attribute that binds a status ID to a processor. (Required to make a status effect instantiable using the registry's 'CreateStatus' method.)
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class BindStatusId : Attribute
{
	public string StatusId;

	public BindStatusId(string id)
	{
		StatusId = id;
	}
}