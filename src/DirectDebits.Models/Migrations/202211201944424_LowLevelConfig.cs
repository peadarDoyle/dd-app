namespace DirectDebits.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LowLevelConfig : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BatchSettings", "LowLevelConfig", c => c.String(nullable: false, maxLength: 12, defaultValue: "000000000000"));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BatchSettings", "LowLevelConfig");
        }
    }
}
