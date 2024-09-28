using System;
using DirectDebits.Models.Entities;

namespace DirectDebits.Persistence.Contracts
{
    public interface IOrganisationRepository
    {
        void Create(Organisation organisation);
        Organisation Get(Guid externalId);
        Organisation GetByUserName(string username);
        bool Exists(Guid externalId);
        void Update(Organisation organisation);
    }
}
