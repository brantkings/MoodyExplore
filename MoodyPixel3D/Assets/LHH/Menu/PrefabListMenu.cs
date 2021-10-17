using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace LHH.Menu
{
    public interface IPrefabListMenu
    {
        void MoveSelection(int movement);

        void SelectCurrent();
    }

    public interface IPrefabListMenuRepaintable<StateType> : IPrefabListMenu
    {
        void Repaint(in StateType param);
    }

    public abstract class PrefabListMenu<OptionViewType, OptionInfoType> : MonoBehaviour , IPrefabListMenu
        where OptionViewType : Component 
        where OptionInfoType : class
    {
        public OptionViewType optionPrefab;

        public class Option
        {
            public OptionViewType currentOptionView;
            public OptionInfoType currentInformation;

            public Option(OptionViewType view, OptionInfoType info)
            {
                currentInformation = info;
                currentOptionView = view;
            }
        }

        protected List<Option> _options = new List<Option>(8);

        private Option CreateNewOption(OptionInfoType type)
        {
            OptionViewType view = GetNewInstance();
            PopulateInstance(ref view, type);
            Option opt = new Option(view, type);
            return opt;
        }

        public void RepopulateWithDifferences()
        {
            if (CurrentOption != null) SetSelected(CurrentOption, false);

            int index = 0;
            int length = GetOptionsPopulationLength();
            foreach (OptionInfoType type in GetOptionsPopulation())
            {
                int result = _options.FindIndex(0, (x) => x.currentInformation == type);
                if(result >= 0) //Found any
                {
                    if(result == index) //No need to reposition
                    {
                        //Noop
                    }
                    else //Need to change
                    {
                        Option toChange = _options[index];
                        _options[index] = _options[result];
                        _options[result] = toChange;
                        Reposition(_options[result], result, length, false);
                        Reposition(_options[index], index, length, false);
                    }
                }
                else //OK will need to create
                {
                    Option opt = CreateNewOption(type);
                    Option alreadyThere = _options.ElementAtOrDefault(index);
                    if(alreadyThere != null) //There is a different one there
                    {
                        _options[index] = opt;
                        _options.Add(alreadyThere);
                    }
                    else
                    {
                        _options.Add(opt);
                        Reposition(opt, index, length, true);
                    }
                }

                //Always up index in the foreach no matter the outcome
                index++;
            }

            if (CurrentOption != null) SetSelected(CurrentOption, true);

        }

        public int _currentSelection;

        public Option CurrentOption
        {
            get
            {
                if (_currentSelection >= OptionLength || _currentSelection < 0) return null;
                return _options[_currentSelection];
            }
        }

        public int OptionLength
        {
            get
            {
                return _options.Count;
            }
        }

        public abstract int GetOptionsPopulationLength();
        public abstract IEnumerable<OptionInfoType> GetOptionsPopulation();
        public virtual OptionViewType GetNewInstance()
        {
            return Instantiate<OptionViewType>(optionPrefab);
        }

        public abstract void PopulateInstance(ref OptionViewType instance, OptionInfoType origin);

        public abstract void Reposition(Option option, int index, int length, bool justCreated);

        public void MoveSelection(int movement)
        {
            SetSelectedIfNotNull(CurrentOption, false);
            Debug.LogFormat("{0} + {1} = {2} (L:{3})", _currentSelection, movement, Mathf.FloorToInt(Mathf.Repeat(_currentSelection + movement, OptionLength)), OptionLength);
            _currentSelection = Mathf.FloorToInt(Mathf.Repeat(_currentSelection + movement, OptionLength));
            SetSelectedIfNotNull(CurrentOption, true);
        }

        public void SelectCurrent()
        {
            if(CurrentOption != null) 
                Select(CurrentOption);
        }

        private void SetSelectedIfNotNull(Option option, bool selected)
        {
            if (option != null) SetSelected(option, selected);
        }


        protected abstract void SetSelected(Option option, bool selected);
        protected abstract void Select(Option option);


    }
}