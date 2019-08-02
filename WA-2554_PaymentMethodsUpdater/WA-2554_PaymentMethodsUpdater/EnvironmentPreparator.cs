using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WA_2554_PaymentMethodsUpdater
{
    class EnvironmentPreparator
    {
        private DatabaseDao DatabaseDao { get; set; }
        private FileDao FileDao { get; set; }

        public IEnumerable<string> Logins { get; set; }
        public IEnumerable<Guid> Products { get; set; }
        public string NewTariffTable { get; private set; }

        public EnvironmentPreparator()
        {
            DatabaseDao = new DatabaseDao();
            FileDao = new FileDao();
        }
        public void PrepareDatabase()
        {
            DatabaseDao.CreateDatabaseObjects();
        }

        public void ClearDatabase()
        {
            DatabaseDao.DropDatabaseObjects();
        }

        public void InitializeData(string path)
        {
            Logins = FileDao.GetLogins(path);
            Products = FileDao.GetProducts();
            NewTariffTable = FileDao.GetNewValue();
        }

        public void PrepareThreadPool()
        {
            int minThreads, minIOC, maxThreads, maxIOC, procCount;
            ThreadPool.GetMinThreads(out minThreads, out minIOC);
            ThreadPool.GetMaxThreads(out maxThreads, out maxIOC);
            procCount = Environment.ProcessorCount;
            maxThreads = Math.Max(Math.Min(10, procCount), minThreads);
            ThreadPool.SetMaxThreads(maxThreads, maxIOC);
        }

        public void WaitForThreads()
        {
            int maxThreads = 0;
            int placeHolder = 0;
            int availThreads = 0;

            //Now wait until all threads from the Threadpool have returned
            while (true)
            {
                //figure out what the max worker thread count it
                System.Threading.ThreadPool.GetMaxThreads(out
                                     maxThreads, out placeHolder);
                System.Threading.ThreadPool.GetAvailableThreads(out availThreads,
                                                               out placeHolder);

                if (availThreads == maxThreads) break;
                // Sleep
                System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(1000));
            }
            // You can add logic here to log timeouts
        }
    }
}
