using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LHH.Menu
{
    public class PrefabListMenuTester : MonoBehaviour
    {
        IPrefabListMenu _menu;

        public KeyCode next1 = KeyCode.UpArrow;
        public KeyCode prev1 = KeyCode.DownArrow;
        public KeyCode next5 = KeyCode.RightArrow;
        public KeyCode prev5 = KeyCode.LeftArrow;
        public KeyCode confirmCode = KeyCode.Space;

        private void Awake()
        {
            _menu = GetComponentInChildren<IPrefabListMenu>();
            if(_menu == null)
            {
                Debug.LogErrorFormat(this, "{0} not found an {1}", this, typeof(IPrefabListMenu));
            }
        }

        private void Update()
        {
            GetInput(out int move, out bool confirmed);

            if (move != 0) _menu.MoveSelection(move);
            if (confirmed) _menu.SelectCurrent();
        }

        private void GetInput(out int movement, out bool confirmed)
        {
            movement = 0;
            if(Input.GetKeyDown(next1))
            {
                movement += 1;
            }
            if (Input.GetKeyDown(next5))
            {
                movement += 5;
            }
            if (Input.GetKeyDown(prev1))
            {
                movement -= 1;
            }
            if (Input.GetKeyDown(prev5))
            {
                movement -= 5;
            }
            confirmed = Input.GetKeyDown(confirmCode);
        }
    }
}