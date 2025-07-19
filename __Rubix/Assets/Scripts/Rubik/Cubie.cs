using Consts;
using UnityEngine;
using Zenject;

namespace Rubik
{
    public class Cubie : MonoBehaviour
    {
        public LayerMask cubieLayer;
        private Vector2? mouseDownPosition = null;
        private bool isDragging = false;
        private Vector3? clickedFaceNormal = null;
        private Vector3? dragWorld = null;
        private Vector3? worldPos = null;
        private GameObject cubie;

        private Vector3? localHitPos = null;

        [Inject] private RubikManager _rubikManager;



        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out RaycastHit hit, 100f, cubieLayer))
                {
                    cubie = hit.collider.gameObject;

                    worldPos = hit.point;
                    localHitPos = _rubikManager.transform.InverseTransformPoint(worldPos.Value);
                    Debug.Log($"{localHitPos.Value}");

                    Vector3 faceNormal = hit.normal;
                    Vector3 localNormal = transform.InverseTransformDirection(faceNormal);
                    clickedFaceNormal = localNormal;
                    hit.collider.gameObject.SendMessage("OnCustomMouseDown", SendMessageOptions.DontRequireReceiver);
                }
            }

            if (isDragging && Input.GetMouseButton(0))
            {
                Vector2 currentMousePos = Input.mousePosition;
                if (mouseDownPosition.HasValue && clickedFaceNormal.HasValue)
                {
                    Vector2 delta = currentMousePos - mouseDownPosition.Value;
                    if (delta.magnitude > 10f)
                    {
                        Vector3 localNormal = clickedFaceNormal.Value;

                        string direction = GetDirection(delta);

                        int pivotIndex = GetPivotIndex(localNormal, direction);

                        _rubikManager.selectedPivot = pivotIndex;

                        float angle = 90f; // Default rotation angle
                        if (direction is "Left" or "Down")
                        {
                            angle = -angle;
                        }
                        _rubikManager.Rotate(GetRotationAxis(localNormal, IsHorizontalDrag(delta)), angle);
                        isDragging = false;
                    }
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                mouseDownPosition = null;
            }
        }

        private void OnCustomMouseDown()
        {
            mouseDownPosition = Input.mousePosition;
            isDragging = true;
        }

        private string GetDirection(Vector2 delta)
        {
            if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            {
                return delta.x > 0 ? "Right" : "Left";
            }
            else
            {
                return delta.y > 0 ? "Up" : "Down";
            }
        }

        private bool IsHorizontalDrag(Vector2 delta)
        {
            return Mathf.Abs(delta.x) > Mathf.Abs(delta.y);
        }

        private int GetPivotIndex(Vector3 normal, string direction)
        {
            BoxCollider box = cubie.GetComponent<BoxCollider>();
            int pivotIndex = -1;
            if (normal == Vector3.back)
            {
                if (direction is "Up" or "Down")
                {
                    pivotIndex = box.center.x < 0 ? (int)PivotType.Orange : (int)PivotType.Red;
                }
                else if (direction is "Right" or "Left")
                {
                    pivotIndex = box.center.y < 0 ? (int)PivotType.Green : (int)PivotType.Blue;
                }
            }
            if (normal == Vector3.forward)
            {
                if (direction is "Up" or "Down")
                {
                    pivotIndex = box.center.x < 0 ? (int)PivotType.Orange : (int)PivotType.Red;
                }
                else if (direction is "Right" or "Left")
                {
                    pivotIndex = box.center.y < 0 ? (int)PivotType.Green : (int)PivotType.Blue;
                }
            }
            if (normal == Vector3.up)
            {
                if (direction is "Right" or "Left")
                {
                    pivotIndex = box.center.z < 0 ? (int)PivotType.Yellow : (int)PivotType.White;
                }
                else if (direction is "Up" or "Down")
                {
                    pivotIndex = box.center.x < 0 ? (int)PivotType.Green : (int)PivotType.Blue;
                }
            }
            if (normal == Vector3.down)
            {
                if (direction is "Right" or "Left")
                {
                    pivotIndex = box.center.z < 0 ? (int)PivotType.White : (int)PivotType.Yellow;
                }
                else if (direction is "Up" or "Down")
                {
                    pivotIndex = box.center.x < 0 ? (int)PivotType.Orange : (int)PivotType.Red;
                }
            }
            if (normal == Vector3.right)
            {
                if (direction is "Up" or "Down")
                {
                    pivotIndex = box.center.z < 0 ? (int)PivotType.White : (int)PivotType.Yellow;
                }
                else if (direction is "Right" or "Left")
                {
                    pivotIndex = box.center.y < 0 ? (int)PivotType.Green : (int)PivotType.Blue;
                }
            }
            if (normal == Vector3.left)
            {
                if (direction is "Up" or "Down")
                {
                    pivotIndex = box.center.z < 0 ? (int)PivotType.White : (int)PivotType.Yellow;
                }
                else if (direction is "Right" or "Left")
                {
                    pivotIndex = box.center.y < 0 ? (int)PivotType.Green : (int)PivotType.Blue;
                }
            }

            return pivotIndex; // Default, adjust as needed
        }

        private Vector3 GetRotationAxis(Vector3 normal, bool isHorizontalDrag)
        {
            if (normal == Vector3.forward) // Front face
            {
                return isHorizontalDrag ? Vector3.up : Vector3.right;
            }
            else if (normal == Vector3.back) // Back face
            {
                return isHorizontalDrag ? Vector3.up : Vector3.right;
            }
            else if (normal == Vector3.up) // Top face
            {
                return isHorizontalDrag ? Vector3.right : Vector3.forward;
            }
            else if (normal == Vector3.down) // Bottom face
            {
                return isHorizontalDrag ? Vector3.right : Vector3.forward;
            }
            else if (normal == Vector3.right) // Right face
            {
                return isHorizontalDrag ? Vector3.up : Vector3.forward;
            }
            else if (normal == Vector3.left) // Left face
            {
                return isHorizontalDrag ? Vector3.up : Vector3.forward;
            }

            return Vector3.up; // Default fallback
        }

    }
}
