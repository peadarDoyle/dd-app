namespace DirectDebits.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Payments : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.Customer", newName: "Account");
            DropForeignKey("dbo.Organisation", "BankId", "dbo.Bank");
            DropForeignKey("dbo.Allocation", "CustomerId", "dbo.Customer");
            DropIndex("dbo.Allocation", new[] { "CustomerId" });
            DropIndex("dbo.Organisation", new[] { "BankId" });
            CreateTable(
                "dbo.BatchSettings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Period1 = c.Int(nullable: false),
                        Period2 = c.Int(nullable: false),
                        Period3 = c.Int(nullable: false),
                        BankAccName = c.String(nullable: false, maxLength: 100),
                        BIC = c.String(nullable: false, maxLength: 50),
                        IBAN = c.String(nullable: false, maxLength: 50),
                        AuthId = c.String(nullable: false, maxLength: 50),
                        BankJournalCode = c.String(nullable: true, maxLength: 20),
                        TradeJournalCode = c.String(nullable: true, maxLength: 20),
                        BankGlCode = c.String(nullable: true, maxLength: 20),
                        TradeGlCode = c.String(nullable: true, maxLength: 20),
                        ClassificationFilterID = c.Int(),
                        CreatedOn = c.DateTime(nullable: false),
                        UpdatedOn = c.DateTime(),
                        Bank_Id = c.Int(nullable: false),
                        CreatedBy_Id = c.String(maxLength: 128),
                        UpdatedBy_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Bank", t => t.Bank_Id, cascadeDelete: true)
                .ForeignKey("dbo.AspNetUsers", t => t.CreatedBy_Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UpdatedBy_Id)
                .Index(t => t.Bank_Id)
                .Index(t => t.CreatedBy_Id)
                .Index(t => t.UpdatedBy_Id);
            
            AddColumn("dbo.Batch", "BatchType", c => c.Int(nullable: false));
            RenameColumn("dbo.Allocation", "CustomerId", "Account_Id");
            RenameColumn("dbo.Batch", "CustomersAffected", "AccountsAffected");
            AddColumn("dbo.Batch", "UpdatedOn", c => c.DateTime());
            AddColumn("dbo.Batch", "UpdatedBy_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.Organisation", "CreatedBy_Id", c => c.String(maxLength: 128));
            AddColumn("dbo.Organisation", "DirectDebitSettings_Id", c => c.Int());
            AddColumn("dbo.Organisation", "PaymentSettings_Id", c => c.Int());
            AddColumn("dbo.Bank", "CountryCode", c => c.String(maxLength: 3));
            AlterColumn("dbo.Allocation", "InvoiceId", c => c.String(maxLength: 20));
            AlterColumn("dbo.Organisation", "Name", c => c.String(maxLength: 150));
            AlterColumn("dbo.Organisation", "UpdatedOn", c => c.DateTime());
            AlterColumn("dbo.Bank", "Name", c => c.String(maxLength: 100));
            AlterColumn("dbo.Bank", "Shorthand", c => c.String(maxLength: 10));
            CreateIndex("dbo.Allocation", "Account_Id");
            CreateIndex("dbo.Batch", "UpdatedBy_Id");
            CreateIndex("dbo.Organisation", "CreatedBy_Id");
            CreateIndex("dbo.Organisation", "DirectDebitSettings_Id");
            CreateIndex("dbo.Organisation", "PaymentSettings_Id");
            AddForeignKey("dbo.Organisation", "CreatedBy_Id", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Organisation", "DirectDebitSettings_Id", "dbo.BatchSettings", "Id");
            AddForeignKey("dbo.Organisation", "PaymentSettings_Id", "dbo.BatchSettings", "Id");
            AddForeignKey("dbo.Batch", "UpdatedBy_Id", "dbo.AspNetUsers", "Id");
            AddForeignKey("dbo.Allocation", "Account_Id", "dbo.Account", "Id");
            Sql(@"
            SET IDENTITY_INSERT BatchSettings ON
            GO

            INSERT INTO BatchSettings (Id,Period1,Period2,Period3,BankAccName,BIC,IBAN,AuthId,BankJournalCode,TradeJournalCode,BankGlCode,TradeGlCode,ClassificationFilterID,CreatedOn,UpdatedOn,Bank_Id,UpdatedBy_Id)
            SELECT Id,Period1,Period2,Period3,BankAccName,BIC,IBAN,AuthId,BankJournalCode,SalesJournalCode, BankGlCode,TradeDebtorGlCode,ClassificationFilterID,CreatedOn,UpdatedOn,BankId,UpdatedBy_Id
            FROM Organisation
            GO

            SET IDENTITY_INSERT BatchSettings OFF
            GO

            UPDATE Organisation SET DirectDebitSettings_Id = Id
            GO");
            DropColumn("dbo.Organisation", "Period1");
            DropColumn("dbo.Organisation", "Period2");
            DropColumn("dbo.Organisation", "Period3");
            DropColumn("dbo.Organisation", "BankAccName");
            DropColumn("dbo.Organisation", "BIC");
            DropColumn("dbo.Organisation", "IBAN");
            DropColumn("dbo.Organisation", "AuthId");
            DropColumn("dbo.Organisation", "BankId");
            DropColumn("dbo.Organisation", "BankJournalCode");
            DropColumn("dbo.Organisation", "SalesJournalCode");
            DropColumn("dbo.Organisation", "BankGlCode");
            DropColumn("dbo.Organisation", "TradeDebtorGlCode");
        }
        
        public override void Down()
        {
        }
    }
}
