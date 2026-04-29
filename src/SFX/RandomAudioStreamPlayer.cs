using Godot.Collections;

namespace Game.SFX;

public partial class RandomAudioStreamPlayer : AudioStreamPlayer
{
	[Export]
	public Array<AudioStream>? RandomAudioStreams;

	public void PlayRandom()
	{
		Stream = RandomAudioStreams?.PickRandom();
		Play();
	}
}
