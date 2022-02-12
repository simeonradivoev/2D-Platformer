using AI;
using AI.FSM;
using UnityEngine;
using Util;
using Zenject;

namespace DefaultNamespace.Installers
{
	public class GoapInstaller : MonoInstaller
	{
		[SerializeField] private AttackPlayerGoapSystem.Settings attackPlayerSettings;
		[SerializeField] private MoveStateSystem.Settings moveStateSystemSettings;
		[SerializeField] private SearchTargetGoapActionSystem.Settings findTargetSettings;
		[SerializeField] private ReloadGoapActionSystem.Settings reloadSettings;

		public override void InstallBindings()
		{
			// Settings
			Container.Bind<MoveStateSystem.Settings>().FromInstance(moveStateSystemSettings);
			Container.Bind<AttackPlayerGoapSystem.Settings>().FromInstance(attackPlayerSettings);
			Container.Bind<SearchTargetGoapActionSystem.Settings>().FromInstance(findTargetSettings);
			Container.Bind<ReloadGoapActionSystem.Settings>().FromInstance(reloadSettings);

			// Systems
			Container.Bind<MoveStateSystem>().FromEcs();
			Container.Bind<AttackPlayerGoapSystem>().FromEcs();
			Container.Bind<RangeAttackPlayerGoapActionSystem>().FromEcs();
			Container.Bind<SearchTargetGoapActionSystem>().FromEcs();
			Container.Bind<ReloadGoapActionSystem>().FromEcs();
		}
	}
}