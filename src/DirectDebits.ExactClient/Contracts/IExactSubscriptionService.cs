using System.Collections.Generic;
using System.Threading.Tasks;
using ExactOnline.Client.Models.Current;
using ExactOnline.Client.Models.HRM;

namespace DirectDebits.ExactClient.Contracts
{
    public interface IExactSubscriptionService
    {
        Task<IList<Division>> GetAllDivisionsAsync();
        Division GetDivisionFromCode(int code);
        Me GetMe();
    }
}