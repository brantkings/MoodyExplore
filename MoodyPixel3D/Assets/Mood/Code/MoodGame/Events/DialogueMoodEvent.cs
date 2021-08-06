using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Code.MoodGame.Events
{
    [CreateAssetMenu(fileName = "Event_Dialogue_", menuName = "Mood/Event/Dialogue", order = 0)]
    public class DialogueMoodEvent : MoodEvent, MoodCheckHUD.ITalkAsset
    {
        [System.Serializable]
        public class WhatHappen
        {
            public int line;
            public Thought toAdd;

            public override string ToString()
            {
                return $"Line {line} - {toAdd}";
            }
        }

        [System.Serializable]
        public class WhatHappenFT
        {
            public int line;
            public FlyingThoughtInstance toAdd;

            public override string ToString()
            {
                return $"Line {line} - {toAdd}";
            }
        }

        [TextArea]
        public List<string> dialogue;
        public List<WhatHappen> toHappen;
        [Tooltip("Leave destination as null")]
        public List<WhatHappenFT> flyingThoughtsDuring;
        public List<FlyingThoughtInstance> flyingThoughtsAfter;

        protected override void Effect(Transform where)
        {
            MoodCheckHUD.Instance.ShowText(where, this);
        }
    
        public List<string> GetDialogue()
        {
            return dialogue;
        }

        public float GetTimeBetweenChars()
        {
            return 0.05f;
        }

        IEnumerable<MoodCheckHUD.ITalkAsset.DialogueLine> MoodCheckHUD.ITalkAsset.GetDialogue(Transform origin)
        {
            MoodPlayerController player = MoodPlayerController.Instance;
            ThoughtSystemController thoughtSystem = player?.GetComponentInChildren<ThoughtSystemController>();

            for (int i = 0, len = dialogue.Count; i < len; i++) 
            {
                //Make the happening if something will occur
                MoodCheckHUD.ITalkAsset.DialogueLine.DelHappening happening = null;
                WhatHappen evt = toHappen?.FirstOrDefault((x) => x.line == i);
                WhatHappenFT evtFT = flyingThoughtsDuring?.FirstOrDefault((x) => x.line == i);
                //Debug.LogFormat("Dialogue {0}/{1} has line {2} and event {3}", i, len, dialogue[i], evt);
                if (evt != null)
                {
                    if (thoughtSystem != null && evt.toAdd != null)
                    {
                        happening = () =>
                        {
                            thoughtSystem.AddThought(evt.toAdd, player.Pawn);
                        };
                    }
                }
                if(evtFT != null)
                {
                    if (thoughtSystem != null)
                    {
                        happening = () =>
                        {
                            evtFT.toAdd.InstatiateFlyingThought(origin, player.Pawn.ObjectTransform);
                        };
                    }
                }

                //Yield a new dialogue line
                yield return new MoodCheckHUD.ITalkAsset.DialogueLine()
                {
                    line = dialogue[i],
                    letterToDoEvent = 0,
                    evt = happening
                };
            }

            foreach(var ft in flyingThoughtsAfter)
            {
                ft.InstatiateFlyingThought(origin, player.Pawn.ObjectTransform);
            }
        }

        public int GetAmountOfLines()
        {
            return dialogue.Count;
        }

        public override void SetInteracting(bool set)
        {
            if (!set && MoodCheckHUD.Instance.IsShowingText(this))
            {
                MoodCheckHUD.Instance.HideText();
            }
        }
    }
}
