using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DirectDebits.Common.Utility;
using DirectDebits.ViewModels.Batches;
using DirectDebits.Attributes.Security;
using DirectDebits.Attributes.ModelStatePersistence;
using DirectDebits.ExactClient.Models;
using ExactOnline.Client.Models.FinancialTransaction;
using DirectDebits.ExactClient.Helpers;
using DirectDebits.Common;
using System.Configuration;
using MvcPaging;
using DirectDebits.ExactClient.Services;
using DirectDebits.Models.Entities;
using DirectDebits.Persistence.Contracts;
using ExactOnline.Client.Models.CRM;
using Account = DirectDebits.Models.Entities.Account;
using ExactOnline.Client.Models.Financial;
using Serilog;
using System.Diagnostics;

namespace DirectDebits.Controllers
{
    [RoutePrefix("batches")]
    [AllowCrossSiteJson]
    public class BatchController : SecureController
    {
        protected IAccountRepository AccountStorage;
        protected IBatchRepository BatchStorage;

        public BatchController(
            ILogger logger,
            IAccountRepository accountStorage,
            IBatchRepository batchStorage,
            IOrganisationRepository organisationRepository,
            ApplicationUserManager userManager) : base (logger, organisationRepository)
        {
            AccountStorage = accountStorage;
            BatchStorage = batchStorage;
            UserManager = userManager;
        }

        [Route("{type}")]
        [HttpGet]
        public async Task<ActionResult> BatchList(BatchType type, int? page)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            ApplicationUser user = await GetCurrentUser();

            var endpoint = ControllerContext.HttpContext.Request.RawUrl;
            var method = ControllerContext.HttpContext.Request.HttpMethod;

            Logger.Information("Begin processing request {Method} {Endpoint} for {Orginisation}/{User}",
                method, endpoint, user.Organisation.Name, user.UserName);

            if (FeatureRestricted(user, type))
            {
                return new HttpUnauthorizedResult();
            }

            string value = ConfigurationManager.AppSettings["PageSize"];
            int pageSize = int.Parse(value);

            page = page == null ? 0 : page - 1;

            int count = BatchStorage.Count(user.Organisation.Id, type);
            var batches = BatchStorage.GetMany(user.Organisation.Id, type, page.Value, pageSize);

            var model = new BatchListViewModel
            {
                Batches = batches.Select(x => new BatchHeaderViewModel(x)).ToPagedList(page.Value, pageSize, count),
                Type = type
            };

            Logger.Information("Completed processing request {Method} {Endpoint} for {Orginisation}/{User}, (elapsed:{Elapsed}ms)}",
                method, endpoint, user.Organisation.Name, user.UserName, stopwatch.ElapsedMilliseconds);

            return View("Index", model);
        }

        [Route("{type}/create")]
        [HttpGet]
        public async Task<ActionResult> Create(BatchType type)
        {
            ApplicationUser user = await GetCurrentUser();

            if (FeatureRestricted(user, type))
            {
                return new HttpUnauthorizedResult();
            }

            Organisation org = OrganisationRepository.Get(user.Organisation.ExternalId);
            BatchSettings settings = org.GetSettings(type);

            var filterId = settings.ClassificationFilterId;

            CreateBatchSettingsViewModel model;

            if (filterId.HasValue)
            {
                var exactAccountService = new ExactCrmService(Logger, org.ExternalAccessCode);
                AccountClassificationName classificationName = exactAccountService.GetAccountClassificationNameByCode(filterId.Value);
                var classifications = exactAccountService.GetAccountClassificationById(classificationName.ID);
                model = new CreateBatchSettingsViewModel(type, settings, classificationName.Description, classifications);
            }
            else
            {
                model = new CreateBatchSettingsViewModel(type, settings);
            }

            return View(model);
        }

