using Controllers;
using Services;
using UnityEngine;
using Zenject;

namespace Installers
{
    public class RubikInstaller : MonoInstaller
    {
        [SerializeField]
        private GameObject cubeControllerPrefab;

        public override void InstallBindings()
        {
            Container.Bind<CubeControls>().AsSingle();

            Container.Bind<CubeState>().AsSingle();

            Container.Bind<CubeController>()
                .FromComponentInHierarchy()
                .AsSingle();
            
            Container.Bind<CameraController>()
                .FromComponentInHierarchy()
                .AsSingle();
        }
    }
}