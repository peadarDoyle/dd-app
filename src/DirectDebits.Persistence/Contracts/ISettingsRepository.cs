using DirectDebits.Models.Entities;

namespace DirectDebits.Persistence.Contracts
{
    public interface ISettingsRepository
    {
        BatchSettings Get(int id);
        void Update(BatchSettings settings);
    }
}