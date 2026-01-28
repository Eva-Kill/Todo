using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Todo;

namespace Todo.View
{
    /// <summary>
    /// Логика взаимодействия для Main.xaml
    /// </summary>
    public partial class Main : Window
    {
        private string _currentSelectedTaskId;
        private readonly Dictionary<string, Task> _activeTasks;
        private readonly Dictionary<string, Task> _completedTasks;
        private string _currentCategory = "Все";
        private bool _isHistoryMode = false;
        private BitmapImage _userPhoto;

        public class Task // Класс для представления задачи
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public string Time { get; set; }
            public string Date { get; set; }
            public string Description { get; set; }
            public string Category { get; set; }
            public bool IsCompleted { get; set; }
            public Border TaskBorder { get; set; }
            public CheckBox TaskCheckBox { get; set; }
            public TextBlock TitleTextBlock { get; set; }
        }

        public Main() // Конструктор главного окна приложения
        {
            InitializeComponent();
            _activeTasks = new Dictionary<string, Task>();
            _completedTasks = new Dictionary<string, Task>();

            LoadUserProfilePhoto();
            InitializeTaskDisplay();
        }

        public void SetUserPhoto(BitmapImage photo) // Устанавливает фото пользователя
        {
            _userPhoto = photo;
            UpdateProfilePhotoDisplay();
        }

        private void LoadUserProfilePhoto() // Загружает фото профиля пользователя
        {
            try
            {
                var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var configFile = System.IO.Path.Combine(appDataPath, "WeddingAgency", "photo_path.txt");

                if (File.Exists(configFile))
                {
                    var savedPath = File.ReadAllText(configFile);
                    if (File.Exists(savedPath))
                    {
                        LoadPhotoFromFilePath(savedPath);
                        return;
                    }
                }

                LoadDefaultProfilePhoto();
            }
            catch (Exception ex)
            {
                LoadDefaultProfilePhoto();
                Console.WriteLine($"Ошибка загрузки фото: {ex.Message}");
            }
        }

