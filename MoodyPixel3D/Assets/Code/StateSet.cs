using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public abstract class StateSet<T> : MonoBehaviour where T : Behaviour
{
    [System.Serializable]
    public abstract class StateSetup
    {
        public string name;
        public int priority;

#if UNITY_EDITOR
        [SerializeField]
        [ReadOnly]
#endif
        private int _votes;

        public int Votes
        {
            private set
            {
                _votes = value;
            }
            get
            {
                return _votes;
            }
        }

        public void AddVote()
        {
            Votes++;
        }

        public void RemoveVote()
        {
            Votes--;
            if (Votes < 0) Votes = 0;
        }

        public bool HasVotes()
        {
            return Votes > 0;
        }

        public abstract void SetState(T type);
    }

    private int _totalVotes;

    private T _thing;

    [SerializeField]
    protected abstract StateSetup[] States
    {
        get;
    }

    public string initialState;

#if UNITY_EDITOR
    [SerializeField]
    [ReadOnly]
    [Space()]
    private string _chosenState;
#endif

    public void Awake()
    {
        _thing = GetComponent<T>();
        if(_thing == null)
        {
            Debug.LogErrorFormat("{0} has no {1} to change states in the same game object!", this, typeof(T).ToString());
        }
    }

    public void ApplyState(string id)
    {
        ApplyState(States, id, _thing);
    }

    public void RemoveVoteForState(string id)
    {
        RemoveVoteForState(States, id, _thing);
    }

    public void VoteForState(string id)
    {
        VoteForState(States, id, _thing);
    }

    private void Start()
    {
        if (initialState != string.Empty) ApplyState(initialState);
    }

    private StateSetup FindState(IEnumerable<StateSetup> states, string id)
    {
        return states.FirstOrDefault((x) => x.name == id);
    }


    protected void ApplyState(IEnumerable<StateSetup> states, string id, T toApply)
    {
        StateSetup state = FindState(states, id);
        if (state != null)
        {
#if UNITY_EDITOR
            _chosenState = id + " -> by direct applying";
#endif
            state.SetState(toApply);
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogErrorFormat("{0} trying to apply state with id '{1}' that does not exist.", this, id);
        }
#endif
    }

    /// <summary>
    /// Add a vote for state x. If it is the most voted with the most priority, it will be chosen. This is to control many systems trying to control the same walker. Remember to remove the vote.
    /// </summary>
    /// <param name="id"></param>
    protected void VoteForState(IEnumerable<StateSetup> states, string id, T toApply)
    {
        StateSetup state = FindState(states, id);
        if (state != null)
        {
            state.AddVote();
            _totalVotes++;

            FindMostVotedState(states, true).SetState(toApply);
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogErrorFormat("{0} trying to vote for state with id '{1}' that does not exist.", this, id);
        }
#endif
    }

    /// <summary>
    /// Remove the vote so other states might be chosen.
    /// </summary>
    /// <param name="id"></param>
    protected void RemoveVoteForState(IEnumerable<StateSetup> states, string id, T toApply)
    {
        StateSetup state = FindState(states, id);
        if (state != null)
        {
            state.RemoveVote();
            _totalVotes--;
            if (_totalVotes < 0) _totalVotes = 0;

            FindMostVotedState(states, _totalVotes > 0).SetState(toApply);
        }
#if UNITY_EDITOR
        else
        {
            Debug.LogErrorFormat("{0} trying to remove vote for state with id '{1}' that does not exist.", this, id);
        }
#endif
    }

    private StateSetup FindMostVotedState(IEnumerable<StateSetup> states, bool hasVotes)
    {
        StateSetup state;
        if (!hasVotes)
        {
            state = states.OrderByDescending(x => x.priority).First();

#if UNITY_EDITOR
            _chosenState = state.name + " -> by priority";
#endif

            return state;
        }
        else
        {
            state = states.OrderByDescending(x => x.HasVotes() ? x.priority : -1).First();
            int priority = state.priority;
            int votes = state.Votes;
            foreach (StateSetup due in states)
            {
                if (due.priority != priority)
                {
#if UNITY_EDITOR
                    _chosenState = state.name + " -> by votes in priority";
#endif
                    return state;
                }

                if (due.Votes > votes)
                {
                    state = due;
                    votes = due.Votes;
                }
            }

#if UNITY_EDITOR
            _chosenState = state.name + " -> by votes";
#endif
            return state;
        }
    }
}
