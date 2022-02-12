using Unity.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace DefaultNamespace
{
	[RequireComponent(typeof(Animator))]
	public class PlayableGraphTester : MonoBehaviour
	{
		[SerializeField] private float bounceHeight = 0.2f;
		[SerializeField] private AnimationClip[] clips;
		[SerializeField] private float fallHeight = 0.2f;
		[SerializeField] private AnimationClip idleClip;
		[SerializeField] private Transform Root;
		[SerializeField] private float speed = 1;
		[SerializeField, Range(0, 1)]  public float velocity;
		private Animator animator;
		private Avatar avatar;
		private NativeArray<float> boneWeights;
		private AnimationScriptPlayable bounceAnim;
		private PlayableGraph graph;
		private NativeArray<TransformStreamHandle> handles;
		private AnimationClipPlayable idlePlayable;
		private AnimationScriptPlayable lookAnim;
		private AnimationMixerPlayable movementMixer;
		private AnimationScriptPlayable runPlayable;

		private void Start()
		{
			var transforms = transform.GetComponentsInChildren<Transform>();
			var numTransforms = transforms.Length - 1;

			animator = GetComponent<Animator>();
			avatar = AvatarBuilder.BuildGenericAvatar(gameObject, "");
			animator.avatar = avatar;
			graph = PlayableGraph.Create("Test Graph");

			handles = new NativeArray<TransformStreamHandle>(numTransforms, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			boneWeights = new NativeArray<float>(numTransforms, Allocator.Persistent);
			for (var i = 0; i < numTransforms; i++)
			{
				handles[i] = animator.BindStreamTransform(transforms[i + 1]);
				boneWeights[i] = 1f;
			}

			var mixerJob = new MixerJob { handles = handles, boneWeights = boneWeights };

			var bounceJob = new BounceAnimJob { root = animator.BindStreamTransform(Root), defaultPos = Root.transform.localPosition };

			var lookJob = new LookAnimJob { root = animator.BindStreamTransform(Root) };

			var layerMixer = AnimationLayerMixerPlayable.Create(graph);

			runPlayable = AnimationScriptPlayable.Create(graph, mixerJob);
			runPlayable.SetProcessInputs(false);

			for (var i = 0; i < clips.Length; i++)
			{
				runPlayable.AddInput(AnimationClipPlayable.Create(graph, clips[i]), 0, 1);
			}

			movementMixer = AnimationMixerPlayable.Create(graph, 2);
			idlePlayable = AnimationClipPlayable.Create(graph, idleClip);
			movementMixer.AddInput(runPlayable, 0);
			movementMixer.AddInput(idlePlayable, 0);
			layerMixer.AddInput(movementMixer, 0, 1);

			bounceAnim = AnimationScriptPlayable.Create(graph, bounceJob);
			layerMixer.AddInput(bounceAnim, 0, 1);

			lookAnim = AnimationScriptPlayable.Create(graph, lookJob);
			layerMixer.AddInput(lookAnim, 0, 1);

			var animatorOutput = AnimationPlayableOutput.Create(graph, "Animator", animator);
			animatorOutput.SetSourcePlayable(layerMixer);
			graph.Play();
		}

		private void Update()
		{
			movementMixer.SetInputWeight(runPlayable, velocity);
			movementMixer.SetInputWeight(idlePlayable, 1 - velocity);

			if (idlePlayable.GetTime() > 1)
			{
				idlePlayable.Play();
			}

			var mixerData = runPlayable.GetJobData<MixerJob>();
			var bounceData = bounceAnim.GetJobData<BounceAnimJob>();
			var lookData = lookAnim.GetJobData<LookAnimJob>();

			mixerData.weight = Time.time * speed;
			bounceData.time = Time.time * speed;
			bounceData.bounceHeight = bounceHeight * velocity;
			bounceData.fallHeight = fallHeight * velocity;
			lookData.targetPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

			runPlayable.SetJobData(mixerData);
			bounceAnim.SetJobData(bounceData);
			lookAnim.SetJobData(lookData);

			/*
			for (int i = 0; i < clips.Length; i++)
			{
				mixer.SetInputWeight(i, currentIndex == i ? 1 - time : nextIndex == i ? time : 0);
			}
			*/
		}

		private void OnDestroy()
		{
			graph.Destroy();
			handles.Dispose();
			boneWeights.Dispose();
			DestroyImmediate(avatar);
		}

		private float EaseInOut(float t, float c = 1, float b = 0, float d = 1)
		{
			t /= d / 2;
			if (t < 1)
			{
				return c / 2 * t * t * t + b;
			}
			t -= 2;
			return c / 2 * (t * t * t + 2) + b;
		}

		public struct MixerJob : IAnimationJob
		{
			public NativeArray<TransformStreamHandle> handles;
			public NativeArray<float> boneWeights;
			public float weight;

			public void ProcessAnimation(AnimationStream stream)
			{
				var numHandles = handles.Length;
				var inputCount = stream.inputStreamCount;
				var currentIndex = Mathf.FloorToInt(weight) % inputCount;
				var nextIndex = (currentIndex + 1) % inputCount;
				var time = weight - Mathf.FloorToInt(weight);

				for (var i = 0; i < numHandles; i++)
				{
					var handle = handles[i];
					if (handle.IsValid(stream))
					{
						var s0 = stream.GetInputStream(GetWraped(currentIndex - 1, inputCount));
						var s1 = stream.GetInputStream(currentIndex);
						var s2 = stream.GetInputStream(GetWraped(currentIndex + 1, inputCount));
						var s3 = stream.GetInputStream(GetWraped(currentIndex + 2, inputCount));

						var pos0 = handle.GetLocalPosition(s0);
						var pos1 = handle.GetLocalPosition(s1);
						var pos2 = handle.GetLocalPosition(s2);
						var pos3 = handle.GetLocalPosition(s3);

						var rotA = handle.GetLocalRotation(s1);
						var rotB = handle.GetLocalRotation(s2);
						handle.SetLocalRotation(stream, Quaternion.Slerp(rotA, rotB, time * boneWeights[i]));

						var x = CubicInterpolate(pos0.x, pos1.x, pos2.x, pos3.x, time * boneWeights[i]);
						var y = CubicInterpolate(pos0.y, pos1.y, pos2.y, pos3.y, time * boneWeights[i]);

						handle.SetLocalPosition(stream, new Vector3(x, y, pos1.z));
					}
				}
			}

			public void ProcessRootMotion(AnimationStream stream)
			{
				/*var streamA = stream.GetInputStream(0);
				var streamB = stream.GetInputStream(1);

				var velocity = Vector3.Lerp(streamA.velocity, streamB.velocity, weight);
				var angularVelocity = Vector3.Lerp(streamA.angularVelocity, streamB.angularVelocity, weight);
				stream.velocity = velocity;
				stream.angularVelocity = angularVelocity;*/
			}

			private float CubicInterpolate(float y0, float y1, float y2, float y3, float mu)
			{
				float a0, a1, a2, a3, mu2;

				mu2 = mu * mu;
				a0 = y3 - y2 - y0 + y1;
				a1 = y0 - y1 - a0;
				a2 = y2 - y0;
				a3 = y1;

				return a0 * mu * mu2 + a1 * mu2 + a2 * mu + a3;
			}

			private int GetWraped(int index, int length)
			{
				var wrapedIndex = index % length;
				if (wrapedIndex < 0)
				{
					return length + wrapedIndex;
				}
				return wrapedIndex;
			}
		}

		public struct BounceAnimJob : IAnimationJob
		{
			public Vector2 defaultPos;
			public TransformStreamHandle root;
			public float time;
			public float bounceHeight;
			public float fallHeight;

			public void ProcessAnimation(AnimationStream stream)
			{
				var pos = defaultPos;
				pos += new Vector2(0, Mathf.Abs(Mathf.Sin(Mathf.PI + time * 0.25f * Mathf.PI * 2))) * bounceHeight;
				pos += new Vector2(0, Mathf.Abs(Mathf.Cos(Mathf.PI + time * 0.25f * Mathf.PI * 2))) * fallHeight;
				root.SetLocalPosition(stream, pos);
			}

			public void ProcessRootMotion(AnimationStream stream)
			{
			}
		}

		public struct LookAnimJob : IAnimationJob
		{
			public TransformStreamHandle root;
			public Vector2 targetPos;

			public void ProcessAnimation(AnimationStream stream)
			{
				Vector2 pos = root.GetPosition(stream);
				var dir = (targetPos - pos).normalized;
				var angle = Vector2.SignedAngle(Vector2.right, dir);
				root.SetLocalRotation(stream, Quaternion.Euler(0, 0, angle));
			}

			public void ProcessRootMotion(AnimationStream stream)
			{
			}
		}
	}
}