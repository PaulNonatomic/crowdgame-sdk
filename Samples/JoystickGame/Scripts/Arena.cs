using System.Collections.Generic;
using UnityEngine;

namespace Nonatomic.CrowdGame.Samples.JoystickGame
{
	/// <summary>
	/// Owns arena scene references and visual styling.
	/// Registers itself in the ServiceLocator for external access.
	/// ExecuteAlways ensures materials are applied in editor for correct preview.
	/// DefaultExecutionOrder ensures registration before other components query it.
	/// </summary>
	[ExecuteAlways]
	[DefaultExecutionOrder(-100)]
	public class Arena : MonoBehaviour, IArena
	{
		[SerializeField] private Color _floorColour = new Color(0.15f, 0.15f, 0.2f);
		[SerializeField] private Color _wallColour = new Color(0.3f, 0.3f, 0.35f);
		[SerializeField] private Renderer _floorRenderer;
		[SerializeField] private Renderer[] _wallRenderers;

		public Renderer FloorRenderer => _floorRenderer;
		public IReadOnlyList<Renderer> WallRenderers => _wallRenderers;

		private void OnEnable()
		{
			ServiceLocator.Register<IArena>(this);
			ApplyColours();
		}

		private void OnDisable()
		{
			ServiceLocator.Unregister<IArena>();
		}

		private void ApplyColours()
		{
			if (_floorRenderer != null)
			{
				MaterialUtility.ApplyLitMaterial(_floorRenderer, _floorColour);
			}

			if (_wallRenderers == null) return;

			foreach (var wallRenderer in _wallRenderers)
			{
				if (wallRenderer == null) continue;
				MaterialUtility.ApplyLitMaterial(wallRenderer, _wallColour);
			}
		}
	}
}
