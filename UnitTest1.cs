using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using ETG.SABENTISpro.Models.Core.Locale;
using TestBrokenEf.Model;
using Z.EntityFramework.Extensions;

namespace TestBrokenEf
{
    [TestClass]
    public class UnitTest1
    {
        /// <summary>
        /// 
        /// </summary>
        private string ConnectionString = "Data Source=DESKTOP-DP110GP\\MSSQL2019;User ID=;Password=;MultipleActiveResultSets=False;Trusted_Connection=True;Initial Catalog=testef";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public DbConnection GetConnection(string connectionString)
        {
            var factory = DbProviderFactories.GetFactory("System.Data.SqlClient");
            var cnn = factory.CreateConnection();

            if (cnn == null)
            {
                throw new Exception("Connection cannot be null.");
            }

            cnn.ConnectionString = connectionString;

            return cnn;
        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TestMethod1()
        {
            this.DeployEntityFrameworkExtensionsLicense();

            Database.SetInitializer<DBModelTest>(null);

            var connection = this.GetConnection(this.ConnectionString);

            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V6_0);
            modelBuilder.Configurations.Add(new CORE_LOCALES_SOURCEConfiguration());
            var model = modelBuilder.Build(this.GetConnection(this.ConnectionString));
            var compiledModel = model.Compile();

            var sqlConnection = new SqlConnection(this.ConnectionString);

            this.CreateTables();

            using (var db = new DBModelTest(sqlConnection, compiledModel, true))
            {
                var sources = new List<CORE_LOCALES_SOURCE>();

                sources.Add(new CORE_LOCALES_SOURCE()
                {
                    source = "my Líteral source - string"
                });

                sources.Add(new CORE_LOCALES_SOURCE()
                {
                    source = "my Líteral source 2 - string"
                });

                db.BulkMerge(
                    sources,
                    op =>
                    {
                        op.ColumnPrimaryKeyExpression = obj => obj.source;

                        op.ColumnInputExpression = obj => obj.source;

                        op.ColumnOutputExpression = obj => obj.id;
                    });

                Assert.AreNotEqual(Guid.Empty, sources.First().id);

                sources = new List<CORE_LOCALES_SOURCE>();

                sources.Add(new CORE_LOCALES_SOURCE()
                {
                    source = "my Líteral source - string"
                });

                sources.Add(new CORE_LOCALES_SOURCE()
                {
                    source = "my Líteral source 2 - string"
                });


                db.BulkMerge(
                    sources,
                    op =>
                    {
                        op.ColumnPrimaryKeyExpression = obj => obj.source;

                        op.ColumnInputExpression = obj => obj.source;

                        op.ColumnOutputExpression = obj => obj.id;
                    });

                Assert.AreNotEqual(Guid.Empty, sources.First().id);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void CreateTables()
        {
            using (var sqlConnection = new SqlConnection(this.ConnectionString))
            {
                sqlConnection.Open();

                var cmd = sqlConnection.CreateCommand();

                cmd.CommandText = @"
DROP TABLE IF EXISTS CORE_LOCALES_SOURCE;

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CORE_LOCALES_SOURCE]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[CORE_LOCALES_SOURCE](
	[id] [uniqueidentifier] NOT NULL,
	[source] [nvarchar](max) COLLATE Latin1_General_CS_AS NOT NULL ,
	[cacheStrategy] [int] NOT NULL,
	[length]  AS (len([source])),
	[sourceHash]  AS (CONVERT([varchar](40),hashbytes('SHA1',[source]),(2))) PERSISTED,
 CONSTRAINT [PK_N_CORE_LOCALES_SOURCE_1] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
;

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF_N_CORE_LOCALES_SOURCE_id]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[CORE_LOCALES_SOURCE] ADD  CONSTRAINT [DF_N_CORE_LOCALES_SOURCE_id]  DEFAULT (newsequentialid()) FOR [id]
END

;

IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[dbo].[DF_N_CORE_LOCALES_SOURCE_cache_strategy]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[CORE_LOCALES_SOURCE] ADD  CONSTRAINT [DF_N_CORE_LOCALES_SOURCE_cache_strategy]  DEFAULT ((0)) FOR [cacheStrategy]
END

ALTER TABLE [dbo].[CORE_LOCALES_SOURCE] ADD CONSTRAINT [UQ_CORE_LOCALES_SOURCE_SOURCE] UNIQUE NONCLUSTERED ([sourceHash])


";
                cmd.ExecuteNonQuery();
            }
        }

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Base64Decode
        /// </summary>
        /// <param name="base64EncodedData"></param>
        /// <returns></returns>
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        protected void DeployEntityFrameworkExtensionsLicense()
        {
            var licenseName = "";
            var licenseKey = "";

            licenseName = Base64Decode(licenseName);
            licenseKey = Base64Decode(licenseKey);

            // Set the EntityFrameworkExtensions plugins
            LicenseManager.AddLicense(licenseName, licenseKey);

            LicenseManager.ValidateLicense(out var error, Z.BulkOperations.ProviderType.SqlServer);
            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new Exception("Unable to deploy Entity Framework Extensions license.");
            }
        }

    }
}
