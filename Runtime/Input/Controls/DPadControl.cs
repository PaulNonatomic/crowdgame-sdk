namespace Nonatomic.CrowdGame
{
	/// <summary>
	/// Directional pad control supporting 4 or 8 directions.
	/// </summary>
	public class DPadControl : IControl
	{
		public string Id { get; }
		public ControlType Type => ControlType.DPad;
		public string Label { get; }
		public bool EightDirectional { get; set; }

		public DPadControl(string id, string label = "D-Pad", bool eightDirectional = false)
		{
			Id = id;
			Label = label;
			EightDirectional = eightDirectional;
		}
	}
}
