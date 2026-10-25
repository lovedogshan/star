using System.Collections.Generic;
using UnityEngine;

public interface IReservationManager
{
    bool TryReserve(Person person, object target, AITask task = null);
    bool IsReservedByOther(Person person, object target);
    void Release(Person person, object target);
    void ReleaseTask(AITask task);
    void ReleaseAll(Person person);
}

public class ReservationManager : MonoBehaviour, IReservationManager
{
    private class Reservation
    {
        public Person Person;
        public AITask Task;
    }

    private readonly Dictionary<object, Reservation> reservations = new Dictionary<object, Reservation>();

    public static ReservationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public bool TryReserve(Person person, object target, AITask task = null)
    {
        if (!person || target == null)
        {
            return false;
        }

        if (reservations.TryGetValue(target, out Reservation current))
        {
            return current.Person == person && (task == null || current.Task == task);
        }

        reservations[target] = new Reservation { Person = person, Task = task };
        return true;
    }

    public bool IsReservedByOther(Person person, object target)
    {
        return target != null && reservations.TryGetValue(target, out Reservation current) && current.Person != person;
    }

    public void Release(Person person, object target)
    {
        if (target != null && reservations.TryGetValue(target, out Reservation current) && current.Person == person)
        {
            reservations.Remove(target);
        }
    }

    public void ReleaseTask(AITask task)
    {
        if (task == null)
        {
            return;
        }

        RemoveMatching(reservation => reservation.Task == task);
    }

    public void ReleaseAll(Person person)
    {
        if (!person)
        {
            return;
        }

        RemoveMatching(reservation => reservation.Person == person);
    }

    private void RemoveMatching(System.Predicate<Reservation> predicate)
    {
        List<object> remove = new List<object>();
        foreach (KeyValuePair<object, Reservation> pair in reservations)
        {
            if (predicate(pair.Value))
            {
                remove.Add(pair.Key);
            }
        }

        foreach (object key in remove)
        {
            reservations.Remove(key);
        }
    }
}
