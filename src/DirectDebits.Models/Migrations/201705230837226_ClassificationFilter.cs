namespace DirectDebits.Models.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ClassificationFilter : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Organisation", "ClassificationFilterID", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Organisation", "ClassificationFilterID");
        }
    }
}
