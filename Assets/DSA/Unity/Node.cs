using UnityEngine;

namespace FrontierDispatch.Unity
{
    public class Node : MonoBehaviour
    {
        WorldMapDemoController mapController => WorldMapDemoController.Instance;

        public float fromStartDistance => Vector3.Distance(mapController.Views[int.Parse(mapController.StartIDText)].transform.position, transform.position);
        public float toGoalDistance => Vector3.Distance(mapController.Views[int.Parse(mapController.GoalIDText)].transform.position, transform.position);

    }
}
