using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Structures
{
    
    public interface ICapturedInterfaceFeedback
    {
        void FeedbackNoticed(bool noticed);
    }
    
    public class InterfaceCapture<T> : MonoBehaviour where T:class
    {
        public delegate void DelChanged(T newFirst);

        public event DelChanged OnChangeSelected;

        private LinkedList<T> _captured;

        public enum HowToAdd
        {
            AddFirst,
            AddLast
        }

        public enum HowToSelect
        {
            SelectFirst,
            SelectLast
        }

        public HowToAdd howToAdd = HowToAdd.AddFirst;
        public HowToSelect howToSelect = HowToSelect.SelectFirst;

        protected LinkedList<T> Captured
        {
            get { return _captured ??= new LinkedList<T>(); }
        }
        
        public T FirstStack => Captured.First != null? Captured.First.Value : null;
        public T LastStack => Captured.Last != null? Captured.Last.Value : null;
        public int Count => Captured.Count;

        public T GetSelected()
        {
            switch (howToSelect)
            {
                case HowToSelect.SelectFirst:
                    return FirstStack;
                case HowToSelect.SelectLast:
                    return LastStack;
                default:
                    return FirstStack;
            }
        }

        protected void AddElement(T element)
        {
            T oldSelected = GetSelected();
            if (element != null)
            {
                switch (howToAdd)
                {
                    case HowToAdd.AddFirst:
                        Captured.AddFirst(element);
                        break;
                    case HowToAdd.AddLast:
                        Captured.AddLast(element);
                        break;
                    default:
                        break;
                }
            }

            T newSelected = GetSelected();
            NoticeChange(oldSelected, newSelected);
        }

        protected void RemoveElement(T element)
        {
            T oldSelected = GetSelected();
            if (element != null)
            {
                Captured.Remove(element);
            }

            T newSelected = GetSelected();
            NoticeChange(oldSelected, newSelected);
        }

        private void NoticeChange(T oldObj, T newObj)
        {
            if(oldObj == null && newObj != null)
            {
                OnChangeSelected?.Invoke(newObj);
                DoFeedback(newObj, true);
                return;
            }
            else if(oldObj != null && newObj == null)
            {
                OnChangeSelected?.Invoke(null);
                DoFeedback(oldObj, false);
                return;
            }
            else if (!oldObj.Equals(newObj))
            {
                OnChangeSelected?.Invoke(newObj);
                DoFeedback(oldObj, false);
                DoFeedback(newObj, true);
            }
        }

        private void DoFeedback(T obj, bool feedback)
        {
            if (obj != null && obj is ICapturedInterfaceFeedback)
            {
                (obj as ICapturedInterfaceFeedback).FeedbackNoticed(feedback);
            }
        }
        

    }
}
