using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using HeavenStudio.Editor.Commands;

public class TestCommand : IAction
{
    private GameObject prefab;
    private Vector3 pos;

    private GameObject spawnedgameObj;

    public TestCommand(GameObject prefab, Vector3 pos)
    {
        this.prefab = prefab;
        this.pos = pos;
    }

    public void Execute()
    {
        spawnedgameObj = GameObject.Instantiate(prefab, pos, Quaternion.identity);
    }

    public void Redo()
    {
        throw new System.NotImplementedException();
    }

    public void Undo()
    {
        GameObject.Destroy(spawnedgameObj);
    }
}
