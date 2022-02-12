using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Tween
{
	public class TweenSystem : ComponentSystem
	{
		private NativeMultiHashMap<Entity, Entity> ActiveTweens;

		protected override void OnCreate()
		{
			ActiveTweens = new NativeMultiHashMap<Entity, Entity>(8, Allocator.Persistent);
		}

		protected override void OnDestroy()
		{
			ActiveTweens.Dispose();
		}

		protected override void OnUpdate()
		{
			var deltaTime = Time.DeltaTime;

			Entities.ForEach(
				(TweenValueFromToData props, ref TweenData data) =>
				{
					if (data.Cancel)
					{
						return;
					}
					props.OnUpdate?.Invoke(Mathf.Lerp(props.From, props.To, data.EaseType.Ease(data.Time)));
				});

			Entities.ForEach(
				(TweenMoveFromToData props, ref TweenData data) =>
				{
					if (data.Cancel)
					{
						return;
					}
					var pos = Vector2.Lerp(props.From, props.To, data.EaseType.Ease(data.Time));
					if (EntityManager.HasComponent<Rigidbody2D>(data.Target))
					{
						var rigidBody = EntityManager.GetComponentObject<Rigidbody2D>(data.Target);
						if (props.IgnorePhysics)
						{
							if (props.Space == Space.World)
							{
								rigidBody.position = pos;
							}
							else
							{
								rigidBody.position = rigidBody.transform.TransformPoint(pos);
							}
						}
						else
						{
							if (props.Space == Space.World)
							{
								rigidBody.MovePosition(pos);
							}
							else
							{
								rigidBody.MovePosition(rigidBody.transform.TransformPoint(pos));
							}
						}
					}
					else if (EntityManager.HasComponent<Transform>(data.Target))
					{
						var transform = EntityManager.GetComponentObject<Transform>(data.Target);
						switch (props.Space)
						{
							case Space.World:
								transform.position = pos;
								break;

							case Space.Self:
								transform.localPosition = pos;
								break;
						}
					}
					else if (EntityManager.HasComponent<RectTransform>(data.Target))
					{
						var transform = EntityManager.GetComponentObject<RectTransform>(data.Target);
						switch (props.Space)
						{
							case Space.World:
								transform.position = pos;
								break;

							case Space.Self:
								transform.localPosition = pos;
								break;
						}
					}
				});

			Entities.ForEach(
				(TweenMoveToMouseData props, ref TweenData data) =>
				{
					if (data.Cancel)
					{
						return;
					}
					var time = data.EaseType.Ease(data.Time);
					if (EntityManager.HasComponent<RectTransform>(data.Target))
					{
						var rectTransform = EntityManager.GetComponentObject<RectTransform>(data.Target);
						rectTransform.anchoredPosition = Vector2.Lerp(props.FromPosition, Input.mousePosition, time);
					}
				});

			Entities.ForEach(
				(TweenRotateFromToData props, ref TweenData data) =>
				{
					if (data.Cancel)
					{
						return;
					}
					var angle = Mathf.LerpAngle(props.From, props.To, data.EaseType.Ease(data.Time));
					if (EntityManager.HasComponent<Rigidbody2D>(data.Target))
					{
						var rigidBody = EntityManager.GetComponentObject<Rigidbody2D>(data.Target);
						if (props.Space == Space.World)
						{
							rigidBody.MoveRotation(angle);
						}
					}
					else if (EntityManager.HasComponent<Transform>(data.Target))
					{
						var transform = EntityManager.GetComponentObject<Transform>(data.Target);
						switch (props.Space)
						{
							case Space.World:
								transform.rotation = Quaternion.Euler(0, 0, angle);
								break;

							case Space.Self:
								transform.localRotation = Quaternion.Euler(0, 0, angle);
								break;
						}
					}
					else if (EntityManager.HasComponent<RectTransform>(data.Target))
					{
						var rectTransform = EntityManager.GetComponentObject<RectTransform>(data.Target);
						switch (props.Space)
						{
							case Space.World:
								rectTransform.rotation = Quaternion.Euler(0, 0, angle);
								break;

							case Space.Self:
								rectTransform.localRotation = Quaternion.Euler(0, 0, angle);
								break;
						}
					}
				});

			Entities.ForEach(
				(TweenScaleFromToData props, ref TweenData data) =>
				{
					if (data.Cancel)
					{
						return;
					}
					var scale = Vector2.Lerp(props.From, props.To, data.EaseType.Ease(data.Time));
					if (EntityManager.HasComponent<Transform>(data.Target))
					{
						var transform = EntityManager.GetComponentObject<Transform>(data.Target);
						transform.localScale = new Vector3(scale.x, scale.y, transform.localScale.z);
					}
					else if (EntityManager.HasComponent<RectTransform>(data.Target))
					{
						var rectTransform = EntityManager.GetComponentObject<RectTransform>(data.Target);
						rectTransform.localScale = new Vector3(scale.x, scale.y, rectTransform.localScale.z);
					}
				});

			Entities.ForEach(
				(Entity entity, ParticleEmissionData props, ref TweenData data) =>
				{
					if (data.Cancel)
					{
						return;
					}
					var rate = deltaTime * props.Rate;
					props.SpawnCount += rate;
					var spawnCount = Mathf.FloorToInt(props.SpawnCount);
					if (spawnCount > 0)
					{
						props.ParticleSystem.Emit(
							new ParticleSystem.EmitParams { position = props.Target != null ? (Vector2)props.Target.position : props.Position },
							spawnCount);
						props.SpawnCount -= spawnCount;
					}

					PostUpdateCommands.SetSharedComponent(entity, props);
				});

			Entities.ForEach(
				(TweenFollowPath props, ref TweenData data) =>
				{
					var point = Interp(props.path, data.EaseType.Ease(data.Time));
					if (EntityManager.HasComponent<Rigidbody2D>(data.Target))
					{
						var rigidBody = EntityManager.GetComponentObject<Rigidbody2D>(data.Target);
						if (props.Space == Space.World)
						{
							rigidBody.MovePosition(point);
						}
						else
						{
							rigidBody.MovePosition(rigidBody.transform.TransformPoint(point));
						}
					}
					else if (EntityManager.HasComponent<Transform>(data.Target))
					{
						var transform = EntityManager.GetComponentObject<Transform>(data.Target);
						if (props.Space == Space.World)
						{
							transform.position = point;
						}
						else
						{
							transform.localPosition = point;
						}
					}
					else if (EntityManager.HasComponent<RectTransform>(data.Target))
					{
						var rectTransform = EntityManager.GetComponentObject<RectTransform>(data.Target);
						if (props.Space == Space.World)
						{
							rectTransform.position = point;
						}
						else
						{
							rectTransform.anchoredPosition = point;
						}
					}
				});

			Entities.ForEach(
				(Entity entity, ref TweenData data) =>
				{
					if (data.Cancel)
					{
						return;
					}
					if (!data.Initialized)
					{
						ActiveTweens.Add(data.Target, entity);
					}
					data.Time = Mathf.Clamp01(data.Time + deltaTime * data.Speed);
					if (data.Time >= 1)
					{
						ActiveTweens.Remove(data.Target);
						PostUpdateCommands.DestroyEntity(entity);
					}
				});
		}

		private static Vector2 Interp(Vector2[] pts, float t)
		{
			var numSections = pts.Length - 3;
			var currPt = Mathf.Min(Mathf.FloorToInt(t * numSections), numSections - 1);
			var u = t * numSections - currPt;

			var a = pts[currPt];
			var b = pts[currPt + 1];
			var c = pts[currPt + 2];
			var d = pts[currPt + 3];

			return .5f * ((-a + 3f * b - 3f * c + d) * (u * u * u) + (2f * a - 5f * b + 4f * c - d) * (u * u) + (-a + c) * u + 2f * b);
		}

		public void StopAllTweens(Entity target)
		{
			if (!ActiveTweens.TryGetFirstValue(target, out var tween, out var it))
			{
				return;
			}

			var tData = EntityManager.GetComponentData<TweenData>(tween);
			tData.Cancel = true;
			EntityManager.SetComponentData(tween, tData);

			while (ActiveTweens.TryGetNextValue(out tween, ref it))
			{
				tData = EntityManager.GetComponentData<TweenData>(tween);
				tData.Cancel = true;
				EntityManager.SetComponentData(tween, tData);
			}
		}

		public bool HasTween(Entity target)
		{
			return ActiveTweens.TryGetFirstValue(target, out _, out _);
		}
	}
}