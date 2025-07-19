using UnityEngine;

namespace Services
{
    public class CubeState
    {
        public Transform[,,] Pieces { get; private set; } = new Transform[3, 3, 3];
    }
}