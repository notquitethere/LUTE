namespace LoGaCulture.LUTE
{
    public static class LocationServiceSignals
    {
        public static event LocationFailedHandler OnLocationFailed;
        public delegate void LocationFailedHandler(FailureMethod failureMethod, Node relatedNode);

        public static void DoLocationFailed(FailureMethod failureMethod, Node relatedNode)
        {
            OnLocationFailed?.Invoke(failureMethod, relatedNode);
        }
    }
}
