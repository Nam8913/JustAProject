using UnityEngine;

public sealed class BuildService
{
    private bool isBuildMode = false;
    public BuildService()
    {
        isBuildMode = false;
    }

    public void TriggerBuildMode()
    {
        isBuildMode = true;
        #if UNITY_EDITOR
        Debug.Log("Entered Build Mode");
        #endif
    }

    public void ExitBuildMode()
    {
        isBuildMode = false;
        #if UNITY_EDITOR
        Debug.Log("Exited Build Mode");
        #endif
    }

    private bool IsBuildableAtPosition(Vector3 position)
    {
        
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, 0.05f);

        if(hit.collider != null)
        {
            return false;
        }

        return true;
    }

    public bool IsBuildableAtTile(Vector3 pos)
    {
        Vector2 tilePos = new Vector2(Mathf.Floor(pos.x) + 0.5f, Mathf.Floor(pos.y) + 0.5f);
        if(!IsBuildableAtPosition(pos))
        {
            return false;
        }


        //Create 4 raycasts to check the corners of the tile
        Vector2[] corners = new Vector2[]
        {
            tilePos + new Vector2(-0.48f, -0.48f),
            tilePos + new Vector2(0.48f, -0.48f),
            tilePos + new Vector2(-0.48f, 0.48f),
            tilePos + new Vector2(0.48f, 0.48f)
        };
        foreach(var corner in corners)
        {
            if(!IsBuildableAtPosition(corner))
            {
                return false;
            }
        }

        return true;
    }
}
