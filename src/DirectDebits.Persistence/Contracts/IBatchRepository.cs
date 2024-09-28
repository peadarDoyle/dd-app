using System.Collections.Generic;
using DirectDebits.Common;
using DirectDebits.Models.Entities;

namespace DirectDebits.Persistence.Contracts
{
    public interface IBatchRepository
    {
        void Create(Batch batch);
        int Count(int orgId, BatchType type);
        Batch Get(int orgId, BatchType type, int batchNum);
        Batch GetLatest(int orgId, BatchType type);
        IList<Batch> GetMany(int orgId, BatchType type, int page, int size);
    }
}
