using Godot;

namespace MoonFlow.Scene.Main;

[GlobalClass]
public partial class MoonFlowStatusIcon : TextureRect
{
	public enum AnimationStates : int
	{
		SPINNING = 0,
		ACTIVE = 1,
		IDLE = 2,
	}

	[Export]
	public AnimationStates AnimationState = AnimationStates.SPINNING;

	private double AnimationTimer = 0.0F;

	private const double AnimationLength = 2.0F;
	private const int AnimationFPS = 24;

	public override void _Process(double delta)
	{
		// Update animation timer
		AnimationTimer += delta;
		if (AnimationTimer > AnimationLength)
			AnimationTimer -= AnimationLength;

		// Update animation frame in shader
		if (Material.GetType() != typeof(ShaderMaterial))
			return;

		int frame = (int)(AnimationTimer * AnimationFPS);
		frame += (int)AnimationState * (int)(AnimationLength * AnimationFPS);

		((ShaderMaterial)Material).SetShaderParameter("sheet_position", frame);
	}
}
