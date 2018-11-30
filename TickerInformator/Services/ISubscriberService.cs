using System.Threading.Tasks;

namespace TickerInformator
{
    interface ISubscriberService
    {
        Task<bool> UpdateSubscriberData(SubmitInfo submitInfo);
    }
}
