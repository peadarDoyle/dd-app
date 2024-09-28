using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExactOnline.Client.Models.Current;
using ExactOnline.Client.Models.HRM;
using DirectDebits.ExactClient.Contracts;
using Serilog;

namespace DirectDebits.ExactClient.Services
{
    public class ExactSubscriptionService : ExactServiceBase, IExactSubscriptionService
    {
        public ExactSubscriptionService(ILogger logger, int? division) : base (logger, division) { }

        public Me GetMe()
        {
            var fields = "DivisionCustomerName,DivisionCustomer,UserName,Email";

            return Client.For<Me>()
                         .Select(fields)
                         .Get()
                         .Single();
        }

        public Division GetDivisionFromCode(int code)
        {
            var fields = "Code,Customer,CustomerName";
            string query = $"Code+eq+{code}";

            return Client.For<Division>()
                         .Select(fields)
                         .Where(query)
                         .Get()
                         .Single();
        }

        public async Task<IList<Division>> GetAllDivisionsAsync()
        {
            var fields = "Code,Description,Customer";

            return await Task.Run(() =>
                Client.For<Division>()
                      .Select(fields)
                      .Get()
            );
        }
    }
}