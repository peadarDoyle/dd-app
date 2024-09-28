using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DirectDebits.Models;
using DirectDebits.Models.Context;

namespace DirectDebits.Tests
{
    [TestClass]
    public class TestBase
    {
        protected static SynergyDbContext db;
        private static string dropSchema;
        private static string createSchema;
        private static string createData;

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            dropSchema = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Persistence\\Scripts\\DropTestSchema.sql");
            createSchema = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Persistence\\Scripts\\CreateTestSchema.sql");
            createData = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Persistence\\Scripts\\CreateTestData.sql");
        }

        [TestInitialize]
        public void Initialize()
        {
            db = SynergyDbContext.Create();

            db.Database.ExecuteSqlCommand(dropSchema);
            db.Database.ExecuteSqlCommand(createSchema);
            db.Database.ExecuteSqlCommand(createData);
        }
    }
}
