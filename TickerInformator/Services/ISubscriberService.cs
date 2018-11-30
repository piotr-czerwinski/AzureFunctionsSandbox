using System.Threading.Tasks;

namespace TickerInformator
{
    public interface ISubscriberService
    {
        Task<bool> UpdateSubscriberData(SubmitInfo submitInfo);
    }
}
