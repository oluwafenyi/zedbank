using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using zedbank.Database;

namespace zedbankInterestWorker
{
    public static class Database
    {
        public static readonly Context Context = GetContext();
        
        private static Context GetContext()
        {
            var config = ConfigurationManager.Config;
            var opts = new DbContextOptionsBuilder<Context>()
                .UseSqlServer(config.GetConnectionString("DATABASE_URL")).Options;
            return new Context(opts);
        }   
    }
}