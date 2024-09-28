using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using DirectDebits.ViewModels.Settings;
using DirectDebits.Attributes.ModelStatePersistence;
using DirectDebits.Common;
using DirectDebits.Models.Entities;
using DirectDebits.Persistence.Contracts;
using Serilog;

namespace DirectDebits.Controllers
{
    [RoutePrefix("settings")]
    public class SettingsController : SecureController
    {
        protected ISettingsRepository SettingsStorage;
        protected IBankRepository BankStorage;
        protected IOrganisationRepository OrganisationRespository;

        public SettingsController(
            ILogger logger,
            ISettingsRepository settingsStorage,
            IBankRepository bankStorage,
            IOrganisationRepository organisationRespository,
            ApplicationUserManager userManager)
            : base (logger, organisationRespository)
        {
            SettingsStorage = settingsStorage;
            BankStorage = bankStorage;
            OrganisationRespository = organisationRespository;
            UserManager = userManager;
        }

        [Route("")]
        [HttpGet]
        public ActionResult Index()
        {
            return new RedirectResult("/settings/directdebit");
        }

        [Route("{type}")]
        [HttpGet]
        [ImportModelStateFromTempData]
        public async Task<ActionResult> Settings(BatchType type)
        {
            ApplicationUser user = await GetCurrentUser();

            if (FeatureRestricted(user, type))
                return new HttpUnauthorizedResult();

            int settingsId = user.Organisation.GetSettings(type).Id;
            BatchSettings settings = SettingsStorage.Get(settingsId);
            var model = new SettingsViewModel(type, settings);
            return View(model);
        }

        [Route("{type}/update-bank-details")]
        [HttpPost]
        [ExportModelStateToTempData]
        public async Task<ActionResult> UpdateBankDetails(BatchType type, UpdateBankDetailsViewModel model)
        {
            ApplicationUser user = await GetCurrentUser();

            if (FeatureRestricted(user, type))
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                int settingsId = user.Organisation.GetSettings(type).Id;
                BatchSettings settings = SettingsStorage.Get(settingsId);

                settings.UpdatedBy = user;
                settings.UpdatedOn = DateTime.Now;
                settings.BankAccName = model.BankAccName;
                settings.Bic = model.Bic;
                settings.Iban = model.Iban;
                settings.AuthId = model.AuthId;
                settings.Bank = new Bank { Id = model.BankId };

                SettingsStorage.Update(settings);
            }

            TempData["RedirectCall"] = true;
            return RedirectToAction("Settings", new { type });
        }

        [Route("{type}/update-periods")]
        [HttpPost]
        [ExportModelStateToTempData]
        public async Task<ActionResult> Updateperiods(BatchType type, UpdatePeriodsViewModel model)
        {
            ApplicationUser user = await GetCurrentUser();

            if (FeatureRestricted(user, type))
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                int settingsId = user.Organisation.GetSettings(type).Id;
                BatchSettings settings = SettingsStorage.Get(settingsId);

                user.Organisation.UpdatedBy = user;
                user.Organisation.UpdatedOn = DateTime.Now;

                settings.Period1 = model.Period1;
                settings.Period2 = model.Period2;
                settings.Period3 = model.Period3;

                SettingsStorage.Update(settings);
            }

            TempData["RedirectCall"] = true;
            return RedirectToAction("Settings", new { type });
        }

        [Route("{type}/update-exact-config")]
        [HttpPost]
        [ExportModelStateToTempData]
        public async Task<ActionResult> UpdateExactConfig(BatchType type, UpdateExactConfigViewModel model)
        {
            ApplicationUser user = await GetCurrentUser();

            if (FeatureRestricted(user, type))
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                int settingsId = user.Organisation.GetSettings(type).Id;
                BatchSettings settings = SettingsStorage.Get(settingsId);

                settings.UpdatedBy = user;
                settings.UpdatedOn = DateTime.Now;
                settings.BankJournalCode = model.BankJournalCode;
                settings.TradeJournalCode = model.TradeJournalCode;
                settings.BankGlCode = model.BankGlCode;
                settings.TradeGlCode = model.TradeGlCode;
                settings.ClassificationFilterId = model.ClassificationFilterId;

                SettingsStorage.Update(settings);
            }

            TempData["RedirectCall"] = true;
            return RedirectToAction("Settings", new { type });
        }


        [Route("{type}/update-app-config")]
        [HttpPost]
        [ExportModelStateToTempData]
        public async Task<ActionResult> UpdateAppConfig(BatchType type, UpdateAppConfigViewModel model)
        {
            ApplicationUser user = await GetCurrentUser();

            if (FeatureRestricted(user, type))
                return new HttpUnauthorizedResult();

            if (ModelState.IsValid)
            {
                int settingsId = user.Organisation.GetSettings(type).Id;
                BatchSettings settings = SettingsStorage.Get(settingsId);

                settings.UpdatedBy = user;
                settings.UpdatedOn = DateTime.Now;
                settings.LowLevelConfig = model.LowLevelConfig;

                SettingsStorage.Update(settings);
            }

            TempData["RedirectCall"] = true;
            return RedirectToAction("Settings", new { type });
        }

        [Route("modes")]
        [HttpGet]
        public async Task<ActionResult> GetModes()
        {
            ApplicationUser user = await GetCurrentUser();
            var model = new ModeDropdownViewModel(user.Organisation);
            return PartialView("_ModeDropdown", model);
        }

        public PartialViewResult BankSelector(int bankId)
        {
            var banks = BankStorage.GetAll();

            var model = banks.Select(x => new SelectListItem
            {
                Selected = (bankId == x.Id),
                Text = $"{x.Name} ({x.Shorthand})",
                Value = x.Id.ToString()
            }).ToList();

            return PartialView("_BankSelector", model);
        }
    }
}
