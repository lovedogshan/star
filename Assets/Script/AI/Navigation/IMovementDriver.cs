public interface IMovementDriver
{
    MovementStatus Tick(Person person, NavigationSegment segment);
    void Cancel(Person person);
}
