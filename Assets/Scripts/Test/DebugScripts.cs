using System;
using Unity.VisualScripting;
using UnityEngine;

public class DebugScripts : MonoBehaviour
{
    [SerializeField] private bool isDebugMode = true;
    [SerializeField] private bool showActiveChunks = false;
    [SerializeField] private bool showChunkMemoryInPathfinding = true;
    [SerializeField] private bool showTilesInChunkMemoryInPathfinding = true;
    [SerializeField] private bool showCreatures = true;

    void OnDrawGizmos()
    {
        if(!isDebugMode)
        {
            return;
        }

        if(showActiveChunks)
        {
            // Draw working chunks
            ShowActiveChunks();
        }

        if(showChunkMemoryInPathfinding || showTilesInChunkMemoryInPathfinding)
        {
            // Draw pathfinding chunk memory
            ShowPathfindingChunkMemory();
        }

        if(showCreatures)
        {
            // Draw creatures
            ShowCreatures();
        }
    }

    private void ShowCreatures()
    {
        if(ThingHandler.Things.Count > 0)
        {
            foreach(var thing in ThingHandler.Things)
            {
                if(thing is Creature creature)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawSphere(creature.transform.position, 0.2f);
                }
            }
        }
    }

    void ShowActiveChunks()
    {
        if(GameService.Ins.WorldHandler != null)
        {
            foreach(var chunkPos in GameService.Ins.WorldHandler.GetActiveChunkPositions())
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube((Vector2)chunkPos * LocalTestValue.tilesPerChunk + new Vector2(LocalTestValue.tilesPerChunk / 2f, LocalTestValue.tilesPerChunk / 2f), new Vector2(LocalTestValue.tilesPerChunk, LocalTestValue.tilesPerChunk));
            }
        }
    }

    void ShowPathfindingChunkMemory()
    {
        if(GameService.Ins.Navigation != null)
        {
            foreach(var chunkGraphPair in GameService.Ins.Navigation.Pathfinder.TileGraphsByChunk)
            {
                if(showChunkMemoryInPathfinding)
                {
                    Vector2 chunkPos = chunkGraphPair.Key;
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube((Vector2)chunkPos * LocalTestValue.tilesPerChunk + new Vector2(LocalTestValue.tilesPerChunk / 2f, LocalTestValue.tilesPerChunk / 2f), new Vector2(LocalTestValue.tilesPerChunk, LocalTestValue.tilesPerChunk));
                }

                if(showTilesInChunkMemoryInPathfinding)
                {
                    var tileGraph = chunkGraphPair.Value;
                    foreach(var node in tileGraph.Graph)
                    {
                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(node.Key, 0.1f);
                    }
                }
            }
        }
    }
}
