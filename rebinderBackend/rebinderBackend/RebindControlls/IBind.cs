namespace rebinderBackend.RebindControls
{
    public interface IBind
    {
        void Start();
        void Stop();
        string ToFrontendData();
    }
}