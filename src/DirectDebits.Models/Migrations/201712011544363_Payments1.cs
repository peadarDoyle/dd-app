namespace DirectDebits.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Payments1 : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.BatchSettings", "BankJournalCode", c => c.String(maxLength: 20));
            AlterColumn("dbo.BatchSettings", "TradeJournalCode", c => c.String(maxLength: 20));
            AlterColumn("dbo.BatchSettings", "BankGlCode", c => c.String(maxLength: 20));
            AlterColumn("dbo.BatchSettings", "TradeGlCode", c => c.String(maxLength: 20));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.BatchSettings", "TradeGlCode", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.BatchSettings", "BankGlCode", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.BatchSettings", "TradeJournalCode", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.BatchSettings", "BankJournalCode", c => c.String(nullable: false, maxLength: 20));
        }
    }
}