        private void LoadPhotoFromFilePath(string path) // Загружает фото из указанного пути
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path);
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.EndInit();

            _userPhoto = bitmap;
            UpdateProfilePhotoDisplay();
        }

        private void LoadDefaultProfilePhoto() // Загружает фото профиля по умолчанию
        {
            try
            {
                const string defaultPhotoPath = "фото_умолч.jpg";
                if (File.Exists(defaultPhotoPath))
                {
                    LoadPhotoFromFilePath(defaultPhotoPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки фото по умолчанию: {ex.Message}");
            }
        }

        private void UpdateProfilePhotoDisplay() // Обновляет отображение фото профиля
        {
            if (UserPhotoImage != null && _userPhoto != null)
            {
                UserPhotoImage.Source = _userPhoto;
            }
        }

        private void InitializeTaskDisplay() // Инициализирует отображение задач
        {
            DisplayActiveTasks();
        }

        private void HandleNewTaskCreated(Todo.View.Main.Task newTask) // Обрабатывает создание новой задачи
        {
            var localTask = new Task
            {
                Id = newTask.Id,
                Title = newTask.Title,
                Time = newTask.Time,
                Date = newTask.Date,
                Description = newTask.Description,
                Category = newTask.Category,
                IsCompleted = newTask.IsCompleted
            };

            _activeTasks[localTask.Id] = localTask;
            DisplayActiveTasks(_currentCategory);
            _currentSelectedTaskId = localTask.Id;
            ShowTaskDetailsPanel(localTask);

            DisplaySuccessMessage($"Задача '{localTask.Title}' добавлена в список!");
        }

        private void DisplayActiveTasks(string category = "Все") // Отображает активные задачи
        {
            TasksPanel.Children.Clear();
            _currentCategory = category;
            _isHistoryMode = false;

            UpdateCategoryButtonStates(category);
            UpdateHeaderTitle("Задачи");

            var tasksToShow = FilterTasksByCategory(category, _activeTasks.Values);

            if (!tasksToShow.Any())
            {
                ShowEmptyTasksMessage("Нет задач");
                return;
            }

            foreach (var task in tasksToShow)
            {
                var taskBorder = CreateActiveTaskBorder(task);
                TasksPanel.Children.Add(taskBorder);
            }
        }

        private void DisplayCompletedTasks(string category = "Все") // Отображает завершенные задачи
        {
            TasksPanel.Children.Clear();
            _isHistoryMode = true;

            UpdateCategoryButtonStates(category);
            UpdateHeaderTitle("История задач");

            var completedTasksToShow = FilterTasksByCategory(category, _completedTasks.Values);

            if (!completedTasksToShow.Any())
            {
                ShowEmptyTasksMessage("Нет выполненных задач");
                return;
            }

            foreach (var task in completedTasksToShow)
            {
                var taskBorder = CreateCompletedTaskBorder(task);
                TasksPanel.Children.Add(taskBorder);
            }
        }

        private IEnumerable<Task> FilterTasksByCategory(string category, IEnumerable<Task> tasks) // Фильтрует задачи по категории
        {
            return category == "Все"
                ? tasks
                : tasks.Where(t => t.Category == category);
        }

        private void ShowEmptyTasksMessage(string message) // Показывает сообщение об отсутствии задач
        {
            var noTasksText = new TextBlock
            {
                Text = message,
                FontSize = 16,
                Foreground = new SolidColorBrush(Colors.Gray),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 50, 0, 0)
            };
            TasksPanel.Children.Add(noTasksText);
        }

        private void MoveTaskToCompleted(Task task) // Перемещает задачу в завершенные
        {
            _activeTasks.Remove(task.Id);
            _completedTasks[task.Id] = task;
            RefreshTaskDisplay();

            if (_currentSelectedTaskId == task.Id)
            {
                ClearTaskSelection();
            }
        }

        private void RefreshTaskDisplay() // Обновляет отображение задач
        {
            if (!_isHistoryMode)
            {
                DisplayActiveTasks(_currentCategory);
            }
            else
            {
                DisplayCompletedTasks(_currentCategory);
            }
        }

        private void ClearTaskSelection() // Очищает выбор задачи
        {
            _currentSelectedTaskId = null;
            TaskDetailsContent.Visibility = Visibility.Collapsed;
            SelectedTaskTitle.Text = "Выберите задачу";
        }

        private void HandleCheckBoxChecked(object sender, RoutedEventArgs e) // Обрабатывает отметку чекбокса задачи
        {
            if (!_isHistoryMode && sender is CheckBox checkBox)
            {
                var task = FindTaskByCheckBox(checkBox);
                if (task != null)
                {
                    MarkTaskAsCompleted(task);
                }
            }
        }

        private void MarkTaskAsCompleted(Task task) // Отмечает задачу как выполненную
        {
            task.IsCompleted = true;
            ApplyCompletedTaskStyle(task);
            MoveTaskToCompleted(task);

            DisplaySuccessMessage($"Задача '{task.Title}' выполнена и перемещена в историю!");
        }

        private void HandleCompleteTaskButtonClick(object sender, RoutedEventArgs e) // Обрабатывает кнопку завершения задачи
        {
            if (!_isHistoryMode && !string.IsNullOrEmpty(_currentSelectedTaskId) && _activeTasks.ContainsKey(_currentSelectedTaskId))
            {
                var task = _activeTasks[_currentSelectedTaskId];
                MarkTaskAsCompleted(task);
            }
        }

        private void UpdateHeaderTitle(string text) // Обновляет заголовок панели задач
        {
            SelectedTaskTitle.Text = text;
            TaskDetailsContent.Visibility = Visibility.Collapsed;
        }

        private void UpdateCategoryButtonStates(string activeCategory) // Обновляет состояние кнопок категорий
        {
            ShowAllCategoryButtons();
            ResetCategoryButtonStyles();

            var activeButton = GetCategoryButtonByName(activeCategory);
            if (activeButton != null)
            {
                ApplyActiveCategoryButtonStyle(activeButton);
            }
        }

        private Button GetCategoryButtonByName(string category) // Возвращает кнопку категории по имени
        {
            switch (category)
            {
                case "Дом":
                    return DomButton;
                case "Работа":
                    return RabotaButton;
                case "Учеба":
                    return UchebaButton;
                case "Отдых":
                    return OtdyhButton;
                default:
                    return null;
            }
        }

        private void ApplyActiveCategoryButtonStyle(Button button) // Применяет стиль активной кнопки категории
        {
            button.Background = new SolidColorBrush(Color.FromRgb(240, 248, 255));
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 149, 237));
        }

        private void ShowAllCategoryButtons() // Показывает все кнопки категорий
        {
            DomButton.Visibility = Visibility.Visible;
            RabotaButton.Visibility = Visibility.Visible;
            UchebaButton.Visibility = Visibility.Visible;
            OtdyhButton.Visibility = Visibility.Visible;
        }

        private void ResetCategoryButtonStyles() // Сбрасывает стили кнопок категорий
        {
            var buttons = new[] { DomButton, RabotaButton, UchebaButton, OtdyhButton };
            foreach (var button in buttons)
            {
                button.Background = Brushes.White;
                button.BorderBrush = Brushes.White;
            }
        }

        private void HandleHomeCategoryClick(object sender, RoutedEventArgs e) // Обрабатывает выбор категории "Дом"
        {
            SwitchCategory("Дом");
        }

        private void HandleWorkCategoryClick(object sender, RoutedEventArgs e) // Обрабатывает выбор категории "Работа"
        {
            SwitchCategory("Работа");
        }

        private void HandleStudyCategoryClick(object sender, RoutedEventArgs e) // Обрабатывает выбор категории "Учеба"
        {
            SwitchCategory("Учеба");
        }

        private void HandleLeisureCategoryClick(object sender, RoutedEventArgs e) // Обрабатывает выбор категории "Отдых"
        {
            SwitchCategory("Отдых");
        }

        private void SwitchCategory(string category) // Переключает отображение по категории
        {
            if (_isHistoryMode)
                DisplayCompletedTasks(category);
            else
                DisplayActiveTasks(category);
        }

        private void HandleActiveTasksClick(object sender, RoutedEventArgs e) // Обрабатывает показ активных задач
        {
            DisplayActiveTasks("Все");
            ClearTaskSelection();
        }

        private void HandleHistoryClick(object sender, RoutedEventArgs e) // Обрабатывает показ истории задач
        {
            DisplayCompletedTasks("Все");
            ClearTaskSelection();
        }

        private Border CreateActiveTaskBorder(Task task) // Создает границу для активной задачи
        {
            var border = CreateBaseTaskBorder(task.Id, Brushes.White, Color.FromRgb(224, 224, 224));
            var grid = CreateTaskGridLayout();

            var checkBox = CreateTaskCheckBox();
            var taskContent = CreateTaskContentPanel(task);

            Grid.SetColumn(checkBox, 0);
            Grid.SetColumn(taskContent, 1);

            grid.Children.Add(checkBox);
            grid.Children.Add(taskContent);
            border.Child = grid;

            border.MouseLeftButtonDown += (s, e) => HandleTaskSelection(s, e, task);

            task.TaskBorder = border;
            task.TaskCheckBox = checkBox;
            var titleTextBlock = taskContent.Children[0] as TextBlock;
            if (titleTextBlock != null)
            {
                task.TitleTextBlock = titleTextBlock;
            }

            return border;
        }

        private Border CreateCompletedTaskBorder(Task task) // Создает границу для завершенной задачи
        {
            var border = CreateBaseTaskBorder(task.Id,
                new SolidColorBrush(Color.FromRgb(245, 245, 245)),
                Color.FromRgb(200, 200, 200));

            var grid = CreateCompletedTaskGridLayout();

            var checkBox = CreateCheckedCheckBox();
            var taskContent = CreateCompletedTaskContentPanel(task);
            var statusLabel = CreateCompletedStatusLabel();

            Grid.SetColumn(checkBox, 0);
            Grid.SetColumn(taskContent, 1);
            Grid.SetColumn(statusLabel, 2);

            grid.Children.Add(checkBox);
            grid.Children.Add(taskContent);
            grid.Children.Add(statusLabel);
            border.Child = grid;

            border.MouseLeftButtonDown += (s, e) => HandleTaskSelection(s, e, task);

            return border;
        }

        private Border CreateBaseTaskBorder(string taskId, Brush background, Color borderColor) // Создает базовую границу задачи
        {
            return new Border
            {
                Background = background,
                BorderBrush = new SolidColorBrush(borderColor),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(15),
                Width = 270,
                Tag = taskId
            };
        }

        private Grid CreateTaskGridLayout() // Создает разметку сетки для задачи
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            return grid;
        }

        private Grid CreateCompletedTaskGridLayout() // Создает разметку сетки для завершенной задачи
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            return grid;
        }

        private CheckBox CreateTaskCheckBox() // Создает чекбокс для задачи
        {
            return new CheckBox
            {
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                IsChecked = false,
                Template = CreateCustomCheckBoxTemplate()
            };
        }

        private CheckBox CreateCheckedCheckBox() // Создает отмеченный чекбокс
        {
            var checkBox = CreateTaskCheckBox();
            checkBox.IsChecked = true;
            return checkBox;
        }

        private StackPanel CreateTaskContentPanel(Task task) // Создает панель содержимого задачи
        {
            var stackPanel = new StackPanel();

            var titleTextBlock = new TextBlock
            {
                Text = task.Title,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(51, 51, 51))
            };

            var timeTextBlock = new TextBlock
            {
                Text = $"{task.Time} • {task.Date}",
                Foreground = new SolidColorBrush(Color.FromRgb(102, 102, 102)),
                FontSize = 12,
                Margin = new Thickness(0, 5, 0, 0)
            };

            stackPanel.Children.Add(titleTextBlock);
            stackPanel.Children.Add(timeTextBlock);

            return stackPanel;
        }

        private StackPanel CreateCompletedTaskContentPanel(Task task) // Создает панель содержимого завершенной задачи
        {
            var stackPanel = new StackPanel();

            var titleTextBlock = new TextBlock
            {
                Text = task.Title,
                FontWeight = FontWeights.SemiBold,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                TextDecorations = TextDecorations.Strikethrough
            };

            var detailsTextBlock = new TextBlock
            {
                Text = $"{task.Time} • {task.Date} • {task.Category}",
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                FontSize = 12,
                Margin = new Thickness(0, 5, 0, 0)
            };

            stackPanel.Children.Add(titleTextBlock);
            stackPanel.Children.Add(detailsTextBlock);

            return stackPanel;
        }

        private TextBlock CreateCompletedStatusLabel() // Создает метку статуса "Выполнено"
        {
            return new TextBlock
            {
                Text = "✓ Выполнено",
                Foreground = new SolidColorBrush(Color.FromRgb(118, 223, 147)),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0)
            };
        }

        private void HandleTaskSelection(object sender, MouseButtonEventArgs e, Task task) // Обрабатывает выбор задачи
        {
            UpdateTaskSelectionState(task);

            if (sender is Border selectedBorder)
            {
                HighlightSelectedTaskBorder(selectedBorder);
                _currentSelectedTaskId = task.Id;
                ShowTaskDetailsPanel(task);
            }
        }

        private void UpdateTaskSelectionState(Task selectedTask) // Обновляет состояние выбора задач
        {
            var tasksToUpdate = _isHistoryMode ? _completedTasks.Values : _activeTasks.Values;

            foreach (var task in tasksToUpdate)
            {
                if (task.TaskBorder != null)
                {
                    var backgroundColor = _isHistoryMode ?
                        new SolidColorBrush(Color.FromRgb(245, 245, 245)) :
                        Brushes.White;

                    var borderColor = _isHistoryMode ?
                        new SolidColorBrush(Color.FromRgb(200, 200, 200)) :
                        new SolidColorBrush(Color.FromRgb(224, 224, 224));

                    task.TaskBorder.Background = backgroundColor;
                    task.TaskBorder.BorderBrush = borderColor;
                }
            }
        }

        private void HighlightSelectedTaskBorder(Border border) // Подсвечивает выбранную границу задачи
        {
            if (_isHistoryMode)
            {
                border.Background = new SolidColorBrush(Color.FromRgb(230, 240, 255));
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 149, 237));
            }
            else
            {
                border.Background = new SolidColorBrush(Color.FromRgb(240, 248, 255));
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(100, 149, 237));
            }
        }

        private void ShowTaskDetailsPanel(Task task) // Показывает панель деталей задачи
        {
            TaskDetailsContent.Visibility = Visibility.Visible;

            SelectedTaskTitle.Text = task.Title;
            TaskTime.Text = task.Time;
            TaskDate.Text = task.Date;
            TaskDescription.Text = task.Description;

            CompleteButton.Visibility = _isHistoryMode ? Visibility.Collapsed : Visibility.Visible;
            DeleteButton.Visibility = _isHistoryMode ? Visibility.Collapsed : Visibility.Visible;
        }

        private Task FindTaskByCheckBox(CheckBox checkBox) // Находит задачу по чекбоксу
        {
            return _activeTasks.Values.Concat(_completedTasks.Values)
                .FirstOrDefault(t => t.TaskCheckBox == checkBox);
        }

        private void ApplyCompletedTaskStyle(Task task) // Применяет стиль завершенной задачи
        {
            if (task.TaskBorder != null)
            {
                task.TaskBorder.Background = new SolidColorBrush(Color.FromRgb(245, 245, 245));
                task.TaskBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(200, 200, 200));

                if (task.TitleTextBlock != null)
                {
                    task.TitleTextBlock.TextDecorations = TextDecorations.Strikethrough;
                    task.TitleTextBlock.Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150));
                }
            }
        }

        private void HandleDeleteTaskClick(object sender, RoutedEventArgs e) // Обрабатывает удаление задачи
        {
            if (!_isHistoryMode && !string.IsNullOrEmpty(_currentSelectedTaskId) && _activeTasks.ContainsKey(_currentSelectedTaskId))
            {
                var task = _activeTasks[_currentSelectedTaskId];

                if (ConfirmTaskDeletion(task.Title))
                {
                    _activeTasks.Remove(_currentSelectedTaskId);
                    DisplayActiveTasks(_currentCategory);
                    ClearTaskSelection();
                    DisplaySuccessMessage("Задача удалена");
                }
            }
        }

        private bool ConfirmTaskDeletion(string taskTitle) // Подтверждает удаление задачи
        {
            var result = MessageBox.Show($"Вы уверены, что хотите удалить задачу '{taskTitle}'?",
                                       "Подтверждение удаления",
                                       MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        private ControlTemplate CreateCustomCheckBoxTemplate() // Создает кастомный шаблон чекбокса
        {
            var template = new ControlTemplate(typeof(CheckBox));

            var gridFactory = new FrameworkElementFactory(typeof(Grid));
            var ellipseFactory = CreateCheckBoxEllipseFactory();
            var contentFactory = CreateCheckBoxContentFactory();

            gridFactory.AppendChild(ellipseFactory);
            gridFactory.AppendChild(contentFactory);
            template.VisualTree = gridFactory;

            return template;
        }

        private FrameworkElementFactory CreateCheckBoxEllipseFactory() // Создает фабрику для эллипса чекбокса
        {
            var factory = new FrameworkElementFactory(typeof(Ellipse));
            factory.SetValue(Ellipse.StrokeProperty, new SolidColorBrush(Color.FromRgb(46, 80, 252)));
            factory.SetValue(Ellipse.StrokeThicknessProperty, 2.0);
            factory.SetValue(Ellipse.FillProperty, Brushes.Transparent);
            factory.SetValue(Ellipse.WidthProperty, 20.0);
            factory.SetValue(Ellipse.HeightProperty, 20.0);
            factory.SetValue(Ellipse.NameProperty, "outerEllipse");
            return factory;
        }

        private FrameworkElementFactory CreateCheckBoxContentFactory() // Создает фабрику для контента чекбокса
        {
            var factory = new FrameworkElementFactory(typeof(ContentPresenter));
            factory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            factory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            return factory;
        }

        public void AddNewTask(Task newTask) // Добавляет новую задачу
        {
            _activeTasks[newTask.Id] = newTask;
            DisplayActiveTasks(_currentCategory);
            _currentSelectedTaskId = newTask.Id;
            ShowTaskDetailsPanel(newTask);
        }

        private void HandleAddTaskButtonClick(object sender, RoutedEventArgs e) // Обрабатывает кнопку добавления задачи
        {
            Creating_tasks taskCreationWindow = new Creating_tasks();

            taskCreationWindow.TaskCreated += HandleNewTaskCreated;

            taskCreationWindow.Closed += (s, args) =>
            {
                taskCreationWindow.TaskCreated -= HandleNewTaskCreated;
            };

            taskCreationWindow.Show();
        }

        private void DisplaySuccessMessage(string message) // Отображает сообщение об успехе
        {
            MessageBox.Show(message, "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void HandleTextChanged(object sender, TextChangedEventArgs e) // Обрабатывает изменение текста
        {
           
        }

        private void HandleButtonClick(object sender, RoutedEventArgs e) // Обрабатывает нажатие кнопки
        {
            
        }
    }
}