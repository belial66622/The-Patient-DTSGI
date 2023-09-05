using System;
using System.Collections;
using System.Collections.Generic;
using ThePatient;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Android;
using Utilities;

public class DoorObject : Interactable
{
    enum DoorState
    {
        Closed,
        Opened,
        Locked
    }

    [Header("Reference")]
    [SerializeField] Transform doorPivot;
    [SerializeField] DoorState doorState = DoorState.Closed;
    [SerializeField] GameObject requiredKey;

    [Header("Door Settings")]
    [SerializeField] Vector3 defaultRotAxis = Vector3.forward;
    [SerializeField] float closedRotation;
    [SerializeField] float openRotation = 120;
    [SerializeField] float rattleRotation = 1;
    [SerializeField] float openOrCloseDuration = .7f;
    [SerializeField] float rattleDuration = .05f;
    [SerializeField] float cooldownTime = .5f;
    [SerializeField] float lockDuration = .5f;

    CountdownTimer doorCooldownTimer;
    CountdownTimer rattleTimer;
    CountdownTimer openOrCloseTimer;
    CountdownTimer lockTimer;

    Quaternion defaultRotation;

    IPickupable key;
    bool needKey = false;
    AudioSource _spatialsound;
    SoundUtils sound;

    protected override void Start()
    {
        _spatialsound = this.AddComponent<AudioSource>();
        defaultRotation = doorPivot.localRotation;
        doorCooldownTimer = new CountdownTimer(cooldownTime);
        openOrCloseTimer = new CountdownTimer(openOrCloseDuration);
        rattleTimer = new CountdownTimer(rattleDuration);
        lockTimer = new CountdownTimer(lockDuration);
        sound = new SoundUtils();

        lockTimer.OnTimerStop += () => {
            if (needKey)
            {
                Interact();
                OnFinishInteractEvent();
                doorCooldownTimer.Start();  
            }
        };
        lockTimer.OnTimerPause += () => OnFinishInteractEvent();
        doorCooldownTimer.OnTimerStart += () => OnFinishInteractEvent();

        needKey = requiredKey != null;
    
        if (needKey)
            key = requiredKey.GetComponent<IPickupable>();
    }

    protected override void Update()
    {
        doorCooldownTimer.Tick(Time.deltaTime);
        openOrCloseTimer.Tick(Time.deltaTime);
        lockTimer.Tick(Time.deltaTime);
        rattleTimer.Tick(Time.deltaTime);

        if(OnHold && !lockTimer.IsRunning && !doorCooldownTimer.IsRunning && needKey && Inventory.Instance.HasItem(key))
        {
            lockTimer.Start();
        }
        else if(!OnHold && lockTimer.IsRunning && lockTimer.Progress > 0)
        {
            lockTimer.Pause();
        }
        else if(OnHold && lockTimer.IsRunning && lockTimer.Progress <= 0)
        {
            lockTimer.Stop();
        }
        Debug.Log("Progress: " + lockTimer.Progress);
        Debug.Log("Inverse : " + lockTimer.InverseProgress);
        
    }

    public override bool Interact()
    {
        HandleDoorState(doorState);
        return false;
    }

    private void HandleDoorState(DoorState state)
    {
        if(openOrCloseTimer.IsRunning) return;

        switch (state)
        {
            case DoorState.Closed:
                if (lockTimer.Progress <= 0 && needKey)
                {
                    if (doorCooldownTimer.IsRunning) return;
                    sound.PlaySound(AudioManager.Instance.GetSfx("LockDoor"),_spatialsound);
                    doorState = DoorState.Locked;
                }
                else
                {
                    sound.PlaySound(AudioManager.Instance.GetSfx("Door"),_spatialsound);
                    StartCoroutine(ToggleDoor(closedRotation, openRotation, openOrCloseTimer));
                    doorState = DoorState.Opened;
                }
                break;
            case DoorState.Opened:
                sound.PlaySound(AudioManager.Instance.GetSfx("Door"),_spatialsound);
                StartCoroutine(ToggleDoor(openRotation, closedRotation, openOrCloseTimer));
                doorState = DoorState.Closed;
                break;
            case DoorState.Locked:
                if (doorCooldownTimer.IsRunning) return;
                if (lockTimer.Progress <= 0 && needKey)
                {
                        if (gameObject.TryGetComponent<QuestObjectiveObject>
                    (out QuestObjectiveObject questObjectiveObject))
                        {
                            if (questObjectiveObject.GetQuest().IsActive)
                            {
                                if(doorState == DoorState.Closed || doorState == DoorState.Opened)
                                {
                                    questObjectiveObject.CompleteObjective();
                                    gameObject.GetComponent<Collider>().enabled = false;
                                }
                            }
                        }

                    sound.PlaySound(AudioManager.Instance.GetSfx("UnlockDoor"),_spatialsound);
                    doorState = DoorState.Closed;
                }
                else
                {
                    sound.PlaySound(AudioManager.Instance.GetSfx("RattleDoor"), _spatialsound);
                    StartCoroutine(RattleDoorRepeat());
                    doorCooldownTimer.Start();
                }
                break;
        }
    }



    IEnumerator ToggleDoor(float startRot, float targetRot, TimerUtils time)
    {
        time.Start();

        Quaternion startRotation = Quaternion.AngleAxis(startRot, defaultRotAxis );
        Quaternion targetRotation = Quaternion.AngleAxis(targetRot, defaultRotAxis );



        while (time.InverseProgress < 1)
        {
            float t = time.InverseProgress * time.InverseProgress * (3 - 2 * time.InverseProgress);

            var currentRot = Quaternion.Slerp(startRotation, targetRotation, t);

            doorPivot.localRotation = defaultRotation  * currentRot;
            yield return null;
        }

        doorPivot.localRotation = defaultRotation * targetRotation;

        time.Stop();
    }
    IEnumerator RattleDoorRepeat()
    {
        for (int i = 0; i < 4; i++)
        {
            StartCoroutine(RattleDoorOnce());
            yield return new WaitForSeconds(.125f);
        }
    }
    IEnumerator RattleDoorOnce()
    {
        //audioSource.PlayOneShot(rattleAudio);
        StartCoroutine(ToggleDoor(closedRotation, rattleRotation, rattleTimer));
        yield return new WaitForSeconds(.075f);
        StartCoroutine(ToggleDoor(rattleRotation, closedRotation, rattleTimer));
        yield return new WaitForSeconds(.075f);
    }

    public override void OnFinishInteractEvent()
    {
        EventAggregate<InteractionTextEventArgs>.Instance.TriggerEvent(new InteractionTextEventArgs(false, ""));
        EventAggregate<InteractionLockUIEventArgs>.Instance.TriggerEvent(new InteractionLockUIEventArgs(false, 0));
    }

    

    public override void OnInteractEvent(string objectName)
    {
        EventAggregate<InteractionTextEventArgs>.Instance.TriggerEvent(new InteractionTextEventArgs(true,
            GetText(doorState, objectName)));

        if (lockTimer.IsRunning && lockTimer.InverseProgress >= .2f && lockTimer.Progress > 0 && doorState != DoorState.Opened)
        {
            EventAggregate<InteractionLockUIEventArgs>.Instance.TriggerEvent(new InteractionLockUIEventArgs(true, lockTimer.InverseProgress));
        }
    }
    string GetText(DoorState state, string name)
    {
        if (state == DoorState.Locked)
        {
            if (Inventory.Instance.HasItem(key))
            {
                return $"[ HOLD E ]\nUnlock {name}";
            }
            else
            {
                return "Required Key";
            }
        }
        return $"[ E ]\nInteract With {name}";
    }
}
