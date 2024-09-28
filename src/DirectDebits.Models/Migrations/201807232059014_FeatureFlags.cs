namespace DirectDebits.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FeatureFlags : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organisation", "HasDirectDebitsFeature", c => c.Boolean(nullable: false));
            AddColumn("dbo.Organisation", "HasPaymentsFeature", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Organisation", "HasPaymentsFeature");
            DropColumn("dbo.Organisation", "HasDirectDebitsFeature");
        }
    }
}
