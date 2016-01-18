namespace GP_Final
{
    public interface ISubject
    {
        void Attach(IObserver observer);

        void Detach(IObserver observer);

        void BoolNotify(bool b, string s);
    }
}
