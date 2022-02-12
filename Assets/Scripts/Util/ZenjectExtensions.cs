using Unity.Entities;
using Zenject;

namespace Util
{
	public static class ZenjectExtensions
	{
		public static IfNotBoundBinder FromEcs<TContract>(this ConcreteIdBinderGeneric<TContract> binder) where TContract : ComponentSystemBase
		{
			return binder.FromMethod(
					c =>
					{
						var contract = (TContract)c.Container.Resolve<World>().GetExistingSystem(typeof(TContract));
						c.Container.Inject(contract);
						return contract;
					})
				.NonLazy();
		}
	}
}