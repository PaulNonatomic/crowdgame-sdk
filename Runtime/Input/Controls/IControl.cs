using System;

namespace Nonatomic.CrowdGame.Input
{
	/// <summary>
	/// Base control interface representing a single input modality.
	/// </summary>
	public interface IControl
	{
		string Id { get; }
		ControlType Type { get; }
		bool HasValue { get; }
		void Reset();
	}

	/// <summary>
	/// Typed control interface with value and change notification.
	/// </summary>
	public interface IControl<T> : IControl
	{
		T Value { get; }
		event Action<T> OnValueChanged;
	}
}
