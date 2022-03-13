using UnityEngine;

// this is a script for testing

using HeavenStudio.Editor;

namespace HeavenStudio.Tests
{
    public class WTF : MonoBehaviour
    {
        public GameObject test;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                GetComponent<CommandManager>().Execute(new TestCommand(test, new Vector3(Random.Range(-8f, 8f), Random.Range(-6f, 6f))));
            }
        }
    }
}