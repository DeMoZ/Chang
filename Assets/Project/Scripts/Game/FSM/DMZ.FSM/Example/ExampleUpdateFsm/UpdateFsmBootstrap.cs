using DMZ.FSM.Example;
using UnityEngine;

namespace DMZ.FSM.UpdateExample
{
    public class UpdateFsmBootstrap : MonoBehaviour
    {
        private ExampleUpdateFSM _fsm;

        private void Start()
        {
            _fsm = new ExampleUpdateFSM(OnStateChanged);
        }

        private void Update()
        {
            _fsm.Update(Time.deltaTime);
        }

        private void OnStateChanged(States state)
        {
            Debug.Log($"State changed to: {state}");
        }
    }
}