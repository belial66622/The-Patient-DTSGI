﻿using UnityEngine;

namespace ThePatient
{
    public class KeyObject : Interactable
    {
        private void Update()
        {
        }

        public override void Interact()
        {
            if (!OnHold)
            {
                // do something
                Debug.Log("mirror");
            }
        }

        public override void OnFinishInteractEvent()
        {

        }

        public override void OnInteractEvent(string name)
        {

        }

    }


}
