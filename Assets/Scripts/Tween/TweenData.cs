using DefaultNamespace;
using System;
using Unity.Entities;
using UnityEngine;

namespace Tween
{
	public enum EaseType
	{
		linear,
		easeInQuad,
		easeOutQuad,
		easeInOutQuad,
		easeInCubic,
		easeOutCubic,
		easeInOutCubic,
		easeInQuart,
		easeOutQuart,
		easeInOutQuart,
		easeInQuint,
		easeOutQuint,
		easeInOutQuint,
		easeInSine,
		easeOutSine,
		easeInOutSine,
		easeInExpo,
		easeOutExpo,
		easeInOutExpo,
		easeInCirc,
		easeOutCirc,
		easeInOutCirc,
		spring,
		easeInBounce,
		easeOutBounce,
		easeInOutBounce,
		easeInBack,
		easeOutBack,
		easeInOutBack,
		easeInElastic,
		easeOutElastic,
		easeInOutElastic
	}

	public struct TweenData : IComponentData
	{
		public bool1 Cancel;
		public bool1 Initialized;
		public float Time;
		public float Speed;
		public EaseType EaseType;
		public Entity Target;

		public TweenData(float speed, EaseType easeType, Entity target)
		{
			Time = 0;
			Speed = speed;
			EaseType = easeType;
			Target = target;
			Initialized = false;
			Cancel = false;
		}
	}

	public interface ITween : ISharedComponentData
	{
	}

	public struct TweenValueFromToData : ITween, IEquatable<TweenValueFromToData>
	{
		public float From;
		public float To;
		public Action<float> OnUpdate;

		public TweenValueFromToData(float from, float to, Action<float> onUpdate)
		{
			From = from;
			To = to;
			OnUpdate = onUpdate;
		}

		public bool Equals(TweenValueFromToData other)
		{
			return From.Equals(other.From) && To.Equals(other.To) && Equals(OnUpdate, other.OnUpdate);
		}

		public override bool Equals(object obj)
		{
			return obj is TweenValueFromToData other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = From.GetHashCode();
				hashCode = (hashCode * 397) ^ To.GetHashCode();
				hashCode = (hashCode * 397) ^ (OnUpdate != null ? OnUpdate.GetHashCode() : 0);
				return hashCode;
			}
		}
	}

	public struct TweenMoveFromToData : ITween
	{
		public Vector2 From;
		public Vector2 To;
		public Space Space;
		public bool IgnorePhysics;

		public TweenMoveFromToData(Vector2 from, Vector2 to, Space space, bool ignorePhysics = false)
		{
			IgnorePhysics = ignorePhysics;
			From = from;
			To = to;
			Space = space;
		}
	}

	public struct TweenMoveToMouseData : ITween
	{
		public Vector2 FromPosition;

		public TweenMoveToMouseData(Vector2 fromPosition, float depth)
		{
			FromPosition = fromPosition;
		}
	}

	public struct TweenRotateFromToData : ITween
	{
		public float From;
		public float To;
		public Space Space;

		public TweenRotateFromToData(float from, float to, Space space)
		{
			From = from;
			To = to;
			Space = space;
		}
	}

	public struct TweenScaleFromToData : ITween
	{
		public Vector2 From;
		public Vector2 To;

		public TweenScaleFromToData(Vector2 from, Vector2 to)
		{
			From = from;
			To = to;
		}
	}

	public struct ParticleEmissionData : ITween, IEquatable<ParticleEmissionData>
	{
		public int Rate;
		public ParticleSystem ParticleSystem;
		public Transform Target;
		public Vector2 Position;
		public float SpawnCount;

		public ParticleEmissionData(int rate, ParticleSystem particleSystem, Transform target)
		{
			Rate = rate;
			ParticleSystem = particleSystem;
			Target = target;
			Position = Vector2.zero;
			SpawnCount = 1;
		}

		public ParticleEmissionData(int rate, ParticleSystem particleSystem, Vector2 position)
		{
			Rate = rate;
			ParticleSystem = particleSystem;
			Target = null;
			Position = position;
			SpawnCount = 1;
		}

		public bool Equals(ParticleEmissionData other)
		{
			return Rate == other.Rate &&
			       Equals(ParticleSystem, other.ParticleSystem) &&
			       Equals(Target, other.Target) &&
			       Position.Equals(other.Position) &&
			       SpawnCount.Equals(other.SpawnCount);
		}

		public override bool Equals(object obj)
		{
			return obj is ParticleEmissionData other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Rate;
				hashCode = (hashCode * 397) ^ (ParticleSystem != null ? ParticleSystem.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Target != null ? Target.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ Position.GetHashCode();
				hashCode = (hashCode * 397) ^ SpawnCount.GetHashCode();
				return hashCode;
			}
		}
	}

	public struct TweenFollowPath : ITween, IEquatable<TweenFollowPath>
	{
		public Vector2[] path;
		public Space Space;

		public static TweenFollowPath Build(Vector2[] path, Space space)
		{
			var tween = new TweenFollowPath { Space = space };

			Vector2[] suppliedPath;
			Vector2[] vector3s;

			//create and store path points:
			suppliedPath = path;

			//populate calculate path;
			var offset = 2;
			vector3s = new Vector2[suppliedPath.Length + offset];
			Array.Copy(suppliedPath, 0, vector3s, 1, suppliedPath.Length);

			//populate start and end control points:
			//vector3s[0] = vector3s[1] - vector3s[2];
			vector3s[0] = vector3s[1] + (vector3s[1] - vector3s[2]);
			vector3s[vector3s.Length - 1] = vector3s[vector3s.Length - 2] + (vector3s[vector3s.Length - 2] - vector3s[vector3s.Length - 3]);

			//is this a closed, continuous loop? yes? well then so let's make a continuous Catmull-Rom spline!
			if (vector3s[1] == vector3s[vector3s.Length - 2])
			{
				var tmpLoopSpline = new Vector2[vector3s.Length];
				Array.Copy(vector3s, tmpLoopSpline, vector3s.Length);
				tmpLoopSpline[0] = tmpLoopSpline[tmpLoopSpline.Length - 3];
				tmpLoopSpline[tmpLoopSpline.Length - 1] = tmpLoopSpline[2];
				vector3s = new Vector2[tmpLoopSpline.Length];
				Array.Copy(tmpLoopSpline, vector3s, tmpLoopSpline.Length);
			}

			tween.path = vector3s;
			return tween;
		}

		public bool Equals(TweenFollowPath other)
		{
			return Equals(path, other.path) && Space == other.Space;
		}

		public override bool Equals(object obj)
		{
			return obj is TweenFollowPath other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((path != null ? path.GetHashCode() : 0) * 397) ^ (int)Space;
			}
		}
	}
}