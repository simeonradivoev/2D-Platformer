using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace AI.FSM
{
	public class IdleSystem : ComponentSystem
	{
		protected override void OnUpdate()
		{
			var toRemove = new NativeList<KeyValuePair<Entity, ComponentType>>(8, Allocator.Temp);
			var toAdd = new List<ValueTuple<Entity, NativeArray<GoapActionReference>>>();
			var actionValidation = GetComponentDataFromEntity<GoapActionValidation>();

			Entities.ForEach(
				(Entity entity, GoapAgentData agent, ref IdleInitializedState idleInitialized) =>
				{
					var hasValidatingFlag = false;
					for (var j = 0; j < agent.Actions.Count; j++)
					{
						if (EntityManager.Exists(agent.Actions[j]) &&
						    actionValidation.Exists(agent.Actions[j]) &&
						    actionValidation[agent.Actions[j]].Validating)
						{
							hasValidatingFlag = true;
							break;
						}
					}

					if (!hasValidatingFlag)
					{
						//remove validation component as all elements have been validated
						agent.Actions.ForEach(PostUpdateCommands.RemoveComponent<GoapActionValidation>);

						var usableActions = new List<GoapActionReference>(agent.Actions.Count);
						for (var j = 0; j < agent.Actions.Count; j++)
						{
							if (actionValidation[agent.Actions[j]].Valid)
							{
								usableActions.Add(new GoapActionReference { Entity = agent.Actions[j] });
							}
						}

						var leaves = new List<Node>();
						var worldState = new HashSet<(GoapKeys, object)>(agent.States);
						var goal = new HashSet<(GoapKeys, object)>(agent.Goals);
						var start = new Node(null, 0, worldState, new GoapActionReference { Entity = Entity.Null });
						var success = buildGraph(start, leaves, usableActions, goal);

						if (!success)
						{
							//no valid action path, reset to idle
							PostUpdateCommands.RemoveComponent<IdleInitializedState>(entity);
							PostUpdateCommands.AddComponent(entity, new IdleState());
							return;
						}

						Node cheapest = null;
						foreach (var leaf in leaves)
						{
							if (cheapest == null)
							{
								cheapest = leaf;
							}
							else
							{
								if (leaf.RunningCost < cheapest.RunningCost)
								{
									cheapest = leaf;
								}
							}
						}

						// get its node and work back through the parents
						var result = new List<GoapActionReference>();
						var n = cheapest;
						while (n != null)
						{
							if (n.Action.Entity != Entity.Null)
							{
								result.Insert(0, n.Action); // insert the action in the front
							}
							n = n.Parent;
						}

						//start processing the action chain
						PostUpdateCommands.RemoveComponent<IdleInitializedState>(entity);
						PostUpdateCommands.AddComponent(entity, new PerformActionState());
						var ar = new NativeArray<GoapActionReference>(result.Count, Allocator.Temp);
						for (var j = 0; j < result.Count; j++)
						{
							if (j == 0)
							{
								PostUpdateCommands.AddComponent(result[j].Entity, new GoapActiveAction());
							}
							ar[j] = result[j];
						}

						toAdd.Add(new ValueTuple<Entity, NativeArray<GoapActionReference>>(entity, ar));
					}
				});

			Entities.ForEach(
				(Entity agentEntity, GoapAgentData agent, ref IdleState idleState) =>
				{
					for (var j = 0; j < agent.Actions.Count; j++)
					{
						var actionEntity = agent.Actions[j];
						//only add validation if none is present
						if (!actionValidation.Exists(actionEntity))
						{
							var validationData = new GoapActionValidation { Validating = true };
							PostUpdateCommands.AddComponent(actionEntity, validationData);
							//reset the action
							PostUpdateCommands.SetComponent(actionEntity, new GoapAction());
							if (EntityManager.HasComponent<GoapActiveAction>(actionEntity))
							{
								PostUpdateCommands.RemoveComponent<GoapActiveAction>(actionEntity);
							}
							if (EntityManager.HasComponent<GoapProcessingAction>(actionEntity))
							{
								PostUpdateCommands.RemoveComponent<GoapProcessingAction>(actionEntity);
							}
						}
					}

					PostUpdateCommands.RemoveComponent<IdleState>(agentEntity);
					PostUpdateCommands.AddComponent(agentEntity, new IdleInitializedState());
				});

			for (var i = 0; i < toRemove.Length; i++)
			{
				EntityManager.RemoveComponent(toRemove[i].Key, toRemove[i].Value);
			}

			for (var i = 0; i < toAdd.Count; i++)
			{
				EntityManager.AddBuffer<GoapActionReference>(toAdd[i].Item1);
				var ar = EntityManager.GetBuffer<GoapActionReference>(toAdd[i].Item1);
				ar.CopyFrom(toAdd[i].Item2);
				toAdd[i].Item2.Dispose();
			}

			toRemove.Dispose();
		}

		private bool buildGraph(Node parent, List<Node> leaves, List<GoapActionReference> usableActions, HashSet<(GoapKeys, object)> goal)
		{
			var foundOne = false;

			// go through each action available at this node and see if we can use it here
			for (var i = 0; i < usableActions.Count; i++)
			{
				var actionReference = usableActions[i];
				var action = EntityManager.GetSharedComponentData<GoapSharedAction>(actionReference.Entity);

				// if the parent state has the conditions for this action's preconditions, we can use it here
				if (inState(action.Preconditions, parent.State))
				{
					// apply the action's effects to the parent state
					var currentState = populateState(parent.State, action.Effects);
					//Debug.Log(GoapAgent.prettyPrint(currentState));
					var node = new Node(parent, parent.RunningCost + action.Cost, currentState, actionReference);

					if (inState(goal, currentState))
					{
						// we found a solution!
						leaves.Add(node);
						foundOne = true;
					}
					else
					{
						// not at a solution yet, so test all the remaining actions and branch out the tree
						var subset = actionSubset(usableActions, actionReference);
						var found = buildGraph(node, leaves, subset, goal);
						if (found)
						{
							foundOne = true;
						}
					}
				}
			}

			return foundOne;
		}

		/**
		 * Create a subset of the actions excluding the removeMe one. Creates a new set.
		 */
		private List<GoapActionReference> actionSubset(List<GoapActionReference> actions, GoapActionReference removeMe)
		{
			var subset = new List<GoapActionReference>();
			foreach (var a in actions)
			{
				if (!a.Equals(removeMe))
				{
					subset.Add(a);
				}
			}
			return subset;
		}

		/**
		 * Check that all items in 'test' are in 'state'. If just one does not match or is not there
		 * then this returns false.
		 */
		private bool inState(HashSet<(GoapKeys, object)> test, HashSet<(GoapKeys, object)> state)
		{
			var allMatch = true;
			foreach (var t in test)
			{
				var match = false;
				foreach (var s in state)
				{
					if (s.Equals(t))
					{
						match = true;
						break;
					}
				}

				if (!match)
				{
					allMatch = false;
				}
			}

			return allMatch;
		}

		/**
	 * Apply the stateChange to the currentState
	 */
		private HashSet<(GoapKeys, object)> populateState(HashSet<(GoapKeys, object)> currentState, HashSet<(GoapKeys, object)> stateChange)
		{
			var state = new HashSet<(GoapKeys, object)>();
			// copy the KVPs over as new objects
			foreach (var s in currentState)
			{
				state.Add((s.Item1, s.Item2));
			}

			foreach (var change in stateChange)
			{
				// if the key exists in the current state, update the Value
				var exists = false;

				foreach (var s in state)
				{
					if (s.Equals(change))
					{
						exists = true;
						break;
					}
				}

				if (exists)
				{
					state.RemoveWhere(kvp => kvp.Item1.Equals(change.Item1));
					var updated = (change.Item1, change.Item2);
					state.Add(updated);
				}
				// if it does not exist in the current state, add it
				else
				{
					state.Add((change.Item1, change.Item2));
				}
			}

			return state;
		}

		private class Node
		{
			public readonly GoapActionReference Action;
			public readonly Node Parent;
			public readonly float RunningCost;
			public readonly HashSet<(GoapKeys, object)> State;

			public Node(Node parent, float runningCost, HashSet<(GoapKeys, object)> state, GoapActionReference action)
			{
				Parent = parent;
				RunningCost = runningCost;
				State = state;
				Action = action;
			}
		}
	}
}