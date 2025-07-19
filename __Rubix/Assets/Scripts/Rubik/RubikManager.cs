using System;
using Consts;
using UnityEngine;

namespace Rubik
{
    public class RubikManager : MonoBehaviour
    {
        public GameObject[] pivots;
        
        public int selectedPivot;

        public void Rotate(Vector3 rotationAxis, float angle)
        {
            // Rotate around the specified axis
            GameObject pivot = pivots[selectedPivot];
            PivotController pivotController = pivot.GetComponent<PivotController>();
            Debug.Log(pivot.name);
            ParentThePlaneToPivot(pivot);
            pivot.transform.Rotate(pivotController.rotationAxis, angle, Space.Self);
            pivot.transform.DetachChildren();
        }

        private void ParentThePlaneToPivot(GameObject pivot)
        {
            BoxCollider box = pivot.GetComponent<BoxCollider>();
            Collider[] cubieColliders = Physics.OverlapBox(box.transform.position, box.size, box.transform.rotation);

            foreach (Collider cubie in cubieColliders)
            {
                if (cubie.gameObject.layer != LayerMask.NameToLayer("Cubie"))
                {
                    continue; // Skip if not a Cubie
                }
                cubie.transform.parent = pivot.transform;
            }
        }

        private void OnDrawGizmos()
        {
            BoxCollider box = pivots[selectedPivot].GetComponent<BoxCollider>();
            Gizmos.color = Color.cyan;

            // Save the original matrix
            Matrix4x4 originalMatrix = Gizmos.matrix;

            // Set matrix with position, rotation, and scale
            Gizmos.matrix = Matrix4x4.TRS(box.transform.position, box.transform.rotation, Vector3.one);

            // Draw the cube with local position (0,0,0) since matrix handles position
            Gizmos.DrawWireCube(Vector3.zero, box.size);

            // Restore the original matrix
            Gizmos.matrix = originalMatrix;
        }
    }
}
