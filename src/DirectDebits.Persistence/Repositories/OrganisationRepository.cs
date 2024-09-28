using System;
using System.Data.Entity;
using System.Linq;
using DirectDebits.Models;
using DirectDebits.Models.Context;
using DirectDebits.Models.Entities;
using DirectDebits.Persistence.Contracts;
using Serilog;

namespace DirectDebits.Persistence.Repositories
{
    public class OrganisationRepository : IOrganisationRepository
    {
        private readonly ILogger _log;
        private readonly ISynergyDbContext _context;

        public OrganisationRepository(ILogger log, ISynergyDbContext context)
        {
            _log = log;
            _context = context;
        }

        public void Create(Organisation organisation)
        {
            _log.Information("Attempting to create an organisation for the externalId:{@ExternalId}", organisation.ExternalId);

            Bank ddBank = _context.Banks.Single(x => x.Id == organisation.DirectDebitSettings.Bank.Id);
            organisation.DirectDebitSettings.Bank = ddBank;

            Bank payBank = _context.Banks.Single(x => x.Id == organisation.PaymentSettings.Bank.Id);
            organisation.PaymentSettings.Bank = payBank;

            _context.Organisations.Add(organisation);
            _context.SaveChanges();

            _log.Information("Organisation created with name:{@Name} and id:{@Id}", organisation.Name, organisation.Id);
        }

        public Organisation Get(Guid externalId)
        {
            _log.Information("Attempting to retrieve an organisation for the externalId:{@ExternalId}", externalId);

            var organisation =
                _context.Organisations
                        .Include(x => x.PaymentSettings)
                        .Include(x => x.PaymentSettings.Bank)
                        .Include(x => x.DirectDebitSettings)
                        .Include(x => x.DirectDebitSettings.Bank)
                        .Single(x => x.ExternalId == externalId);

            _log.Information("Organisation retrieved via the externalId:{@ExternalId} with name:{@Name} and id:{@Id}",
                externalId, organisation.Name, organisation.Id);

            return organisation;
        }

        public Organisation GetByUserName(string username)
        {
            _log.Information("Attempting to retrieve an organisation for the username:{@Username}", username);

            var organisation =
                _context.Organisations
                        .Include(x => x.PaymentSettings)
                        .Include(x => x.PaymentSettings.Bank)
                        .Include(x => x.DirectDebitSettings)
                        .Include(x => x.DirectDebitSettings.Bank)
                        .Single(x => x.Users.Any(y => y.UserName == username));

            _log.Information("Organisation retrieved for the username:{@Username} with name:{@Name} and id:{@Id}",
                username, organisation.Name, organisation.Id);

            return organisation;
        }

        public bool Exists(Guid externalId)
        {
            _log.Information("Attempting to check if the organisation with externalId:{@ExternalId} exists", externalId);

            bool exists = _context.Organisations.Any(x => x.ExternalId == externalId);

            if (exists)
            {
                _log.Information("Organisation verified to exist for the externalId:{@ExternalId}", externalId);
            }
            else
            {
                _log.Information("Organisation does not exist for the externalId:{@ExternalId}", externalId);
            }

            return exists;
        }

        public void Update(Organisation org)
        {
            _log.Information("Attempting to update the organisation with id:{@Id}", org.Id);

            Organisation persistedOrg = _context.Organisations.Single(x => x.Id == org.Id);
            persistedOrg = org;
            _context.SaveChanges();

            _log.Information("Organisation with id:{@Id} was updated", org.Id);
        }
    }
}
