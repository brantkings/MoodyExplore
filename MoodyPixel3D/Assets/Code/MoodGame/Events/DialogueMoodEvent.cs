using System.Collections.Generic;
using UnityEngine;

namespace Code.MoodGame.Events
{
    [CreateAssetMenu(fileName = "Event_Dialogue_", menuName = "Mood/Event/Dialogue", order = 0)]
    public class DialogueMoodEvent : MoodEvent, MoodCheckHUD.ITalkAsset
    {
        public List<string> dialogue;
    
        protected override void Effect()
        {
            MoodCheckHUD.Instance.ShowText(this);
        }
    
        public List<string> GetDialogue()
        {
            return dialogue;
        }

        public float GetTimeBetweenChars()
        {
            return 0.05f;
        }
    }
}
