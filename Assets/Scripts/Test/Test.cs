using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test : MonoBehaviour
{
    // World world;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameService.Ins.GlobalInitialize();
        GameService.PlayerInput.Enable();
        

        //XmlSaver.SaveObjectToXml<GameSettings>(settings, System.IO.Path.Combine(FilePathHandler.ConfigFolderPath, "Settings.xml"));

        // if(UnityEngine.InputSystem.Keyboard.current.spaceKey.wasPressedThisFrame)
        // {
        //     Debug.Log("Space key was pressed.");
        //     // Attempt to drop 1 "log" from the testThing's container
        //     //int droppedQuantity = ItemInteractionService.DropToWorld(testThing, "log", 1);

        //     Vector2 playerPos = GameService.Ins.GetFocusObject().transform.position;
        //     Vector2Int toChunk = Chunk.GetChunkPosition(playerPos);
        //     Vector2 toTile = new Vector2(Mathf.Floor(playerPos.x), Mathf.Floor(playerPos.y));
        //     Tile tile = GameService.GetWorldHandler().GetChunk(toChunk.x, toChunk.y).GetTileAtPosition(toTile);
        //     if(tile.TryGetExistingContainer(out Container container))
        //     {
        //         container.TryAddItem("log", 1);
        //         Debug.Log(container.GetContainerInfo());
        //     }
        // }



    }

    // Update is called once per frame
    void Update()
    {
        

    }

    void FixedUpdate()
    {
        // Vector2 move = PlayerInput.Move;
        // if(move != Vector2.zero)
        // {
        //     Camera.main.transform.position += new Vector3(move.x, move.y, 0) * Time.deltaTime * 5f;
        // }
    }

    void OnGUI()
    {
        
    }

    [MakeButtonFuncOnTestClass(false)]
    void DoSomeThing()
    {
    }

    [MakeButtonFuncOnTestClass(true)]
    void DoSomeThingElse()
    {
    }
}
