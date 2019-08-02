using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WA_2554_PaymentMethodsUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            var preparator = new EnvironmentPreparator();
            var argsSet = GetArgs(args);
            string path;
            var start = DateTime.Now;
            argsSet.TryGetValue("-p", out path);
            if (path != null)
            {
                Console.WriteLine("Настройка окружения...");
                preparator.PrepareDatabase();
                preparator.InitializeData(path);

                Console.WriteLine("Обновление ТТ...");
                var PaymentMethodsUpdater = new PaymentMethodsUpdater(preparator.Logins, preparator.Products, preparator.NewTariffTable);
                PaymentMethodsUpdater.Process();
                preparator.WaitForThreads();

                Console.WriteLine("Проверка обработки филиалов...");
                var count = PaymentMethodsUpdater.Check();

                if(count > 0)
                {
                    Console.WriteLine("Обновление необработанных филиалов...");
                    PaymentMethodsUpdater.ProcessNotProcessed();
                    preparator.WaitForThreads();
                }
            }

            string clear;
            argsSet.TryGetValue("-c", out clear);
            if(clear != null)
            {
                Console.WriteLine("Очищение базы...");
                preparator.ClearDatabase();
            }
            var finish = DateTime.Now;
            var delta = finish - start;
            Console.WriteLine("Работа завершена. Выполнение заняло {0}ч. {1}м. {2}c.", delta.Hours, delta.Minutes, delta.Seconds);
        }

        private static Dictionary<string, string> GetArgs(string[] args)
        {
            var set = new Dictionary<string, string>();
            for(int i = 0; i < args.Length; i++)
            {
                var key = args[i];
                if (key.Length > 0 && key[0] == '-')
                {
                    var hasValue = args.Length > i + 1 && args[i + 1].Length > 0 && args[i + 1][0] != '-';
                    var value = hasValue ? args[i + 1] : string.Empty;
                    set.Add(key, value);
                    i++;
                }
            }
            return set;
        }
    }
}
