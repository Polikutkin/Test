using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WA_2554_PaymentMethodsUpdater
{
    class PaymentMethodsUpdater
    {
        private EnvironmentPreparator Preparator { get;set;}

        private int OriginalLoginsCount = 0;
        private FileDao FileDao { get; set; }
        private DatabaseDao DatabaseDao { get; set; }
        private IEnumerable<string> Logins { get; set; }
        private IEnumerable<Guid> Products { get; set; }
        private string NewTariffTable { get; set; }

        private ICollection<Guid> ProcessedFilials = new HashSet<Guid>();
        private ICollection<Guid> NotProcessedFilials = new HashSet<Guid>();

        public PaymentMethodsUpdater(string pathToLogins)
        {
            FileDao = new FileDao();
            DatabaseDao = new DatabaseDao();
            Logins = FileDao.GetLogins(pathToLogins);
            Products = FileDao.GetProducts();
            NewTariffTable = FileDao.GetNewValue();
        }

        public PaymentMethodsUpdater(IEnumerable<string> logins, IEnumerable<Guid> products, string newValue)
        {
            FileDao = new FileDao();
            DatabaseDao = new DatabaseDao();
            Logins = logins;
            Products = products;
            NewTariffTable = newValue;
        }

        internal int Check()
        {
            var countNotProcessed = 0;
            NotProcessedFilials = new HashSet<Guid>();
            foreach (var filial in ProcessedFilials)
            {
                var updated = DatabaseDao.WasFilialProcessed(filial);
                if (!updated)
                {
                    NotProcessedFilials.Add(filial);
                    countNotProcessed++;
                }
            }

            Console.WriteLine("Не обработано филиалов: {0}", countNotProcessed);
            return countNotProcessed;
        }


        public void Process()
        {
#if DEBUG
            Console.WriteLine(DateTime.Now);
#endif

            foreach (var login in Logins)
            {
                if (!login.Equals("ОТСУТСТВУЕТ", StringComparison.CurrentCultureIgnoreCase))
                {
                    OriginalLoginsCount++;
                    var parentId = DatabaseDao.GetParentIdByLogin(login);
                    UpdateFilial(parentId);
                }
            }
#if DEBUG
            Console.WriteLine("{0}: {1}, {2}", DateTime.Now, OriginalLoginsCount, ProcessedFilials.Count);
#endif
        }

        public void ProcessNotProcessed()
        {
#if DEBUG
            Console.WriteLine(DateTime.Now);
#endif

            foreach (var filial in NotProcessedFilials)
            {
                UpdateFilial(filial);
            }
#if DEBUG
            Console.WriteLine("{0}: {1}, {2}", DateTime.Now, OriginalLoginsCount, ProcessedFilials.Count);
#endif
        }
                
        private void UpdateFilial(Guid parentId)
        {
            if (parentId != Guid.Empty)
            {
                var updated = ProcessedFilials.Contains(parentId);

                if (!updated)
                {
                    ProcessedFilials.Add(parentId);
                    ThreadPool.QueueUserWorkItem(x =>
                    {
                        DatabaseDao.UpdateAllProductsForFilial(parentId, Products, NewTariffTable);
                        DatabaseDao.AddFilial(parentId);                        
                    });
                }
            }
        }
    }
}
