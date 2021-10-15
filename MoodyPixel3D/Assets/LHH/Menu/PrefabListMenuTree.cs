using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Menu
{
    public interface ICategoryPriority
    {
        int GetPriority();
    }

    /// <summary>
    /// Not implemented yet.
    /// </summary>
    /// <typeparam name="Ov">Option view type</typeparam>
    /// <typeparam name="Oi">Option information type</typeparam>
    /// <typeparam name="OptionCategory">Category to categorize options</typeparam>
    public abstract class PrefabListMenuTree<Ov, Oi, OptionCategory> : PrefabListMenu<Ov, PrefabListMenuTree<Ov, Oi, OptionCategory>.TreeInfo>
        where Ov : Component
        where Oi : class
    {
        public class TreeInfo
        {
            public Oi info;
            public OptionCategory category;
            public LinkedList<Option> children;

            public TreeInfo(Oi info, OptionCategory category)
            {
                this.info = info;
                children = new LinkedList<Option>();
            }
        }

        public abstract IEnumerable<Oi> GetAllOptionsPopulation();

        public abstract OptionCategory GetCategoryFrom(Oi oi);

        public override void PopulateInstance(ref Ov instance, TreeInfo origin)
        {
            foreach(var view in origin.children)
            {
                PopulateInstance(ref view.currentOptionView, origin.info);
            }
        }

        public abstract void PopulateInstance(ref Ov instance, Oi info);

        public abstract Ov InstantiateFrom(Oi oi);

        public override IEnumerable<TreeInfo> GetOptionsPopulation()
        {
            throw new NotImplementedException();

            Dictionary<OptionCategory, Option> dic = new Dictionary<OptionCategory, Option>(8);
            LinkedList<Option> categoryLessSkills = new LinkedList<Option>();

            /*
            foreach (Oi oi in GetAllOptionsPopulation())
            {
                OptionCategory cat = GetCategoryFrom(oi);
                TreeInfo treeInfo = new TreeInfo(oi);
                Option theOption = new Option(InstantiateFrom(oi), treeInfo);
                if (cat != null)
                {
                    if (!dic.ContainsKey(cat))
                    {
                        dic.Add(cat, new OptionColumn(cat));
                    }
                    theOption.parent = dic[cat];
                    dic[cat].options.Add(theOption);
                }
                else
                {
                    categoryLessSkills.Add(theOption);
                }
            }
            foreach (var category in dic.Keys.OrderByDescending((x) => x.GetPriority()))
            {
                Option categoryOption = new Option(main, category, null, dic[category]);
                dic[category].SetParent(categoryOption);
                main.options.Add(categoryOption); //Make an option for each category
            }
            foreach (var s in categoryLessSkills)
            {
                main.options.Add(s);
            }

            MakeInstances(main);

            if (!current.IsExistingOption()) current.Set(main, 0);
            if (current.Validate())
            {
                StartCoroutine(SelectAndActivateTreeFeedbackRoutine(changeDuration));
            }

            current.SelectCurrent();*/
        }

        protected override void Select(Option option)
        {
            //TODO
        }
    }
}