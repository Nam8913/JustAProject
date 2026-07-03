using UnityEngine;

public interface IInventoryView
{
    public void GetSelectedContainer(ContainerButton button);
    public void TryRemoveContainerButton(ContainerButton button);

}
