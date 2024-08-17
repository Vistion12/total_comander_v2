using GalaSoft.MvvmLight.Command;
using Microsoft.Win32;
using MyLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;


namespace total_comander_v2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private string selectedDrive;
        private bool isLeftListViewActive = false;
        private bool isRightListViewActive = false;
        static string filepath;
        static ListView currentListView;
        private string currentPath;
        private string clipboardItemType;
       
        public MainWindow()
        {
            InitializeComponent();
            SetupSearchTextBox(); // Настройка текстового поля поиска
            FillDriveComboBoxes(); // Заполнение ComboBox'ов доступными дисками
            SubscribeDriveSelectionEvents(comboBox_left, listviewleft, true); // Подписка на события выбора диска в левом ComboBox
            SubscribeDriveSelectionEvents(comboBox_right, listviewright, false); // Подписка на события выбора диска в правом ComboBox
            //метод позволяющий получить путь файла или папки 
            listviewleft.SelectionChanged += get_path;

            // реализация нажатия открыть через контекстное меню
            ContextMenu cm = (ContextMenu)FindResource("MyContextMenu");
            MenuItem openMenuItem = cm.Items.OfType<MenuItem>().FirstOrDefault(x => x.Header.ToString() == "Открыть");
            if (openMenuItem != null)
            {
                openMenuItem.Click += OpenContextMenuItem_Click;
            }

            // Добавление обработчиков событий для пунктов контекстного меню
            MenuItem copyMenuItem = cm.Items.OfType<MenuItem>().FirstOrDefault(x => x.Header.ToString() == "Копировать");
            if (copyMenuItem != null)
            {
                copyMenuItem.Click += CopyContextMenuItem_Click;
            }

            MenuItem pasteMenuItem = cm.Items.OfType<MenuItem>().FirstOrDefault(x => x.Header.ToString() == "Вставить");
            if (pasteMenuItem != null)
            {
                pasteMenuItem.Click += PasteContextMenuItem_Click;
            }

            MenuItem renameMenuItem = cm.Items.OfType<MenuItem>().FirstOrDefault(x => x.Header.ToString() == "Переименовать");
            if (renameMenuItem != null)
            {
                renameMenuItem.Click += RenameContextMenuItem_Click;
            }

            MenuItem deleteMenuItem = cm.Items.OfType<MenuItem>().FirstOrDefault(x => x.Header.ToString() == "Удалить");
            if (deleteMenuItem != null)
            {
                deleteMenuItem.Click += DeleteContextMenuItem_Click;
            }

            // Добавление обработчиков событий для пунктов главного меню
            MenuItem openFileMenuItem = ((MenuItem)mainMenu.Items[0]).Items.OfType<MenuItem>().FirstOrDefault(x => x.Header.ToString() == "Открыть");
            openFileMenuItem.Click += OpenFileMenuItem_Click;

            MenuItem copyFileMenuItem = ((MenuItem)mainMenu.Items[0]).Items.OfType<MenuItem>().FirstOrDefault(x => x.Header.ToString() == "Копировать");
            copyFileMenuItem.Click += CopyFileMenuItem_Click;

            MenuItem pasteFileMenuItem = ((MenuItem)mainMenu.Items[0]).Items.OfType<MenuItem>().FirstOrDefault(x => x.Header.ToString() == "Вставить");
            pasteFileMenuItem.Click += PasteFileMenuItem_Click;

            MenuItem deleteFileMenuItem = ((MenuItem)mainMenu.Items[0]).Items.OfType<MenuItem>().FirstOrDefault(x => x.Header.ToString() == "Удалить");
            deleteFileMenuItem.Click += DeleteFileMenuItem_Click;

            MenuItem renameFileMenuItem = ((MenuItem)mainMenu.Items[0]).Items.OfType<MenuItem>().FirstOrDefault(x => x.Header.ToString() == "Переименовать");
            renameFileMenuItem.Click += RenameFileMenuItem_Click;

            MenuItem exitMenuItem = ((MenuItem)mainMenu.Items[0]).Items.OfType<MenuItem>().FirstOrDefault(x => x.Header.ToString() == "Выход");
            exitMenuItem.Click += ExitMenuItem_Click;

            MenuItem aboutMenuItem = ((MenuItem)mainMenu.Items[1]).Items.OfType<MenuItem>().FirstOrDefault(x => x.Header.ToString() == "О нас");
            aboutMenuItem.Click += AboutMenuItem_Click;

            // Обработчик события KeyDown для textBox_path_left
            textBox_path_left.KeyDown += (sender, e) => HandleEnterKey(textBox_path_left, listviewleft, e);

            // Обработчик события KeyDown для textBox_path_right
            textBox_path_right.KeyDown += (sender, e) => HandleEnterKey(textBox_path_right, listviewright, e);

            // Обработчик события MouseDoubleClick для ListView слева
            listviewleft.MouseDoubleClick += (sender, e) =>
            {
                OpenSelectedItem(listviewleft, ref selectedDrive);
                textBox_path_left.Text = selectedDrive;
            };

            // Обработчик события MouseDoubleClick для ListView справа
            listviewright.MouseDoubleClick += (sender, e) =>
            {
                OpenSelectedItem(listviewright, ref selectedDrive);
                textBox_path_right.Text = selectedDrive;
            };

            // Обработчик события KeyDown для tb_Search (т.е. поиск)
            tb_Search.KeyDown += async (sender, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    string searchKeyword = tb_Search.Text;

                    await SearchAndUpdateListViewAsync(selectedDrive, searchKeyword, isLeftListViewActive ? listviewleft : listviewright);
                }
            };

            // реализация кнопок
            PreviewKeyDown += MainWindow_PreviewKeyDown;
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // Метод для настройки текстового поля поиска
        private void SetupSearchTextBox()
        {
            tb_Search.GotFocus += (sender, e) => HandleTextBoxFocus(tb_Search, "Поиск...", Brushes.Black);
            tb_Search.LostFocus += (sender, e) => HandleTextBoxFocus(tb_Search, "Поиск...", Brushes.Gray, true);
        }
        // Метод для обработки фокуса текстового поля
        private void HandleTextBoxFocus(TextBox textBox, string defaultText, Brush color, bool lostFocus = false)
        {
            if (lostFocus && string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = defaultText;
                textBox.Foreground = color;
            }
            else if (!lostFocus && textBox.Text == defaultText)
            {
                textBox.Text = "";
                textBox.Foreground = color;
            }
        }

        // Метод для заполнения ComboBox'ов дисками
        private void FillDriveComboBoxes()
        {
            string[] drives = Directory.GetLogicalDrives();
            foreach (string drive in drives)
            {
                comboBox_left.Items.Add(CreateComboBoxItem(drive));
                comboBox_right.Items.Add(CreateComboBoxItem(drive));
            }
        }
        // Метод для создания элемента ComboBoxItem
        private ComboBoxItem CreateComboBoxItem(string content)
        {
            return new ComboBoxItem() { Content = content };
        }

        // Метод для подписки на событие выбора диска в ComboBox
        private void SubscribeDriveSelectionEvents(ComboBox comboBox, ListView listView, bool isLeft)
        {
            comboBox.SelectionChanged += (sender, e) =>
            {
                ComboBox selectedComboBox = sender as ComboBox;
                selectedDrive = (selectedComboBox.SelectedItem as ComboBoxItem).Content.ToString();
                isLeftListViewActive = isLeft;
                isRightListViewActive = !isLeft;
                UpdateListView(listView, selectedDrive);
            };
        }

        // Метод для обновления содержимого ListView при выборе диска        
        private void UpdateListView(ListView listView, string drive)
        {
            try
            {
                listView.ItemsSource = null; // Сбросить текущие данные ListView
                listView.Items.Clear(); // Очистить содержимое ListView

                List<FileDetails> files = new List<FileDetails>();

                // Добавление элемента по умолчанию
                FileDetails parentDirectoryItem = new FileDetails { Name = "..", Type = "", Date = DateTime.Now, Size = 0 };
                files.Add(parentDirectoryItem);

                DirectoryInfo dirInfo = new DirectoryInfo(drive); // Получение информации о каталоге

                foreach (var item in dirInfo.GetFileSystemInfos())
                {
                    if ((item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden)
                    {
                        if (item is DirectoryInfo)
                        {
                            DirectoryInfo directory = item as DirectoryInfo;
                            files.Add(new FileDetails { Name = directory.Name, Type = "Папка", Date = directory.LastWriteTime, Size = 0 });
                        }
                        else if (item is FileInfo)
                        {
                            FileInfo file = item as FileInfo;
                            long fileSizeKB = file.Length / 1024;
                            files.Add(new FileDetails { Name = file.Name, Type = "Файл", Date = file.LastWriteTime, Size = fileSizeKB });
                        }
                    }
                }
                listView.ItemsSource = files; // Привязка нового списка файлов к ListView
            }
            catch (Exception ex)
            {
                MessageBox.Show("Разрушительный сбой!!!Ошибка: " + ex.Message); // Обработка ошибок
                 // Получение информации о месте возникновения исключения
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(0);
                var lineNumber = frame.GetFileLineNumber();
                var methodName = frame.GetMethod().Name;

                LogExceptionToRegistry(ex, methodName, lineNumber); // Логирование исключения в реестр с указанием места
            }
        }

        //метод позволяющий получить путь файла или папки 
        private void get_path(object sender, SelectionChangedEventArgs e)
        {
            currentListView = sender as ListView;
            FileDetails selectedFile = currentListView.SelectedItem as FileDetails;

            if (selectedDrive != null && selectedFile != null)
            {
                filepath = System.IO.Path.Combine(selectedDrive, selectedFile.Name);
            }
        }


        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Привет вы используете частную интелектуальную собственность, возьмите на работу а?", "О нас");
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void RenameFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RenameButton_Click(sender, e);
        }

        private void DeleteFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteButton_Click(sender, e);
        }

        private void PasteFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PasteButton_Click(sender, e);
        }

        private void CopyFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CopyButton_Click(sender, e);
        }

        private void OpenFileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenButton_Click(sender, e);
        }

        private void DeleteContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            DeleteButton_Click(sender, e);
        }

        private void RenameContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            RenameButton_Click(sender, e);
        }

        private void PasteContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            PasteButton_Click(sender, e);
        }

        private void CopyContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            CopyButton_Click(sender, e);
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.F1:
                    OpenButton_Click(sender, e); 
                    break;
                case Key.F2:
                    CopyButton_Click(sender, e); 
                    break;
                case Key.F3:
                    PasteButton_Click(sender, e); 
                    break;
                case Key.F4:
                    DeleteButton_Click(sender, e); 
                    break;
                case Key.F5:
                    RenameButton_Click(sender, e); 
                    break;
                default:
                    
                    break;
            }
        }      
                 
        private void OpenSelectedItem(ListView listView, ref string selectedDrive)
        {
            if (listView.SelectedItem != null)
            {
                FileDetails selectedFile = listView.SelectedItem as FileDetails;

                if (selectedFile.Name == "..")
                {
                    DirectoryInfo parentDirectory = Directory.GetParent(selectedDrive);
                    if (parentDirectory != null)
                    {
                        selectedDrive = parentDirectory.FullName;
                        UpdateListView(listView, selectedDrive);
                    }
                }
                else
                {
                    string fullPath = System.IO.Path.Combine(selectedDrive, selectedFile.Name);

                    if (selectedFile.Type == "Папка")
                    {
                        if (Directory.Exists(fullPath))
                        {
                            selectedDrive = fullPath;
                            UpdateListView(listView, selectedDrive);
                            currentPath = selectedDrive;
                        }
                        else
                        {
                            MessageBox.Show("Разрушительный сбой!!! Папка не существует: " + fullPath);
                        }
                    }
                    else if (selectedFile.Type == "Файл")
                    {
                        if (File.Exists(fullPath))
                        {
                            System.Diagnostics.Process.Start(fullPath);
                            currentPath = System.IO.Path.GetDirectoryName(fullPath);
                        }
                        else
                        {
                            MessageBox.Show("Разрушительный сбой!!!Файл не существует: " + fullPath);
                        }
                    }
                }
            }
        }

       

        // Обработчик события нажатия кнопки "Переименовать"
        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            FileDetails selectedFile = isLeftListViewActive ? listviewleft.SelectedItem as FileDetails : listviewright.SelectedItem as FileDetails;

            if (selectedFile != null)
            {
                string fullPath = Path.Combine(selectedDrive, selectedFile.Name);

                if (selectedFile.Type == "Файл")
                {
                    FileManager.RenameFile(fullPath);
                }
                else if (selectedFile.Type == "Папка")
                {
                    FileManager.RenameFolder(fullPath);
                }

                // Здесь можно обновить ListView по необходимости
                UpdateListView(isLeftListViewActive ? listviewleft : listviewright, selectedDrive);
            }
        }

        // Обработчик события нажатия кнопки "Копировать"
        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            FileDetails selectedFile = isLeftListViewActive ? listviewleft.SelectedItem as FileDetails : listviewright.SelectedItem as FileDetails;

            if (selectedFile != null)
            {
                clipboardItemType = selectedFile.Type; // Сохраняем тип элемента при копировании
                if (selectedFile.Type == "Файл")
                {
                    FileManager.CopyToClipboard(Path.Combine(selectedDrive, selectedFile.Name));
                }
                else if (selectedFile.Type == "Папка")
                {
                    FileManager.CopyFolderToClipboard(Path.Combine(selectedDrive, selectedFile.Name));
                }
            }

        }
        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            if (clipboardItemType == "Файл")
            {
                FileManager.PasteFromClipboard(selectedDrive);
            }
            else if (clipboardItemType == "Папка")
            {
                FileManager.PasteFolderFromClipboard(selectedDrive);
            }

            UpdateListView(isLeftListViewActive ? listviewleft : listviewright, selectedDrive);
        }

        // Обработчик события нажатия кнопки "Удалить"
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            FileDetails selectedFile = isLeftListViewActive ? listviewleft.SelectedItem as FileDetails : listviewright.SelectedItem as FileDetails;

            if (selectedFile != null)
            {
                string fullPath = Path.Combine(selectedDrive, selectedFile.Name);

                if (selectedFile.Type == "Файл")
                {
                    FileManager.DeleteFile(fullPath);
                }
                else if (selectedFile.Type == "Папка")
                {
                    FileManager.DeleteFolder(fullPath);
                }

                // Здесь можно обновить ListView по необходимости
                UpdateListView(isLeftListViewActive ? listviewleft : listviewright, selectedDrive);
            }
        }

        private void OpenContextMenuItem_Click(object sender, RoutedEventArgs e)
        {
            OpenButton_Click(sender, e);            
        }

        // Обработчик события нажатия кнопки "Открыть"
        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            if (isLeftListViewActive)
            {
                OpenSelectedItem(listviewleft, ref selectedDrive);
                textBox_path_left.Text = selectedDrive; // Обновление пути в текстовом поле после открытия
            }
            else
            {
                OpenSelectedItem(listviewright, ref selectedDrive);
                textBox_path_right.Text = selectedDrive; // Обновление пути в текстовом поле после открытия
            }
        }     
        // Асинхронный метод для поиска файлов и обновления ListView
        private async Task SearchAndUpdateListViewAsync(string selectedDrive, string searchKeyword, ListView listView)
        {
            try
            {
                List<FileDetails> searchResults = new List<FileDetails>();

                await Task.Run(() =>
                {
                    try
                    {
                        foreach (string directory in Directory.GetDirectories(selectedDrive, $"*{searchKeyword}*", SearchOption.AllDirectories))
                        {
                            DirectoryInfo dirInfo = new DirectoryInfo(directory);
                            string relativePath = dirInfo.FullName.Replace(selectedDrive, "");
                            searchResults.Add(new FileDetails { Name = relativePath, Type = "Папка", Date = dirInfo.LastWriteTime, Size = 0 });
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        // Логирование исключения
                        Console.WriteLine($"Отказано в доступе: {ex.Message}");
                    }
                    try
                    {
                        foreach (string file in Directory.GetFiles(selectedDrive, $"*{searchKeyword}.*", SearchOption.AllDirectories))
                        {
                            FileInfo fileInfo = new FileInfo(file);
                            string relativePath = fileInfo.FullName.Replace(selectedDrive, "");
                            long fileSizeKB = fileInfo.Length / 1024;
                            searchResults.Add(new FileDetails { Name = relativePath, Type = "Файл", Date = fileInfo.LastWriteTime, Size = fileSizeKB });
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        // Логирование исключения
                        Console.WriteLine($"Отказано в доступе: {ex.Message}");
                    }
                });

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    listView.ItemsSource = null;
                    listView.Items.Clear();
                    listView.ItemsSource = searchResults;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Разрушительный сбой!!!", MessageBoxButton.OK, MessageBoxImage.Error);
                // Дополнительная обработка исключений
            }
        }


        // Метод для обработки нажатия клавиши Enter
        private void HandleEnterKey(TextBox textBox, ListView listView, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string newPath = textBox.Text;
                if (Directory.Exists(newPath))
                {
                    selectedDrive = newPath;
                    UpdateListView(listView, selectedDrive);
                }
                else
                {
                    MessageBox.Show("Разрушительный сбой!!!Проверьте корректность пути");
                }
            }
        }
        // Логирование информации об исключениях в реестр с указанием места возникновения
        private void LogExceptionToRegistry(Exception ex, string methodName, int lineNumber)
        {
            try
            {
                RegistryKey key = Registry.CurrentUser.CreateSubKey("Software\\total_comander_v2\\ErrorLog"); // Создание или открытие ключа реестра

                string errorInfo = String.Format("Ошибка: {0}\nМесто: Метод {1}, строка {2}", ex.Message, methodName, lineNumber);

                key.SetValue(DateTime.Now.ToString(), errorInfo); // Запись данных в реестр с указанием времени и информации об ошибке
                key.Close(); // Закрытие ключа реестра
            }
            catch (Exception logEx)
            {
                MessageBox.Show("Ошибка при логировании исключения в реестр: " + logEx.Message);
            }
        }

    }
}
