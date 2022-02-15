﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETG.SABENTISpro.Models.Core.Locale;

namespace TestBrokenEf.Model
{
    public class DBModelTest : DbContext
    {
        public DBModelTest(SqlConnection connection, DbCompiledModel compiledModel, bool ownsConnection)
            : base(connection, compiledModel, ownsConnection)
        {
        }

        public DbSet<CORE_LOCALES_SOURCE> Students { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
        }
    }
}
