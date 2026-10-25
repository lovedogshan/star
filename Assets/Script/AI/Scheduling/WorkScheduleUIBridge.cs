using System;
using UnityEngine;
using static Constants;

public sealed class WorkScheduleUIBridge : MonoBehaviour
{
    [SerializeField]
    private Person selectedPerson;

    public Person SelectedPerson => selectedPerson;

    public event Action<Person> OnSelectedPersonChanged;
    public event Action<Person, WorkKind, int> OnWorkPriorityChanged;
    public event Action<Person, int, ScheduleActivity> OnScheduleChanged;

    public void SelectPerson(Person person)
    {
        if (selectedPerson == person)
        {
            return;
        }

        selectedPerson = person;
        OnSelectedPersonChanged?.Invoke(selectedPerson);
    }

    public int GetWorkPriority(WorkKind workKind)
    {
        return selectedPerson ? selectedPerson.WorkProfile.GetPriority(workKind) : 0;
    }

    public void SetWorkPriority(WorkKind workKind, int priority)
    {
        if (!selectedPerson)
        {
            return;
        }

        int clamped = Mathf.Clamp(priority, 0, 4);
        selectedPerson.SetWorkPriority(workKind, clamped);
        OnWorkPriorityChanged?.Invoke(selectedPerson, workKind, clamped);
    }

    public void CycleWorkPriority(WorkKind workKind)
    {
        int current = GetWorkPriority(workKind);
        SetWorkPriority(workKind, current >= 4 ? 0 : current + 1);
    }

    public ScheduleActivity GetScheduleActivity(int slotIndex)
    {
        return selectedPerson
            ? selectedPerson.ScheduleProfile.GetActivity(slotIndex)
            : ScheduleActivity.Anything;
    }

    public void SetScheduleActivity(int slotIndex, ScheduleActivity activity)
    {
        if (!selectedPerson)
        {
            return;
        }

        selectedPerson.SetScheduleActivity(slotIndex, activity);
        OnScheduleChanged?.Invoke(selectedPerson, slotIndex, activity);
    }

    public void PaintScheduleRange(int startSlot, int endSlot, ScheduleActivity activity)
    {
        if (!selectedPerson)
        {
            return;
        }

        int first = Mathf.Clamp(Mathf.Min(startSlot, endSlot), 0, selectedPerson.ScheduleProfile.SlotCount - 1);
        int last = Mathf.Clamp(Mathf.Max(startSlot, endSlot), 0, selectedPerson.ScheduleProfile.SlotCount - 1);
        bool changed = false;
        for (int slot = first; slot <= last; slot++)
        {
            if (selectedPerson.ScheduleProfile.SetActivity(slot, activity))
            {
                changed = true;
                OnScheduleChanged?.Invoke(selectedPerson, slot, activity);
            }
        }

        if (changed)
        {
            ScheduleSystem.Instance?.NotifyProfileChanged(selectedPerson);
        }
    }
}
