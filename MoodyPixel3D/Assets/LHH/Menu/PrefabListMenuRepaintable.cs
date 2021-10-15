using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Menu
{
    public abstract class PrefabListMenuRepaintable<OptionViewType, OptionInfoType, RepaintParam> : PrefabListMenu<OptionViewType, OptionInfoType>, IPrefabListMenuRepaintable<RepaintParam>
        where OptionViewType : Component
        where OptionInfoType : class
    {
        public void Repaint(in RepaintParam currentState)
        {
            foreach (var opt in _options) Repaint(opt, currentState);
        }

        protected abstract void Repaint(Option option, in RepaintParam stateToRepaintWith);
    }
}