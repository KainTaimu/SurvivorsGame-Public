namespace Game.Interactables;

public interface IInteractable
{
	void Interact();
}

public partial class BaseInteractable : Node2D, IInteractable
{
	[Signal]
	public delegate void OnInteractionEventHandler();

	public virtual void Interact() { }
}
