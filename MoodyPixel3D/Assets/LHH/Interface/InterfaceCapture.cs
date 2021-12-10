using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace LHH.Interface
{
    
    public interface ICapturedInterfaceFeedback
    {
        void FeedbackNoticed(bool noticed);
    }
    
    public abstract class InterfaceCapture<T> : MonoBehaviour where T:class
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
            SelectLast,
            SelectLeastCustomValue,
            SelectMostCustomValue,
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

        abstract protected float? GetPriorityValue(T obj);

        public T GetSelected()
        {
            switch (howToSelect)
            {
                case HowToSelect.SelectFirst:
                    return FirstStack;
                case HowToSelect.SelectLast:
                    return LastStack;
                case HowToSelect.SelectLeastCustomValue:
                    return Captured.OrderBy((x) => GetPriorityValue(x).HasValue? GetPriorityValue(x).Value : float.PositiveInfinity).FirstOrDefault();
                case HowToSelect.SelectMostCustomValue:
                    return Captured.OrderByDescending((x) => GetPriorityValue(x).HasValue ? GetPriorityValue(x).Value : float.NegativeInfinity).FirstOrDefault();
                default:
                    return FirstStack;
            }
        }

        protected void AddElement(T element)
        {
            T oldSelected = GetSelected();
            Debug.LogFormat("Adding element {0} over {1}. ({2})", element, oldSelected, typeof(T));
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
            Debug.LogFormat("Noticing change between '{0}' and '{1}', of type {2}",oldObj, newObj, typeof(T));
            if(oldObj == null && newObj != null)
            {
                Debug.LogFormat("OnChange1 Selected invoking {0} gonna call someone:{1} ", newObj, OnChangeSelected != null);
                OnChangeSelected?.Invoke(newObj);
                DoFeedback(newObj, true);
                return;
            }
            else if(oldObj != null && newObj == null)
            {
                Debug.LogFormat("OnChange2 Selected invoking {0} gonna call someone:{1} ", newObj, OnChangeSelected != null);
                OnChangeSelected?.Invoke(null);
                DoFeedback(oldObj, false);
                return;
            }
            else if (!oldObj.Equals(newObj))
            {
                Debug.LogFormat("OnChange3 Selected invoking {0} gonna call someone:{1} ", newObj, OnChangeSelected != null);
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
