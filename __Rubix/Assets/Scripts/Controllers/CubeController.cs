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
            
            Debug.Log(cubieHitCollider);
            
            _overlappingPivots = Physics.OverlapBox(cubieHitCollider.bounds.center, 
                                                         cubieHitCollider.bounds.extents, 
                                                         cubieHitCollider.transform.rotation,
                                                         pivot);

            foreach (var overlapping in _overlappingPivots)
            {
                Debug.Log(overlapping.name);
            }
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
            var rotationAxis = Vector3.zero;
            var angle = 90f;
            
            var left = - _mainCamera.transform.right;
            var up = _mainCamera.transform.up;

            if (Mathf.Abs(Vector2.Dot(dragVector.normalized, (Vector2)left))
                > Mathf.Abs(Vector2.Dot(dragVector.normalized, (Vector2)up)))
            {
                rotationAxis = Vector3.Cross(_selectedFaceNormal, left).normalized;
                if (Vector3.Dot(dragVector, left) < 0) 
                    angle = -angle;
            }
            else
            {
                rotationAxis = Vector3.Cross(_selectedFaceNormal, up).normalized;
                if (Vector3.Dot(dragVector, up) < 0)
                    angle = -angle;
            }

            foreach (var candidatePivot in _overlappingPivots)
            {
                if (candidatePivot.GetComponent<Pivot>().rotationAxis == rotationAxis)
                    selectedPivot = candidatePivot;
            }
            
            Debug.Log(rotationAxis);
            Debug.Log(selectedPivot?.name);
            SetSlice(selectedPivot);
            StartCoroutine(RotateSlice(selectedPivot?.transform, rotationAxis, angle));
        }
        
        private void SetSlice(Collider selectedPivot)
        {
            Collider[] cubies = Physics.OverlapBox(selectedPivot.bounds.center, selectedPivot.bounds.extents, selectedPivot.transform.rotation, cubie);

            foreach (var cubieCollider in cubies)
            {
                cubieCollider.transform.SetParent(selectedPivot.transform);
                Debug.Log(cubieCollider.name);
            }
        }
        
        private IEnumerator RotateSlice(Transform slice, Vector3 axis, float angle)
        {
            Quaternion startRotation = slice.localRotation;

            // Use local axis properly
            Vector3 worldAxis = slice.TransformDirection(axis.normalized);
            Quaternion deltaRotation = Quaternion.AngleAxis(angle, worldAxis);
            Quaternion endRotation = startRotation * deltaRotation;

            float duration = 0.3f;
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
            slice.DetachChildren();
        }
        
        private void OnDrawGizmos()
        {
            if (selectedPivot == null) return;
            
            Gizmos.color = Color.red;
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position + selectedPivot.bounds.center, selectedPivot.transform.rotation, Vector3.one);
            Gizmos.matrix = rotationMatrix;
            Gizmos.DrawWireCube(Vector3.zero, selectedPivot.bounds.extents * 2);
        }
    }
    
}