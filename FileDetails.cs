using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace total_comander_v2
{
    public class FileDetails
    {
        public string Name { get; set; } // Имя файла
        public string Type { get; set; } // Тип (Папка/Файл)
        public DateTime Date { get; set; } // Дата изменения
        public long Size { get; set; } // Размер файла (в байтах)
    }
}
