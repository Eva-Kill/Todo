using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Todo.View
{
    /// <summary>
    /// Логика взаимодействия для Creating_tasks.xaml
    /// </summary>
    public partial class Creating_tasks : Window
    {
        public event Action<Todo.View.Main.Task> TaskCreated; // Событие создания задачи

        public Creating_tasks() // Конструктор окна создания задач
        {
            InitializeComponent();
            SetDefaultFormValues();
            ConfigureEventHandlers();
        }

        private void SetDefaultFormValues() // Устанавливает значения формы по умолчанию
        {
            SetCurrentTime();
            SetCurrentDate();
        }

        private void ConfigureEventHandlers() // Настраивает обработчики событий
        {
            // Event handlers can be configured here if needed
        }

        private void SetCurrentTime() // Устанавливает текущее время в комбобоксы
        {
            var now = DateTime.Now;
            var currentHour = now.Hour;
            var currentMinute = now.Minute;

            SelectValueInComboBox(HoursComboBox, currentHour.ToString("00"));
            SelectValueInComboBox(MinutesComboBox, GetNearestFiveMinutes(currentMinute).ToString("00"));
        }

        private int GetNearestFiveMinutes(int minutes) // Округляет минуты до ближайших 5
        {
            var rounded = (int)(Math.Round(minutes / 5.0) * 5);
            return rounded == 60 ? 0 : rounded;
        }

        private void SetCurrentDate() // Устанавливает текущую дату в DatePicker
        {
            if (TaskDatePicker != null)
            {
                TaskDatePicker.SelectedDate = DateTime.Today;
            }
        }

        private void SelectValueInComboBox(ComboBox comboBox, string value) // Выбирает значение в комбобоксе
        {
            if (comboBox != null && comboBox.Items != null)
            {
                foreach (ComboBoxItem item in comboBox.Items)
                {
                    if (item.Content?.ToString() == value)
                    {
                        comboBox.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void HandleTimeSelectionChanged(object sender, SelectionChangedEventArgs e) // Обрабатывает изменение выбора времени
        {
            // Time selection changed logic
        }

        private void HandleCreateButtonClick(object sender, RoutedEventArgs e) // Обрабатывает нажатие кнопки создания
        {
            if (!IsFormValid())
            {
                return;
            }

            try
            {
                var newTask = GenerateTaskFromForm();
                NotifyTaskCreated(newTask);
                DisplaySuccessMessage(newTask);
                CloseCurrentWindow();
            }
            catch (Exception ex)
            {
                DisplayErrorMessage($"Ошибка при создании задачи: {ex.Message}");
            }
        }

        private bool IsFormValid() // Проверяет валидность формы
        {
            if (!AreFormComponentsInitialized())
            {
                return false;
            }

            if (!IsTitleValid())
            {
                return false;
            }

            if (!IsCategorySelected())
            {
                return false;
            }

            if (!IsDateSelected())
            {
                return false;
            }

            return true;
        }

        private bool AreFormComponentsInitialized() // Проверяет инициализацию компонентов формы
        {
            if (TitleTextBox == null || CategoryComboBox == null || TaskDatePicker == null)
            {
                DisplayErrorMessage("Ошибка инициализации формы");
                return false;
            }

            return true;
        }

        private bool IsTitleValid() // Проверяет валидность названия задачи
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                DisplayWarningMessage("Введите название задачи");
                TitleTextBox.Focus();
                return false;
            }

            return true;
        }

        private bool IsCategorySelected() // Проверяет выбор категории
        {
            if (CategoryComboBox.SelectedItem == null)
            {
                DisplayWarningMessage("Выберите категорию");
                CategoryComboBox.Focus();
                return false;
            }

            return true;
        }

        private bool IsDateSelected() // Проверяет выбор даты
        {
            if (!TaskDatePicker.SelectedDate.HasValue)
            {
                DisplayWarningMessage("Выберите дату");
                TaskDatePicker.Focus();
                return false;
            }

            return true;
        }

        private Todo.View.Main.Task GenerateTaskFromForm() // Генерирует задачу из данных формы
        {
            return new Todo.View.Main.Task
            {
                Id = Guid.NewGuid().ToString(),
                Title = TitleTextBox.Text.Trim(),
                Category = GetSelectedCategoryName(),
                Description = GetTaskDescriptionText(),
                Time = GetFormattedTime(),
                Date = GetFormattedDateString(),
                IsCompleted = false
            };
        }

        private string GetSelectedCategoryName() // Получает название выбранной категории
        {
            if (CategoryComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content?.ToString() ?? "Без категории";
            }

            return "Без категории";
        }

        private string GetTaskDescriptionText() // Получает текст описания задачи
        {
            return DescriptionTextBox?.Text?.Trim() ?? string.Empty;
        }

        private string GetFormattedTime() // Форматирует время в строку
        {
            var hour = GetSelectedComboBoxText(HoursComboBox) ?? "00";
            var minute = GetSelectedComboBoxText(MinutesComboBox) ?? "00";
            return $"{hour}:{minute}";
        }

        private string GetFormattedDateString() // Форматирует дату в строку
        {
            return TaskDatePicker.SelectedDate.Value.ToString("dd MMMM yyyy");
        }

        private string GetSelectedComboBoxText(ComboBox comboBox) // Получает текст выбранного элемента комбобокса
        {
            if (comboBox?.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content?.ToString();
            }

            return null;
        }

        private void NotifyTaskCreated(Todo.View.Main.Task task) // Уведомляет о создании задачи
        {
            TaskCreated?.Invoke(task);
        }

        private void DisplaySuccessMessage(Todo.View.Main.Task task) // Отображает сообщение об успешном создании
        {
            var message = $"Задача создана!\n" +
                         $"Название: {task.Title}\n" +
                         $"Категория: {task.Category}";

            MessageBox.Show(message, "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DisplayWarningMessage(string message) // Отображает предупреждающее сообщение
        {
            MessageBox.Show(message, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void DisplayErrorMessage(string message) // Отображает сообщение об ошибке
        {
            MessageBox.Show(message, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void HandleCancelButtonClick(object sender, RoutedEventArgs e) // Обрабатывает кнопку отмены
        {
            CloseCurrentWindow();
        }

        private void CloseCurrentWindow() // Закрывает текущее окно
        {
            this.Close();
        }

        public DateTime GetSelectedDateTime() // Получает выбранные дату и время
        {
            if (TaskDatePicker?.SelectedDate.HasValue == true)
            {
                var time = GetFormattedTime();
                var dateString = TaskDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
                return DateTime.Parse($"{dateString} {time}");
            }

            return DateTime.Now;
        }

        private void HandleTextChanged(object sender, TextChangedEventArgs e) // Обрабатывает изменение текста
        {
            // Text changed logic
        }
    }
}