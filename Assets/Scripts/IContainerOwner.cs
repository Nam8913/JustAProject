public interface IContainerOwner
{
    bool TryGetContainer(out Container container);
}