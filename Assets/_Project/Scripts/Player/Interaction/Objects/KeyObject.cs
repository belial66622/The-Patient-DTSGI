﻿using UnityEngine;
using static UnityEditorInternal.ReorderableList;

namespace ThePatient
{
    public class KeyObject : Interactable
    {
        void OnEnable()
        {
            _input.InspectExit += DestroyInspect;
        }
        private void OnDisable()
        {
            _input.InspectExit -= DestroyInspect;
        }

        public override void Interact()
        {
            Pickup();
            Inspect();
        }



        public override void OnFinishInteractEvent()
        {
            EventAggregate<InteractionTextEventArgs>.Instance.TriggerEvent(new InteractionTextEventArgs(false, ""));
        }

        public override void OnInteractEvent(string name)
        {
            EventAggregate<InteractionTextEventArgs>.Instance.TriggerEvent(
                new InteractionTextEventArgs(true, $"[ E ]\nInspect {name}"));
        }

    }


}
