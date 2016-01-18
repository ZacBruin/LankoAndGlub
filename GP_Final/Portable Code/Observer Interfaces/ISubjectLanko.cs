using System.Collections.Generic;

namespace GP_Final
{
    public interface ISubjectLanko : ISubject
    {
        List<ILankoObserver> Observers {get; set;}

        void Attach(ILankoObserver observer);

        void Detach(ILankoObserver observer);
    }
}
