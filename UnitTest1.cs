using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using ETG.SABENTISpro.Models.Core;
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
        private string ConnectionString = "";

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

        [TestMethod]
        public void TestMethod3()
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
                db.CORE_LOCALES_SOURCE
                    .Where(i => i.source.Contains(" "))
                    .UpdateFromQuery(i => new CORE_LOCALES_SOURCE() { source = i.source.Replace(" ", string.Empty) });
            }

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

        [TestMethod]
        public void TestMethod2()
        {
            this.DeployEntityFrameworkExtensionsLicense();

            Database.SetInitializer<DBModelTest>(null);

            var connection = this.GetConnection(this.ConnectionString);

            var modelBuilder = new DbModelBuilder(DbModelBuilderVersion.V6_0);
            modelBuilder.Configurations.Add(new FIRMADIGITAL_FIRMANTEConfiguration());
            modelBuilder.Configurations.Add(new FIRMADIGITAL_SOLICITUDConfiguration());
            var model = modelBuilder.Build(this.GetConnection(this.ConnectionString));
            var compiledModel = model.Compile();

            var sqlConnection = new SqlConnection(this.ConnectionString);

            this.CreateTables2();

            using (var db = new DBModelTest(sqlConnection, compiledModel, true))
            {
                var solicitud = new FIRMADIGITAL_SOLICITUD()
                {
                    fk_core_person_solicitante = Guid.NewGuid(),
                    name = "nombre",
                    description = "descripcion",
                    createdAt = 123456789
                };

                db.BulkInsert(new List<object>() { solicitud });

                Assert.AreNotEqual(Guid.Empty, solicitud.id);

                var sources = new List<FIRMADIGITAL_FIRMANTE>();

                sources.Add(new FIRMADIGITAL_FIRMANTE()
                {
                    nombre = "prueba xxx",
                    id = default(Guid),
                    cargo = "test",
                    email = "asdgasdg@asdg.com",
                    fk_firmadigital_solicitud = solicitud.id
                });

                db.BulkMerge(sources);

                Assert.AreNotEqual(Guid.Empty, sources.First().id);
                Assert.AreNotEqual(Guid.Empty, sources.Last().id);
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

        /// <summary>
        /// 
        /// </summary>
        private void CreateTables2()
        {
            using (var sqlConnection = new SqlConnection(this.ConnectionString))
            {
                sqlConnection.Open();

                var cmd = sqlConnection.CreateCommand();

                cmd.CommandText = "";

                cmd.CommandText += @"
DROP TABLE IF EXISTS FIRMADIGITAL_FIRMANTE;
DROP TABLE IF EXISTS FIRMADIGITAL_SOLICITUD;

CREATE TABLE [dbo].[FIRMADIGITAL_SOLICITUD](
	[id] [uniqueidentifier] NOT NULL,
	[fk_core_tenant_tenant] [uniqueidentifier] NULL,
	[fk_core_person_solicitante] [uniqueidentifier] NOT NULL,
	[name] [nvarchar](255) NULL,
	[description] [nvarchar](255) NULL,
	[remoteDocumentId] [nvarchar](255) NULL,
	[status] [smallint] NULL,
	[signedAt] [bigint] NULL,
	[expiresAt] [bigint] NULL,
	[revokedAt] [bigint] NULL,
	[createdAt] [bigint] NOT NULL,
	[changedAt] [bigint] NULL,
	[deletedAt] [bigint] NULL,
	[extend] [varbinary](max) NULL,
	[jsonExtend] [nvarchar](max) NULL,
 CONSTRAINT [PK_FIRMADIGITAL_SOLICITUD] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
;

ALTER TABLE [dbo].[FIRMADIGITAL_SOLICITUD] ADD  CONSTRAINT [DF_FIRMADIGITAL_SOLICITUD_id]  DEFAULT (newsequentialid()) FOR [id]
;

;";

                cmd.CommandText += @"

CREATE TABLE [dbo].[FIRMADIGITAL_FIRMANTE](
	[id] [uniqueidentifier] NOT NULL,
	[nombre] [nvarchar](255) NOT NULL,
	[email] [nvarchar](255) NOT NULL,
	[cargo] [nvarchar](255) NULL,
	[signOrder] [smallint] NULL,
	[fk_firmadigital_solicitud] [uniqueidentifier] NOT NULL,
	[fk_core_person_firmante] [uniqueidentifier] NULL,
	[createdAt] [bigint] NULL,
	[changedAt] [bigint] NULL,
	[extend] [varbinary](max) NULL,
	[jsonExtend] [nvarchar](max) NULL,
 CONSTRAINT [PK_FIRMADIGITAL_FIRMANTE] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
;

ALTER TABLE [dbo].[FIRMADIGITAL_FIRMANTE] ADD  CONSTRAINT [DF_FIRMADIGITAL_FIRMANTE_id]  DEFAULT (newsequentialid()) FOR [id]
;

ALTER TABLE [dbo].[FIRMADIGITAL_FIRMANTE]  WITH CHECK ADD  CONSTRAINT [FK_FIRMADIGITAL_FIRMANTE_FIRMADIGITAL_SOLICITUD] FOREIGN KEY([fk_firmadigital_solicitud])
REFERENCES [dbo].[FIRMADIGITAL_SOLICITUD] ([id])
;

ALTER TABLE [dbo].[FIRMADIGITAL_FIRMANTE] CHECK CONSTRAINT [FK_FIRMADIGITAL_FIRMANTE_FIRMADIGITAL_SOLICITUD]
;

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
