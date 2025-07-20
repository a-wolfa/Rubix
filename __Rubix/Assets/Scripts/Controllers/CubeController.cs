using System;
using System.Collections;
using System.Collections.Generic;
using Components;
using Consts;
using Services;
using UnityEngine;
using Zenject;

namespace Controllers
{
    public class CubeController : MonoBehaviour
    {
        [SerializeField] private LayerMask cubie;
        [SerializeField] private LayerMask pivot;
        
        private CubeState _cubeState;
        private CubeControls _cubeControls;
        
        private Camera _mainCamera;
        private Transform _selectedCubie;
        private Vector3 _selectedFaceNormal;
        private Vector3 _dragStartPos;
        private bool _isDragging = false;

        private Collider[] _overlappingPivots;
        [SerializeField] private Collider selectedPivot;

        [Inject]
        public void Construct(CubeState cubeState, CubeControls cubeControls)
        {
            _cubeState = cubeState;
            _cubeControls = cubeControls;
        }

        private void Awake()
        {
            _mainCamera = Camera.main;
            _cubeControls.Enable();
            _cubeControls.Player.Click.performed += _ => OnClick();
            _cubeControls.Player.Click.canceled += _ => OnRelease();
        }

        private void Start()
        {
            FindAllPieces();
        }

        private void OnClick()
        {
            var ray = _mainCamera.ScreenPointToRay(_cubeControls.Player.MousePosition.ReadValue<Vector2>());

            if (!Physics.Raycast(ray, out var cubieHit, Mathf.Infinity, cubie)) return;
            
            _isDragging = true;
            _selectedCubie = cubieHit.collider.transform;
            _selectedFaceNormal = cubieHit.normal;
            _dragStartPos = _cubeControls.Player.MousePosition.ReadValue<Vector2>();

            var cubieHitCollider = cubieHit.collider;
            
            _overlappingPivots = Physics.OverlapBox(cubieHitCollider.bounds.center, 
                                                         cubieHitCollider.bounds.extents, 
                                                         cubieHitCollider.transform.rotation,
                                                         pivot);
            
        }

        private void OnRelease()
        {
            if (!_isDragging) return;
            
            _isDragging = false;

            var dragVector = _cubeControls.Player.MousePosition.ReadValue<Vector2>() - (Vector2)_dragStartPos;
            
            if (dragVector.magnitude < 50f) 
                return;
            
            PerformRotation(dragVector);
        }

        private void FindAllPieces()
        {
            var cubies = GameObject.FindGameObjectsWithTag(Tags.Cubie);

            foreach (var cubie in cubies)
            {
                var x = Mathf.RoundToInt(cubie.transform.position.x);
                var y = Mathf.RoundToInt(cubie.transform.position.y);
                var z = Mathf.RoundToInt(cubie.transform.position.z);
                
                _cubeState.Pieces[x + 1, y + 1, z + 1] = cubie.transform;
            }
        }

        private void PerformRotation(Vector2 dragVector)
        {
            Vector3 rotationAxis = Vector3.zero;
            float angle = 90f;
            
            Vector3 dragStart3D = _mainCamera.ScreenToWorldPoint(new Vector3(_dragStartPos.x, _dragStartPos.y, _mainCamera.WorldToScreenPoint(_selectedCubie.position).z));
            Vector3 dragEnd3D = _mainCamera.ScreenToWorldPoint(new Vector3(_dragStartPos.x + dragVector.x, _dragStartPos.y + dragVector.y, _mainCamera.WorldToScreenPoint(_selectedCubie.position).z));
            Vector3 worldDragDirection = (dragEnd3D - dragStart3D).normalized;

            rotationAxis = Vector3.Cross(_selectedFaceNormal, worldDragDirection).normalized;
            
            if (Vector3.Dot(worldDragDirection, dragEnd3D - dragStart3D) < 0)
                angle = -angle;

            selectedPivot = null;
            
            float bestMatch = -1f;
            foreach (var candidatePivot in _overlappingPivots)
            {
                Vector3 pivotAxis = candidatePivot.GetComponent<Pivot>().rotationAxis.normalized;
                
                float dot = Mathf.Abs(Vector3.Dot(rotationAxis, pivotAxis));
                
                if (dot > bestMatch && dot > 0.8f)
                {
                    bestMatch = dot;
                    selectedPivot = candidatePivot;
                    
                    if (Vector3.Dot(rotationAxis, pivotAxis) < 0)
                        angle = -angle;
                }
            }
            
            if (selectedPivot != null)
            {
                SetSlice(selectedPivot);
                StartCoroutine(RotateSlice(selectedPivot.transform, selectedPivot.GetComponent<Pivot>().rotationAxis, angle));
            }
        }
        
        private void SetSlice(Collider selectedPivot)
        {
            Collider[] cubies = Physics.OverlapBox(selectedPivot.bounds.center, selectedPivot.bounds.extents, selectedPivot.transform.rotation, cubie);

            foreach (var cubieCollider in cubies)
            {
                cubieCollider.transform.SetParent(selectedPivot.transform);
            }
        }
        
        private IEnumerator RotateSlice(Transform slice, Vector3 axis, float angle)
        {
            Quaternion startRotation = slice.localRotation;
            
            Vector3 worldAxis = slice.TransformDirection(axis.normalized);
            Quaternion deltaRotation = Quaternion.AngleAxis(angle, worldAxis);
            Quaternion endRotation = startRotation * deltaRotation;

            float duration = 0.15f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                slice.localRotation = Quaternion.Slerp(startRotation, endRotation, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            slice.localRotation = endRotation;

            yield return new WaitForEndOfFrame();
            slice.SetParent(transform);
            slice.DetachChildren();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (selectedPivot == null) return;
            
            Gizmos.color = Color.red;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position + selectedPivot.bounds.center, selectedPivot.transform.rotation, Vector3.one);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(Vector3.zero, selectedPivot.bounds.extents * 2);
        }
#endif
    }
}