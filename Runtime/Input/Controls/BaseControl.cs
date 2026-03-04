using System;

namespace Nonatomic.CrowdGame.Input
{
	/// <summary>
	/// Base implementation for typed controls with change notification.
	/// </summary>
	public abstract class BaseControl<T> : IControl<T>
	{
		public string Id { get; }
		public abstract ControlType Type { get; }
		public bool HasValue { get; private set; }

		public T Value
		{
			get => _value;
			protected set
			{
				_value = value;
				HasValue = true;
				OnValueChanged?.Invoke(_value);
			}
		}

		public event Action<T> OnValueChanged;

		private T _value;

		protected BaseControl(string id)
		{
			Id = id;
		}

		public virtual void Reset()
		{
			_value = default;
			HasValue = false;
		}

		/// <summary>
		/// Apply an InputMessage to this control. Called by the input system.
		/// </summary>
		public abstract void Apply(InputMessage message);
	}
}
