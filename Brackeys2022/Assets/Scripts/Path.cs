using UnityEngine;
using UnityEngine.SceneManagement;

public class Path : MonoBehaviour
{
    public int indexNumber;
    public int nextSceneIndex;
    [SerializeField] private Transform entrancePoint;

    /// <summary> Allows the editor to show the exit and entrance</summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(transform.position, transform.localScale);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(entrancePoint.position, .5f);
    }
}
