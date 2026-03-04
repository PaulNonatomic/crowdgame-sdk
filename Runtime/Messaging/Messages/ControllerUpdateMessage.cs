using System;
using System.Collections.Generic;

namespace Nonatomic.CrowdGame.Messaging
{
	/// <summary>
	/// Sent to phone clients when the controller layout changes.
	/// </summary>
	[Serializable]
	public class ControllerUpdateMessage : BaseMessage
	{
		public string LayoutName { get; set; }
		public string Orientation { get; set; }
		public List<ControlDefinitionData> Controls { get; set; } = new List<ControlDefinitionData>();

		public ControllerUpdateMessage() : base("controller_update") { }

		public ControllerUpdateMessage(IControllerLayout layout) : this()
		{
			LayoutName = layout.LayoutName;
			Orientation = layout.RequiredOrientation.ToString();

			foreach (var control in layout.Controls)
			{
				Controls.Add(new ControlDefinitionData
				{
					Id = control.Id,
					Type = control.Type.ToString(),
					Placement = control.Placement.ToString(),
					Label = control.Label
				});
			}
		}
	}

	[Serializable]
	public class ControlDefinitionData
	{
		public string Id { get; set; }
		public string Type { get; set; }
		public string Placement { get; set; }
		public string Label { get; set; }
	}
}
