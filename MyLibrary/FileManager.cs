using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyLibrary
{
    public class FileManager
    {
        // Функция для копирования файла в буфер обмена
        public static void CopyToClipboard(string data)
        {
            try
            {
                if (data == null)
                {
                    return;
                }
                Clipboard.Clear();
                Clipboard.SetData("CopiedFile", data.ToString());
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при копировании данных в буфер обмена: " + ex.Message);
            }
        }
        // Функция для получения данных из буфера обмена
        public static void PasteFromClipboard(string targetDirectory)
        {
            try
            {
                // Проверка наличия скопированных данных в буфере обмена
                if (!Clipboard.ContainsData("CopiedFile"))
                {
                    MessageBox.Show("Буфер обмена не содержит скопированных данных.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // Получение пути к скопированному файлу из буфера обмена
                string copiedFilePath = Clipboard.GetData("CopiedFile").ToString();

                if (File.Exists(copiedFilePath))
                {
                    // Получаем имя файла
                    string fileName = Path.GetFileName(copiedFilePath);

                    // Формируем путь для вставки
                    string destinationPath = Path.Combine(targetDirectory, fileName);

                    if (File.Exists(destinationPath))
                    {
                        // Если файл уже существует, запрашиваем подтверждение замены
                        var result = MessageBox.Show($"Файл '{fileName}' уже существует. Заменить его?", "Подтверждение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                        if (result == DialogResult.Yes)
                        {
                            File.Copy(copiedFilePath, destinationPath, true); // Замена файла
                            MessageBox.Show("Файл успешно заменен.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else if (result == DialogResult.No)
                        {
                            // Генерация уникального имени файла в случае отказа от замены
                            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
                            string fileExtension = Path.GetExtension(fileName);

                            int count = 1;
                            string newFileName = fileName;

                            while (File.Exists(Path.Combine(targetDirectory, newFileName)))
                            {
                                newFileName = $"{fileNameWithoutExtension}_{count}{fileExtension}";
                                count++;
                            }

                            // Формируем новый путь с уникальным именем файла
                            string destinationPathWithIncrement = Path.Combine(targetDirectory, newFileName);

                            File.Copy(copiedFilePath, destinationPathWithIncrement); // Копирование файла с новым именем

                            MessageBox.Show($"Файл успешно скопирован с новым именем: {newFileName}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        File.Copy(copiedFilePath, destinationPath); // Копирование файла в целевую директорию
                        MessageBox.Show("Файл успешно вставлен.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Невозможно вставить данные из буфера обмена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при вставке данных из буфера обмена: " + ex.Message);
            }
        }

        // Функция для копирования папки в буфер обмена
        public static void CopyFolderToClipboard(string sourceFolder)
        {
            try
            {
                if (!Directory.Exists(sourceFolder))
                {
                    return;
                }

                Clipboard.Clear();
                Clipboard.SetData("CopiedFolder", sourceFolder);
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при копировании папки в буфер обмена: " + ex.Message);
            }
        }

        // Функция для вставки данных из буфера обмена в виде папки
        public static void PasteFolderFromClipboard(string targetDirectory)
        {
            try
            {
                if (!Clipboard.ContainsData("CopiedFolder"))
                {
                    MessageBox.Show("Буфер обмена не содержит скопированной папки.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                string copiedFolderPath = Clipboard.GetData("CopiedFolder").ToString();

                if (Directory.Exists(copiedFolderPath))
                {
                    // Получаем имя папки
                    string folderName = new DirectoryInfo(copiedFolderPath).Name;

                    // Формируем путь для вставки
                    string destinationPath = Path.Combine(targetDirectory, folderName);

                    if (Directory.Exists(destinationPath))
                    {
                        // Если файл уже существует, запрашиваем подтверждение замены
                        var result = MessageBox.Show($"Папка '{folderName}' уже существует. Заменить её?", "Подтверждение", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                        if (result == DialogResult.Yes)
                        {
                            Directory.Delete(destinationPath, true); // Удаляем существующую папку
                            DirectoryCopy(copiedFolderPath, destinationPath); // Рекурсивное копирование папки

                            MessageBox.Show("Папка успешно заменена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else if (result == DialogResult.No)
                        {
                            string newFolderName = GetUniqueFolderName(targetDirectory, folderName);
                            string newDestinationPath = Path.Combine(targetDirectory, newFolderName);

                            DirectoryCopy(copiedFolderPath, newDestinationPath); // Копируем папку с новым именем

                            MessageBox.Show($"Папка успешно скопирована с новым именем: {newFolderName}", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        DirectoryCopy(copiedFolderPath, destinationPath); // Рекурсивное копирование папки

                        MessageBox.Show("Папка успешно скопирована.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                else
                {
                    MessageBox.Show("Невозможно вставить данные из буфера обмена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при вставке данных из буфера обмена: " + ex.Message);
            }
        }

        // Возвращает уникальное имя для папки, если она уже существует в целевой директории
        private static string GetUniqueFolderName(string targetDirectory, string folderName)
        {
            string newFolderName = folderName;
            int count = 1;

            while (Directory.Exists(Path.Combine(targetDirectory, newFolderName)))
            {
                newFolderName = $"{folderName}_{count}";
                count++;
            }

            return newFolderName;
        }

        // Вспомогательная функция для рекурсивного копирования папки
        private static void DirectoryCopy(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);// Создание целевой директории, если она не существует
            // Копирование файлов из исходной папки в целевую папку
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);// Получаем имя файла
                string destFile = Path.Combine(destDir, fileName);// Формируем путь к файлу в целевой папке
                File.Copy(file, destFile, true);// Копируем файл
            }
            // Рекурсивное копирование подпапок
            foreach (string subDir in Directory.GetDirectories(sourceDir))
            {
                string dirName = new DirectoryInfo(subDir).Name;// Получаем имя подпапки
                string destSubDir = Path.Combine(destDir, dirName);// Формируем путь к подпапке в целевой папке
                DirectoryCopy(subDir, destSubDir);// Рекурсивно копируем содержимое подпапки
            }
        }
        // Функция для удаления файла
        public static void DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    MessageBox.Show("Файл успешно удален.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Файл не найден.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при удалении файла: " + ex.Message);
            }
        }

        public static void DeleteFolder(string folderPath)
        {
            try
            {
                if (Directory.Exists(folderPath))
                {
                    Directory.Delete(folderPath, true); // Удаляем папку рекурсивно
                    MessageBox.Show("Папка успешно удалена.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Папка не найдена.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при удалении папки: " + ex.Message);
            }
        }

        // Функция для переименования файла
        public static void RenameFile(string filePath)
        {
            // Показываем форму для ввода нового имени файла
            Form1 renameForm = new Form1();
            if (renameForm.ShowDialog() == DialogResult.OK)
            {
                string newName = renameForm.NewName; // Получаем новое имя из формы
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    string directory = Path.GetDirectoryName(filePath);
                    string newFilePath = Path.Combine(directory, newName);
                    if (!File.Exists(newFilePath))
                    {
                        File.Move(filePath, newFilePath);
                        MessageBox.Show("Файл успешно переименован.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Файл с таким именем уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Имя файла не может быть пустым.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Функция для переименования папки
        public static void RenameFolder(string folderPath)
        {
            // Показываем форму для ввода нового имени папки
            Form1 renameForm = new Form1();
            if (renameForm.ShowDialog() == DialogResult.OK)
            {
                string newName = renameForm.NewName; // Получаем новое имя из формы
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    string parentDirectory = Directory.GetParent(folderPath).FullName;
                    string newFolderPath = Path.Combine(parentDirectory, newName);
                    if (!Directory.Exists(newFolderPath))
                    {
                        Directory.Move(folderPath, newFolderPath);
                        MessageBox.Show("Папка успешно переименована.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show("Папка с таким именем уже существует.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Имя папки не может быть пустым.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
