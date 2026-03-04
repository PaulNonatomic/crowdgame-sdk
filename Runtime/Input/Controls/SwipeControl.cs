using UnityEngine;

namespace Nonatomic.CrowdGame.Input
{
	/// <summary>
	/// Directional swipe gesture with velocity.
	/// </summary>
	public class SwipeControl : BaseControl<SwipeData>
	{
		public override ControlType Type => ControlType.Swipe;

		public Vector2 Direction => Value.Direction;
		public float Velocity => Value.Velocity;

		public SwipeControl(string id) : base(id) { }

		public override void Apply(InputMessage message)
		{
			Value = message.Swipe;
		}
	}
}
