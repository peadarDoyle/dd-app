using System;
using System.Collections.Generic;
using DirectDebits.Models;
using DirectDebits.Common;
using DirectDebits.Models.Entities;
using DirectDebits.Persistence.Contracts;

namespace DirectDebits.Tests.Persistence.Repositories
{
    public class FakeBatchRepo : IBatchRepository
    {
        public void Create(Batch batch)
        {
            throw new NotImplementedException();
        }

        public int Count(int orgId, BatchType type)
        {
            throw new NotImplementedException();
        }

        public Batch Get(int organisationId, BatchType type, int batchId)
        {
            var bank = new Bank
            {
                Id = 1,
                Name = "Allied Irish Banks",
                Shorthand = "AIB"
            };

            var organisation = new Organisation
            {
                Id = 1,
                ExternalId = Guid.Parse("da92aa44-35dc-4805-a841-3dfbe816e8ea"),
                CreatedOn = DateTime.Now,
                DirectDebitSettings = new BatchSettings
                {
                    Period1 = 30,
                    Period2 = 60,
                    Period3 = 90,
                    Bank = bank,
                    BankAccName = "Bank Account Name",
                    AuthId = "AUTHID",
                    Bic = "AIBKIE2D",
                    Iban = "IE22AIBXXXXXXXXXXXXXXX",
                    BankJournalCode = "20",
                    TradeJournalCode = "70",
                    BankGlCode = "15400",
                    TradeGlCode = "15880"
                }
            };

            var user = new ApplicationUser
            {
                Organisation = organisation
            };

            organisation.Users.Add(user);

            var batch = new Batch
            {
                Id = 1,
                CreatedOn = DateTime.Now,
                ProcessDate = DateTime.Now,
                Organisation = organisation,
                CreatedBy = user
            };

            organisation.Batches.Add(batch);

            var allocations = new List<Allocation>();

            var customer1 = new Account
            {
                Id = 1,
                ExternalId = Guid.Parse("e6973f50-d262-418e-0001-4016ea345386").ToString(),
                ExternalDisplayId = "1101",
                Name = "Customer 1"
            };

            allocations.Add(new Allocation
            {
                Id = 1,
                Amount = 10m,
                InvoiceTotal = 10m,
                InvoiceCreatedOn = DateTime.Now,
                Batch = batch,
                Account = customer1,
                InvoiceId = "100001"
            });

            allocations.Add(new Allocation
            {
                Id = 2,
                Amount = 20m,
                InvoiceTotal = 20m,
                InvoiceCreatedOn = DateTime.Now,
                Batch = batch,
                Account = customer1,
                InvoiceId = "100002"
            });

            allocations.Add(new Allocation
            {
                Id = 3,
                Amount = 30m,
                InvoiceTotal = 40m,
                InvoiceCreatedOn = DateTime.Now,
                Batch = batch,
                Account = customer1,
                InvoiceId = "100003"
            });

            var customer2 = new Account
            {
                Id = 2,
                ExternalId = Guid.Parse("e6973f50-d262-418e-0002-4016ea345386").ToString(),
                ExternalDisplayId = "1103",
                Name = "Customer 2"
            };

            allocations.Add(new Allocation
            {
                Id = 4,
                Amount = 40m,
                InvoiceTotal = 40m,
                InvoiceCreatedOn = DateTime.Now,
                Batch = batch,
                Account = customer2,
                InvoiceId = "200001"
            });

            allocations.Add(new Allocation
            {
                Id = 5,
                Amount = 50m,
                InvoiceTotal = 50m,
                InvoiceCreatedOn = DateTime.Now,
                Batch = batch,
                Account = customer2,
                InvoiceId = "200002"
            });

            var customer3 = new Account
            {
                Id = 3,
                ExternalId = Guid.Parse("e6973f50-d262-418e-0003-4016ea345386").ToString(),
                ExternalDisplayId = "1107",
                Name = "Customer 3"
            };

            allocations.Add(new Allocation
            {
                Id = 6,
                Amount = 60m,
                InvoiceTotal = 60m,
                InvoiceCreatedOn = DateTime.Now,
                Batch = batch,
                Account = customer3,
                InvoiceId = "300001"
            });

            allocations.Add(new Allocation
            {
                Id = 7,
                Amount = 70m,
                InvoiceTotal = 100m,
                InvoiceCreatedOn = DateTime.Now,
                Batch = batch,
                Account = customer3,
                InvoiceId = "300002"
            });

            var customer4 = new Account
            {
                Id = 4,
                ExternalId = Guid.Parse("e6973f50-d262-418e-0004-4016ea345386").ToString(),
                ExternalDisplayId = "1110",
                Name = "Cus4omer 4"
            };

            allocations.Add(new Allocation
            {
                Id = 8,
                Amount = 80m,
                InvoiceTotal = 80m,
                InvoiceCreatedOn = DateTime.Now,
                Batch = batch,
                Account = customer4,
                InvoiceId = "400001"
            });

            batch.Allocations = allocations;

            return batch;
        }

        public Batch GetLatest(int organisationId, BatchType type)
        {
            throw new NotImplementedException();
        }

        public IList<Batch> GetMany(int organisationId, BatchType type, int page, int size)
        {
            throw new NotImplementedException();
        }
    }
}
