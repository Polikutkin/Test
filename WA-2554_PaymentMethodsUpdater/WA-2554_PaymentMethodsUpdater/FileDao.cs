using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WA_2554_PaymentMethodsUpdater
{
    class FileDao
    {
        public IEnumerable<string> GetLogins(string path)
        {

            try
            {
                FileInfo fi = new FileInfo(path);

                if (!fi.Exists)
                {
                    Console.WriteLine("Файл не существует");
                    yield break;
                }

                if (IsFileLocked(fi))
                {
                    Console.WriteLine("Файл занят другим процессом");
                    yield break;
                }

                using (ExcelPackage excelPackage = new ExcelPackage(fi))
                {
                    ExcelWorksheet firstWorksheet = excelPackage.Workbook.Worksheets[1];
                    int i = 2; //пропускаем заголовки
                    string login = null;
                    do
                    {
#if DEBUG
                        if (i % 100 == 0)
                        {
                            Console.WriteLine("{0}: {1}", i, DateTime.Now);
                        }
#else
                        if (i % 1000 == 0)
                        {
                            Console.WriteLine("Обработано записей: {0}", i);
                        }
#endif

                        login = firstWorksheet.Cells[i, 1].Value as string;
                        if (!string.IsNullOrEmpty(login))
                        {
                            yield return login;
                        }
                        i++;
                    } while (!string.IsNullOrEmpty(login));
                }
            }
            finally
            {
            }

            yield break;
        }

        internal string GetNewValue()
        {
            FileInfo fi = new FileInfo(Resources.NewTariffTablePath);
            if (IsFileLocked(fi))
            {
                Console.WriteLine("Новое значение не доступно");
                return string.Empty;
            }

            var value = File.ReadAllText(fi.FullName);
            return value;
        }

        protected virtual bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;

            try
            {
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }

        internal IEnumerable<Guid> GetProducts()
        {
            FileInfo fi = new FileInfo(Resources.ProductsListPath);
            if(IsFileLocked(fi))
            {
                Console.WriteLine("Список продуктов недоступен");
                return new List<Guid>();
            }

            var stringList = File.ReadAllText(fi.FullName);
            var products = stringList.Split(new[] { Environment.NewLine, "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Select(x => new Guid(x)).ToArray();
            return products;
        }
    }
}
