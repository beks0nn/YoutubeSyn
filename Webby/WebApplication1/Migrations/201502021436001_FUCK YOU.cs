namespace WebApplication1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FUCKYOU : DbMigration
    {
        public override void Up()
        {

            CreateTable(
                "dbo.Uris",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UrlPart = c.String(),
                        UrlIdentity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CurrUrls", t => t.UrlIdentity, cascadeDelete: true)
                .Index(t => t.UrlIdentity);
            
            CreateTable(
                "dbo.CurrUrls",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UrlIdentity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.CurrentUrls",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UrlIdentity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Urls",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UrlPart = c.String(),
                        UrlIdentity = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.Uris", "UrlIdentity", "dbo.CurrUrls");
            DropIndex("dbo.Uris", new[] { "UrlIdentity" });
            DropTable("dbo.CurrUrls");
            DropTable("dbo.Uris");
            CreateIndex("dbo.Urls", "UrlIdentity");
            AddForeignKey("dbo.Urls", "UrlIdentity", "dbo.CurrentUrls", "Id", cascadeDelete: true);
        }
    }
}
