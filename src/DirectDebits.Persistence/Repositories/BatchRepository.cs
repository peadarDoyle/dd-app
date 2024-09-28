using System;
using System.Collections.Generic;
using System.Linq;
using DirectDebits.Common;
using DirectDebits.Models.Context;
using DirectDebits.Models.Entities;
using DirectDebits.Persistence.Contracts;
using Serilog;

namespace DirectDebits.Persistence.Repositories
{
    public class BatchRepository : IBatchRepository
    {
        private readonly ILogger _log;
        private readonly ISynergyDbContext _context;

        public BatchRepository(ILogger log, ISynergyDbContext context)
        {
            _log = log;
            _context = context;
        }

        public void Create(Batch batch)
        {
            _context.Batches.Add(batch);
            _context.SaveChanges();
        }

        public int Count(int orgId, BatchType type)
        {
            var count = _context.Batches
                                .Where(x => x.Organisation.Id == orgId)
                                .Count(x => x.BatchType == type);

            _log.Information("Batch count is {@BatchCount}", count);

            return count;
        }

        public Batch Get(int orgId, BatchType type, int batchNum)
        {
            Batch batch = _context.Batches
                                .Where(x => x.Organisation.Id == orgId)
                                .Where(x => x.BatchType == type)
                                .Single(x => x.Number == batchNum);

            _log.Information("Batch retrieved id:{@BatchId}", batch.Id);

            return batch;
        }

        public Batch GetLatest(int orgId, BatchType type)
        {
            _log.Information("Attempting to get latest batch, orgId:{@orgId}, type:{@type}", orgId, type);

            Batch batch = _context.Batches
                           .Where(x => x.Organisation.Id == orgId)
                           .Where(x => x.BatchType == type)
                           .OrderByDescending(x => x.Id)
                           .FirstOrDefault();

            if (batch == null)
            {
                _log.Information("No existing batches found");
            }
            else
            {
                _log.Information("Latest batch retrieved, id:{@BatchId}, createdOn:{@CreatedOn}", batch.Id, batch.CreatedOn);
            }

            return batch;
        }

        public IList<Batch> GetMany(int orgId, BatchType type, int page, int size)
        {
            List<Batch> batches = _context.Batches
                                          .Where(x => x.Organisation.Id == orgId)
                                          .Where(x => x.BatchType == type)
                                          .OrderByDescending(x => x.CreatedOn)
                                          .Skip(page * size)
                                          .Take(size)
                                          .ToList();


            string batchIds = string.Join(", ", batches.Select(batch => batch.Id));
            _log.Information("Get many batches with pagination ({@Pagination}) and (ids:{@BatchIds})", new { page, size }, batchIds);

            return batches;
        }
    }
}
