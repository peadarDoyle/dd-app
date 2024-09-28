using System.Collections.Generic;
using DirectDebits.Models.Entities;

namespace DirectDebits.Persistence.Contracts
{
    public interface IBankRepository
    {
        Bank Get(int id);
        IList<Bank> GetAll();
    }
}
