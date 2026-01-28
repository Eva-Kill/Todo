using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Todo.View
{
    /// <summary>
    /// Логика взаимодействия для Main_empty.xaml
    /// </summary>
    public partial class Main_empty : Window
    {
        private const string DefaultPhotoFileName = "фото_умолч.jpg";
        private const string ConfigFolderName = "WeddingAgency";
        private const string PhotoPathFileName = "photo_path.txt";

        private string _currentPhotoPath;
        private BitmapImage _userPhoto;

        public BitmapImage UserPhoto // Свойство для хранения фото пользователя
        {
            get { return _userPhoto; }
            private set { _userPhoto = value; }
        }

        public Main_empty() 
        {
            InitializeComponent();
            ConfigureEventHandlers();
        }

        private void ConfigureEventHandlers() 
        {
            Loaded += HandleWindowLoaded;
            ExitButton.Click += HandleExitButtonClick;
            CreateTaskButton.Click += HandleCreateTaskButtonClick;
            ChangePhotoButton.Click += HandleChangePhotoButtonClick;
        }

        private void HandleWindowLoaded(object sender, RoutedEventArgs e) 
        {
            LoadUserProfilePhoto();
        }

        private void HandleExitButtonClick(object sender, RoutedEventArgs e) 
        {
            CloseCurrentWindow();
        }

        private void HandleCreateTaskButtonClick(object sender, RoutedEventArgs e) 
        {
            OpenTaskCreationDialog();
        }

        private void HandleChangePhotoButtonClick(object sender, RoutedEventArgs e) 
        {
            ChangeProfilePhoto();
        }

        private void CloseCurrentWindow() 
        {
            var window = Window.GetWindow(this);
            window?.Close();
        }

        private void OpenTaskCreationDialog()
        {
            var taskCreationWindow = new Creating_tasks
            {
                Title = "Создание задачи",
                Width = 800,
                Height = 450,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            taskCreationWindow.TaskCreated += HandleNewTaskCreated;
            taskCreationWindow.Closing += (s, args) =>
            {
                taskCreationWindow.TaskCreated -= HandleNewTaskCreated;
            };

            taskCreationWindow.Show();
        }

        private void HandleNewTaskCreated(Todo.View.Main.Task newTask) 
        {
            var mainWindow = new Todo.View.Main();
            mainWindow.AddNewTask(newTask);

            if (UserPhoto != null)
            {
                mainWindow.SetUserPhoto(UserPhoto);
            }

            mainWindow.Show();
            CloseCurrentWindow();
        }

        private void ChangeProfilePhoto() 
        {
            try
            {
                var openFileDialog = CreatePhotoSelectionDialog();
                var dialogResult = openFileDialog.ShowDialog();

                if (dialogResult == true)
                {
                    ProcessSelectedImageFile(openFileDialog.FileName);
                }
                else
                {
                    DisplayInformationMessage("Выбор фотографии отменен.");
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage($"Ошибка при выборе фотографии: {ex.Message}");
            }
        }

        private OpenFileDialog CreatePhotoSelectionDialog() 
        {
            return new OpenFileDialog
            {
                Filter = "Image Files (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp",
                FilterIndex = 1,
                Title = "Выберите фотографию"
            };
        }

        private void ProcessSelectedImageFile(string selectedImagePath) 
        {
            if (!File.Exists(selectedImagePath))
            {
                DisplayErrorMessage("Выбранный файл не существует!");
                return;
            }

            if (LoadAndSetProfileImage(selectedImagePath))
            {
                SavePhotoFilePath(selectedImagePath);
                DisplaySuccessMessage("Фотография успешно изменена!");
            }
        }

        private bool LoadAndSetProfileImage(string imagePath) 
        {
            try
            {
                var bitmapImage = CreateBitmapImageFromFile(imagePath);

                if (UserProfileImage != null)
                {
                    UserProfileImage.Source = bitmapImage;
                    _currentPhotoPath = imagePath;
                    UserPhoto = bitmapImage;
                    return true;
                }
                else
                {
                    DisplayErrorMessage("Элемент для отображения фото не найден!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                DisplayErrorMessage($"Ошибка загрузки изображения: {ex.Message}");
                return false;
            }
        }

        private BitmapImage CreateBitmapImageFromFile(string imagePath) 
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imagePath);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            return bitmap;
        }

        private void LoadDefaultProfilePhoto() 
        {
            try
            {
                if (File.Exists(DefaultPhotoFileName))
                {
                    LoadAndSetProfileImage(DefaultPhotoFileName);
                }
                else
                {
                    DisplayInformationMessage("Файл фото_умолч.jpg не найден. Используется стандартное изображение.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки фото по умолчанию: {ex.Message}");
            }
        }

        private void SavePhotoFilePath(string photoPath) 
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var appFolder = Path.Combine(appDataPath, ConfigFolderName);

                Directory.CreateDirectory(appFolder);

                var configFilePath = Path.Combine(appFolder, PhotoPathFileName);
                File.WriteAllText(configFilePath, photoPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения пути: {ex.Message}");
            }
        }

        private void LoadUserProfilePhoto() 
        {
            try
            {
                var savedPhotoPath = RetrieveSavedPhotoPath();

                if (!string.IsNullOrEmpty(savedPhotoPath) && File.Exists(savedPhotoPath))
                {
                    LoadAndSetProfileImage(savedPhotoPath);
                    return;
                }

                LoadDefaultProfilePhoto();
            }
            catch (Exception ex)
            {
                LoadDefaultProfilePhoto();
                Console.WriteLine($"Ошибка загрузки фото: {ex.Message}");
            }
        }

        private string RetrieveSavedPhotoPath()
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var configFilePath = Path.Combine(appDataPath, ConfigFolderName, PhotoPathFileName);

                if (File.Exists(configFilePath))
                {
                    return File.ReadAllText(configFilePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения сохраненного пути: {ex.Message}");
            }

            return null;
        }

        private void DisplaySuccessMessage(string message)
        {
            MessageBox.Show(message, "Успех!",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DisplayErrorMessage(string message) 
        {
            MessageBox.Show(message, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void DisplayInformationMessage(string message) 
        {
            MessageBox.Show(message, "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    public class TaskItem // Класс для представления элемента задачи
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsCompleted { get; set; }

        public TaskItem(string title, string description, DateTime dueDate) // Конструктор элемента задачи
        {
            Title = title;
            Description = description;
            DueDate = dueDate;
            IsCompleted = false;
        }
    }
}