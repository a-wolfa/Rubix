using Rubik;
using Zenject;

namespace Installers
{
    public class RubikInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<RubikManager>().FromComponentInHierarchy().AsSingle();
        }
    }
}