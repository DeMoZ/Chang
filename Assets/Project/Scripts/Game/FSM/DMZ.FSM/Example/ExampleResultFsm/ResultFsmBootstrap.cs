using DMZ.FSM.Example;
using UnityEngine;

namespace DMZ.FSM.ResultExample
{
    public class ResultFsmBootstrap : MonoBehaviour
    {
        private ExampleResultFSM _fsm;

        private void Start()
        {
            _fsm = new ExampleResultFSM(OnStateChanged);
        }

        private void OnStateChanged(States state)
        {
            Debug.Log($"State changed to: {state}");
        }
    }
}