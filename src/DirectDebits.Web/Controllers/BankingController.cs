using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Web.Mvc;
using DirectDebits.ViewModels.Banking;
using DirectDebits.Core.Banking;
using System.Collections.Generic;
using DirectDebits.Attributes.ModelStatePersistence;
using DirectDebits.Common;
using DirectDebits.ExactClient.Services;
using DirectDebits.Models.Entities;
using DirectDebits.Persistence.Contracts;
using Serilog;
using System.Diagnostics;

namespace DirectDebits.Controllers
{
    [RoutePrefix("banking")]
    public class BankingController : SecureController
    {
        protected IAccountRepository AccountStorage;
        protected IBankRepository BankStorage;
        protected IBatchRepository BatchStorage;

        public BankingController(
            ILogger logger,
            IAccountRepository accountStorage,
            IBankRepository bankStorage,
            IBatchRepository batchStorage,
            IOrganisationRepository organisationStorage,
            ApplicationUserManager userManager) : base (logger, organisationStorage)
        {
            AccountStorage = accountStorage;
            BankStorage = bankStorage;
            BatchStorage = batchStorage;
            UserManager = userManager;
        }

        [Route("{type}/create-bank-file")]
        [HttpPost]
        [ExportModelStateToTempData]
        public async Task<ActionResult> CreateBankFile(BatchType type, CreateBankFileViewModel model)
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
                Logger.Warning("Request abandoned due to access restrictions, (elapsed:{Elapsed}ms)}", stopwatch.ElapsedMilliseconds);
                return new HttpUnauthorizedResult();
            }

            if (!ModelState.IsValid)
            {
                TempData["RedirectCall"] = true;
                Logger.Warning("Request abandoned due to invalid view model, (elapsed:{Elapsed}ms)}", stopwatch.ElapsedMilliseconds);
                return RedirectToAction("View", "Batch", new { number = model.BatchNum });
            }

            Batch batch = BatchStorage.Get(user.Organisation.Id, type, model.BatchNum);

            string[] externalIds = AccountStorage.GetAccountsForBatch(batch.Id)
                                                 .Select(x => x.ExternalId)
                                                 .ToArray();

            var exactAccountService = new ExactCrmService(Logger, user.Organisation.ExternalAccessCode);
            IList<BankAgent> bankAgents = exactAccountService.GetAccountsById(externalIds)
                                                             .Select(x => BankAgent.Create(type, x))
                                                             .ToList();

            var invalidNames = bankAgents.Where(x => !x.Validate().IsSuccess).Select(x => x.Name);

            if (invalidNames.Any())
            {
                string error = "Bank data missing for " + string.Join(", ", invalidNames);
                ModelState.AddModelError("Bank Details", error);

                Logger.Warning("Request abandoned due to " + error + ", (elapsed:{Elapsed}ms)}", stopwatch.ElapsedMilliseconds);

                return RedirectToAction("View", "Batch", new { number = model.BatchNum });
            }
            else
            {
                Logger.Information("The bank agents are all valid, can continue...");
            }

            Organisation org = OrganisationRepository.Get(user.Organisation.ExternalId);
            Bank bank = BankStorage.Get(org.GetSettings(type).Bank.Id);
            BankFileBuilder bankFileBuilder = BankFileBuilderFactory.Create(type, bank.Name);

            Logger.Information("Beginning to create the bank file stream");
            Stream stream = bankFileBuilder.Create(batch, bankAgents, model.ProcessDate);
            Logger.Information("The bank file stream has been created, {@BankFileBytes}bytes", stream.Length);

            string fileName = BatchTypeHelper.GetFileName(type, model.BatchNum);
            Logger.Information("The bank file name is {@BankFileName}", fileName);

            Response.AddHeader("Content-Disposition", "attachment; filename=" + fileName);
            Response.ContentType = "text/xml";

            Logger.Information("Completed processing request {Method} {Endpoint} for {Orginisation}/{User}, (elapsed:{Elapsed}ms)}",
                method, endpoint, user.Organisation.Name, user.UserName, stopwatch.ElapsedMilliseconds);

            // don't worry about disposing the stream since the FileStreamResult handles it
            return new FileStreamResult(stream, "application/octet-stream");
        }
    }
}