        [Route("{type}/invoices")]
        [HttpGet]
        public async Task<ActionResult> Invoices(BatchType type, IList<Guid> classificationIds)
        {
            ApplicationUser user = await GetCurrentUser();

            Logger.Information("Invoice endpoint hit for {@BatchType}", type);

            if (FeatureRestricted(user, type))
            {
                return new HttpUnauthorizedResult();
            }

            Organisation org = OrganisationRepository.Get(user.Organisation.ExternalId);
            BatchSettings settings = org.GetSettings(type);

            IList<ExactOnline.Client.Models.CRM.Account> accounts;

            var exactAccountService = new ExactCrmService(Logger, org.ExternalAccessCode);

            if (classificationIds.IsNullOrEmpty())
            {
                Logger.Information("Getting accounts by batch type...");
                accounts = exactAccountService.GetAccountsByBatchType(type);
                Logger.Information("Retrieved {@Accounts} accounts by batch type", accounts.Count);
            }
            else
            {
                Logger.Information("Getting accounts by classification and batch type...");
                accounts = exactAccountService.GetAccountsByClassificationAndBatchType(settings.ClassificationFilterId.Value, classificationIds, type);
                Logger.Information("Retrieved {@Accounts} accounts by classification and batch type", accounts.Count);
            }

            if (accounts.Count <= 0)
            {
                Logger.Information("No accounts available!");
                return Json(null, JsonRequestBehavior.AllowGet);
            }

            IList<AccountInvoicesViewModel> model;

            switch (type)
            {
                case BatchType.DirectDebit:
                    model = GetReceivables(accounts, settings, org.ExternalAccessCode);
                    break;
                case BatchType.Payment:
                    model = GetPayments(accounts, settings, org.ExternalAccessCode);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unexpected {nameof(BatchType)} encountered [{type}]");
            }

            Logger.Information("Request completed");
            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [Route("{type}/create")]
        [HttpPost]
        public async Task<ActionResult> Create(BatchType type, CreateBatchViewModel model)
        {
            ApplicationUser user = await GetCurrentUser();

            if (FeatureRestricted(user, type))
            {
                return new HttpUnauthorizedResult();
            }
            if (!ModelState.IsValid)
            {
                return PartialView("_ValidationSummary", model);
            }

            Organisation org = OrganisationRepository.Get(user.Organisation.ExternalId);
            BatchSettings settings = user.Organisation.GetSettings(type);

            var settingsErrors = settings.ValidateForBatchCreate(type);

            if (settingsErrors.Count > 0)
            {
                string error = "The following have not been set: " + string.Join(", ", settingsErrors);
                return Json(new { isSuccess = false, errorMessage = error });
            }

            List<Account> accounts = model.Accounts.Select(x => new Account
            {
                ExternalId = x.Id,
                ExternalDisplayId = x.DisplayId,
                Name = x.Name
            }).ToList();

            AccountStorage.UpdateNameIfChanged(org.Id, accounts);
            AccountStorage.CreateIfNotExists(user.Organisation, accounts);

            Batch batch = MakeBatch(user, org, type, model);

            var exactTransactionService = new ExactFinancialService(Logger, org.ExternalAccessCode);
            Result<FinancialPeriod> financialPeriodResult = exactTransactionService.GetFinancialPeriodForDate(batch.ProcessDate);

            if (!financialPeriodResult.IsSuccess)
            {
                return Json(new { isSuccess = financialPeriodResult.IsSuccess, errorMessage = financialPeriodResult.Error });
            }

            short finYear = financialPeriodResult.Value.FinYear.Value;
            short finPeriod = financialPeriodResult.Value.FinPeriod.Value;
            var transaction = new ExactTransaction(batch, finYear, finPeriod);

            string bankEntry = BankEntryHelper.Create(transaction);
            var exactXmlService = new ExactXmlService(Logger, org.ExternalAccessCode);
            Result<string> bankEntryResult = exactXmlService.UploadBankEntry(bankEntry);

            if (!bankEntryResult.IsSuccess)
            {
                return Json(new { isSuccess = bankEntryResult.IsSuccess, errorMessage = bankEntryResult.Error });
            }

            string batchEntryId = bankEntryResult.Value;
            string matchSets = MatchSetHelper.Create(transaction, batchEntryId);
            Result matchSetsResult = exactXmlService.UploadMatchSets(matchSets);

            if (matchSetsResult.IsSuccess)
            {
                BatchStorage.Create(batch);
                string route = Url.Action("View", new { type, number = batch.Number });
                return Json(new { isSuccess = matchSetsResult.IsSuccess, value = route });
            }
            else
            {
                return Json(new { isSuccess = matchSetsResult.IsSuccess, errorMessage = matchSetsResult.Error });
            }
        }

        [Route("{type}/view/{number}")]
        [HttpGet]
        [ImportModelStateFromTempData]
        public async Task<ActionResult> View(BatchType type, int number)
        {
            ApplicationUser user = await GetCurrentUser();

            if (FeatureRestricted(user, type))
            {
                return new HttpUnauthorizedResult();
            }

            Organisation org = OrganisationRepository.Get(user.Organisation.ExternalId);
            Batch batch = BatchStorage.Get(org.Id, type, number);

            var model = new BatchHeaderViewModel(batch);
                
            return View(model);
        }

        [Route("{type}/viewlines/{number}")]
        [HttpGet]
        public async Task<ActionResult> ViewLines(BatchType type, int number)
        {
            ApplicationUser user = await GetCurrentUser();

            if (FeatureRestricted(user, type))
            {
                return new HttpUnauthorizedResult();
            }

            Organisation org = OrganisationRepository.Get(user.Organisation.ExternalId);
            Batch batch = BatchStorage.Get(org.Id, type, number);

            var model = new BatchLinesViewModel(batch);
                
            return PartialView("_ViewLines", model);
        }


        // todo: it would be good to put this into some kind of batch service
        private Batch MakeBatch(ApplicationUser user, Organisation org, BatchType type, CreateBatchViewModel model)
        {
            Logger.Information("Generating new batch number...");
            int nextBatchNumber = GetNextBatchNumber(org.Id, type);
            Logger.Information("The new batch number is {@NextBatchNumber}", nextBatchNumber);
            
            var newBatch = new Batch
            {
                Number = nextBatchNumber,
                CreatedOn = DateTime.Now,
                Organisation = org,
                CreatedBy = user,
                InvoicesAffected = model.Accounts.Sum(x => x.Invoices.Count()),
                TotalProcessed = model.Accounts.Sum(x => x.Invoices.Sum(y => y.Alloc)),
                AccountsAffected = model.Accounts.Count(),
                ProcessDate = model.ProcessDate.Value,
                BatchType = type
            };

            var exactIds = model.Accounts.Select(x => x.Id);
            IEnumerable<Account> accounts = AccountStorage.GetManyByExactId(exactIds);

            IEnumerable<int> invoiceIds = model.Accounts.SelectMany(x => x.Invoices.Select(y => y.Id));

            var exactTransactionService = new ExactFinancialService(Logger, org.ExternalAccessCode);
            IEnumerable<Transaction> transactions = exactTransactionService.GetTransactions(invoiceIds);

            newBatch.Allocations = model.Accounts.Select(x => x.Invoices.Select(y => new Allocation
            {
                InvoiceId = y.Id.ToString(),
                Amount = y.Alloc,
                InvoiceTotal = y.Amount,
                InvoiceCreatedOn = transactions.Where(z => z.EntryNumber.Value == y.Id)
                                               .Select(z => new DateTime(z.FinancialYear.Value, z.FinancialPeriod.Value, 1))
                                               .Single(),
                Account = accounts.Single(z => z.ExternalId == x.Id),
                Batch = newBatch
            })).SelectMany(x => x).ToList();

            return newBatch;
        }

        private int GetNextBatchNumber(int organisationId, BatchType type)
        {
            Batch latestBatch = BatchStorage.GetLatest(organisationId, type);
            return (latestBatch?.Number ?? 0) + 1;
        }

        private IList<AccountInvoicesViewModel> GetReceivables(IList<ExactOnline.Client.Models.CRM.Account> accounts, BatchSettings settings, int division)
        {
            var exactTransactionService = new ExactFinancialService(Logger, division);

            IList<ExactOnline.Client.Models.Cashflow.Receivable> receivables;

            int queryId = settings.GetReceivableQueryConfigId();

            switch(queryId)
            {
                case 0:
                    receivables = exactTransactionService.GetNonZeroCashflowReceivables(accounts);
                    break;
                case 1:
                    receivables = exactTransactionService.GetOpenCashflowReceivables(accounts);
                    break;
                case 2:
                    var allReceivables = exactTransactionService.GetAllCashflowReceivables(accounts);
                    receivables = allReceivables.Where(x => x.AmountDC > 0 || x.AmountDC < 0).ToList();
                    Logger.Information($"{allReceivables.Count - receivables.Count} of {allReceivables.Count} discarded!");
                    break;
                case 3:
                    receivables = exactTransactionService.GetFinancialReceivablesHavingInvoiceNumber(accounts);
                    break;
                default:
                    throw new NotSupportedException($"The receivable query config '{queryId}' is not supported");
            }

            return receivables.GroupBy(x => x.Account)
                .Select(x => new AccountInvoicesViewModel(x.ToList(), settings))
                .OrderBy(x => x.Name)
                .ToList();
        }

        private IList<AccountInvoicesViewModel> GetPayments(IList<ExactOnline.Client.Models.CRM.Account> accounts, BatchSettings settings, int division)
        {
            var financialService = new ExactFinancialService(Logger, division);

            return financialService.GetPayments(accounts)
                                   .GroupBy(x => x.Account)
                                   .Select(x => new AccountInvoicesViewModel(x.ToList(), settings))
                                   .OrderBy(x => x.Name)
                                   .ToList();
        }
    }
}
