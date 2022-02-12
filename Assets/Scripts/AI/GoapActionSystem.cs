using System.Collections.Generic;
using Unity.Entities;
using Zenject;

namespace AI
{
	public abstract class GoapActionInjectableSystem<T> : GoapActionSystem<T> where T : struct, IComponentData
	{
		private bool injected;

		[Inject]
		private void OnInject()
		{
			injected = true;
		}

		protected override void OnUpdate()
		{
			if (!injected)
			{
				return;
			}
			base.OnUpdate();
		}
	}

	public abstract class GoapActionSystem<T> : ComponentSystem where T : struct, IComponentData
	{
		protected override void OnUpdate()
		{
			OnBeforeUpdate();

			Entities.WithNone<GoapSharedAction>()
				.ForEach(
					(Entity entity, ref T action) =>
					{
#if UNITY_EDITOR
						EntityManager.SetName(entity, action.GetType().Name);
#endif
						OnInitializeInternal(entity, ref action);
					});

			Entities.ForEach(
				(
					Entity entity,
					GoapSharedAction goapSharedAction,
					ref GoapActiveAction activeAction,
					ref GoapProcessingAction processing,
					ref T action,
					ref GoapAction goapAction,
					ref GoapActionActor actor) =>
				{
					OnProcessInternal(entity, goapSharedAction, ref activeAction, ref processing, ref action, ref goapAction, ref actor);
				});

			Entities.ForEach(
				(
					Entity entity,
					GoapSharedAction goapSharedAction,
					ref GoapActionValidation validation,
					ref GoapAction goapAction,
					ref T action,
					ref GoapActionActor actor) =>
				{
					OnValidateInternal(entity, goapSharedAction, ref validation, ref goapAction, ref action, ref actor);
				});

			OnAfterUpdate();
		}

		private void OnInitializeInternal(Entity entity, ref T action)
		{
			var a = new GoapSharedAction { Effects = new HashSet<(GoapKeys, object)>(), Preconditions = new HashSet<(GoapKeys, object)>() };
			OnInitialize(ref action, ref a);

			PostUpdateCommands.AddSharedComponent(entity, a);
		}

		private void OnProcessInternal(
			Entity entity,
			GoapSharedAction goapSharedAction,
			ref GoapActiveAction active,
			ref GoapProcessingAction processing,
			ref T action,
			ref GoapAction goapAction,
			ref GoapActionActor actor)
		{
			if (EntityManager.Exists(actor.Actor))
			{
				OnProcess(ref action, ref goapSharedAction, goapAction, actor, ref active);
			}

			PostUpdateCommands.SetSharedComponent(entity, goapSharedAction);
		}

		private void OnValidateInternal(
			Entity entity,
			GoapSharedAction goapSharedAction,
			ref GoapActionValidation validation,
			ref GoapAction goapAction,
			ref T action,
			ref GoapActionActor actor)
		{
			if (validation.Validating)
			{
				validation.Valid = EntityManager.Exists(actor.Actor) && OnValidate(ref action, ref goapSharedAction, ref goapAction, actor);
				validation.Validating = false;
			}

			PostUpdateCommands.SetSharedComponent(entity, goapSharedAction);
		}

		protected abstract void OnInitialize(ref T action, ref GoapSharedAction goapSharedAction);

		protected abstract void OnProcess(
			ref T action,
			ref GoapSharedAction goapSharedAction,
			GoapAction goapAction,
			GoapActionActor actor,
			ref GoapActiveAction active);

		protected abstract bool OnValidate(ref T action, ref GoapSharedAction goapSharedAction, ref GoapAction goapAction, GoapActionActor actor);

		protected virtual void OnBeforeUpdate()
		{
		}

		protected virtual void OnAfterUpdate()
		{
		}
	}
}