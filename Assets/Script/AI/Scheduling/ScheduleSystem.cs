using System;
using System.Collections.Generic;
using UnityEngine;
using static Constants;

public enum ScheduleActivity
{
    Anything,
    Work,
    Sleep,
    Recreation,
    Medical
}

[Serializable]
public struct ScheduleSlot
{
    [SerializeField]
    private ScheduleActivity activity;

    public ScheduleActivity Activity
    {
        get => activity;
        set => activity = value;
    }

    public ScheduleSlot(ScheduleActivity activity)
    {
        this.activity = activity;
    }
}

[Serializable]
public sealed class PersonScheduleProfile
{
    public const int DefaultSlotsPerDay = 24;

    [SerializeField]
    private ScheduleSlot[] slots = new ScheduleSlot[DefaultSlotsPerDay];

    public int SlotCount
    {
        get
        {
            EnsureSize(DefaultSlotsPerDay);
            return slots.Length;
        }
    }

    public ScheduleActivity GetActivity(int slotIndex)
    {
        EnsureSize(DefaultSlotsPerDay);
        return slots[Mathf.Clamp(slotIndex, 0, slots.Length - 1)].Activity;
    }

    public bool SetActivity(int slotIndex, ScheduleActivity activity)
    {
        EnsureSize(DefaultSlotsPerDay);
        int index = Mathf.Clamp(slotIndex, 0, slots.Length - 1);
        if (slots[index].Activity == activity)
        {
            return false;
        }

        slots[index] = new ScheduleSlot(activity);
        return true;
    }

    public void Fill(ScheduleActivity activity)
    {
        EnsureSize(DefaultSlotsPerDay);
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new ScheduleSlot(activity);
        }
    }

    private void EnsureSize(int requiredSize)
    {
        if (slots != null && slots.Length == requiredSize)
        {
            return;
        }

        ScheduleSlot[] previous = slots;
        slots = new ScheduleSlot[requiredSize];
        if (previous == null)
        {
            return;
        }

        int copyCount = Mathf.Min(previous.Length, slots.Length);
        Array.Copy(previous, slots, copyCount);
    }
}

public sealed class ScheduleSystem : MonoBehaviour
{
    private readonly Dictionary<Person, ScheduleActivity> currentActivities =
        new Dictionary<Person, ScheduleActivity>();

    [SerializeField]
    private Timesystem timeSource;
    [SerializeField, Min(1f)]
    private float fallbackSecondsPerSlot = 60f;

    private int currentSlot = -1;
    private int currentDay = -1;

    public static ScheduleSystem Instance { get; private set; }
    public int CurrentSlot => currentSlot < 0 ? 0 : currentSlot;

    public event Action<Person, ScheduleActivity, ScheduleActivity> OnActivityChanged;

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ResolveTimeSource();
        RefreshCurrentSlot(true);
    }

    private void Update()
    {
        RefreshCurrentSlot(false);
    }

    public void Register(Person person)
    {
        if (!person)
        {
            return;
        }

        ScheduleActivity activity = GetActivity(person);
        currentActivities[person] = activity;
        person.IBehavior?.ApplyScheduleActivity(activity);
    }

    public void Unregister(Person person)
    {
        if (person)
        {
            currentActivities.Remove(person);
        }
    }

    public ScheduleActivity GetActivity(Person person)
    {
        return person ? person.ScheduleProfile.GetActivity(CurrentSlot) : ScheduleActivity.Anything;
    }

    public bool AllowsWork(Person person, WorkKind workKind, bool isEmergency = false)
    {
        if (!person || isEmergency)
        {
            return person;
        }

        ScheduleActivity activity = GetActivity(person);
        switch (activity)
        {
            case ScheduleActivity.Anything:
            case ScheduleActivity.Work:
                return true;
            case ScheduleActivity.Medical:
                return workKind == WorkKind.MedicalCare;
            default:
                return false;
        }
    }

    public bool AllowsIdleWander(Person person)
    {
        ScheduleActivity activity = GetActivity(person);
        return activity == ScheduleActivity.Anything ||
               activity == ScheduleActivity.Work ||
               activity == ScheduleActivity.Recreation;
    }

    public void NotifyProfileChanged(Person person)
    {
        if (!person)
        {
            return;
        }

        ApplyActivity(person, GetActivity(person), true);
        JobSystem.Instance?.NotifyPersonChanged(person);
        TaskController.Instance?.RequestAssignment(person);
    }

    public void RefreshNow()
    {
        RefreshCurrentSlot(true);
    }

    private void ResolveTimeSource()
    {
        if (!timeSource)
        {
            timeSource = FindObjectOfType<Timesystem>();
        }
    }

    private void RefreshCurrentSlot(bool force)
    {
        ResolveTimeSource();
        int nextSlot = CalculateSlot();
        int nextDay = timeSource ? timeSource.dayTime : 0;
        if (!force && nextSlot == currentSlot && nextDay == currentDay)
        {
            return;
        }

        currentSlot = nextSlot;
        currentDay = nextDay;

        if (!TaskController.Instance)
        {
            return;
        }

        List<Person> people = TaskController.Instance.People;
        for (int i = 0; i < people.Count; i++)
        {
            Person person = people[i];
            if (person)
            {
                ApplyActivity(person, GetActivity(person), force);
            }
        }
    }

    private int CalculateSlot()
    {
        if (!timeSource)
        {
            return Mathf.FloorToInt(Time.timeSinceLevelLoad / fallbackSecondsPerSlot) %
                   PersonScheduleProfile.DefaultSlotsPerDay;
        }

        int hour = Mathf.Clamp(timeSource.showMinute, 0, 23);
        int minute = Mathf.Clamp(timeSource.showSeconds, 0, 59);
        int minuteOfDay = hour * 60 + minute;
        return Mathf.Clamp(
            minuteOfDay * PersonScheduleProfile.DefaultSlotsPerDay / (24 * 60),
            0,
            PersonScheduleProfile.DefaultSlotsPerDay - 1);
    }

    private void ApplyActivity(Person person, ScheduleActivity nextActivity, bool force)
    {
        currentActivities.TryGetValue(person, out ScheduleActivity previousActivity);
        if (!force && previousActivity == nextActivity)
        {
            return;
        }

        currentActivities[person] = nextActivity;
        person.IBehavior?.ApplyScheduleActivity(nextActivity);
        OnActivityChanged?.Invoke(person, previousActivity, nextActivity);
    }
}
