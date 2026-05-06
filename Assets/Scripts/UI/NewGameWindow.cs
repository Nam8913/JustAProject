using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewGameWindow : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameWorldInputField;
    [SerializeField] private TMP_InputField seedInputField;

    public void CreateNewGame()
    {
        string worldName = nameWorldInputField.text;
        string seed = seedInputField.text;
        
        int seedValue = string.IsNullOrEmpty(seed) ? Random.Range(int.MinValue, int.MaxValue) : seed.GetHashCode();

        Debug.Log($"Creating new world with name: {worldName} and seed: {seedValue}");
        

        World newWorld = new World(worldName, seedValue);
        World.RegisterNewWorldToCurrentWorld(newWorld);

        SceneHandler.LoadPlayScene();
    }
}
