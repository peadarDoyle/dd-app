using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DirectDebits.Common;

namespace DirectDebits.Models.Entities
{
    [Table("Organisation")]
    public class Organisation : TrackedEntity
    {
        public Organisation()
        {
            Users = new HashSet<ApplicationUser>();
            Accounts = new HashSet<Account>();
            Batches = new HashSet<Batch>();
        }

        /// <summary>
        /// This constructor is expected to be used when creating a new Organisation
        /// </summary>
        /// <param name="externalId">The id unique to the account for the external provider</param>
        /// <param name="externalAccessCode">The code used when accessing data from the external provider</param>
        /// <param name="name">The name of the account of the external provider</param>
        public Organisation(Guid externalId, int externalAccessCode, string name)
        {
            ExternalId = externalId;
            ExternalAccessCode = externalAccessCode;
            Name = name;
            DirectDebitSettings = new BatchSettings();
            PaymentSettings = new BatchSettings();
        }

        public Guid ExternalId { get; set; }
        public int ExternalAccessCode { get; set; }
        [MaxLength(150)]
        public string Name { get; set; }
        public virtual BatchSettings DirectDebitSettings { get; set; }
        public virtual BatchSettings PaymentSettings { get; set; }
        public bool HasDirectDebitsFeature { get; set; }
        public bool HasPaymentsFeature { get; set; }
        public virtual ICollection<ApplicationUser> Users { get; set; }
        public virtual ICollection<Account> Accounts { get; set; }
        public virtual ICollection<Batch> Batches { get; set; }

        public BatchSettings GetSettings(BatchType type)
        {
            switch (type)
            {
                case BatchType.DirectDebit:
                    return DirectDebitSettings;
                case BatchType.Payment:
                    return PaymentSettings;
                default:
                    throw new ArgumentException($"The batch type [{type}] is not recognised");
            }
        }
    }
}
