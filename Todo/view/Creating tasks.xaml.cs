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
        public event Action<Todo.View.Main.Task> TaskCreated; 

        public Creating_tasks() 
        {
            InitializeComponent();
            SetDefaultFormValues();
            ConfigureEventHandlers();
        }

        private void SetDefaultFormValues() 
        {
            SetCurrentTime();
            SetCurrentDate();
        }

        private void ConfigureEventHandlers() // Настраивает обработчики событий
        {
            
        }

        private void SetCurrentTime() // Устанавливает текущее время в комбобоксы
        {
            var now = DateTime.Now;
            var currentHour = now.Hour;
            var currentMinute = now.Minute;

            SelectValueInComboBox(HoursComboBox, currentHour.ToString("00"));
            SelectValueInComboBox(MinutesComboBox, GetNearestFiveMinutes(currentMinute).ToString("00"));
        }

        private int GetNearestFiveMinutes(int minutes) 
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

        private void SelectValueInComboBox(ComboBox comboBox, string value) 
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

        private void HandleTimeSelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
           
        }

        private void HandleCreateButtonClick(object sender, RoutedEventArgs e) 
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

        private bool IsFormValid() 
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

        private bool AreFormComponentsInitialized() 
        {
            if (TitleTextBox == null || CategoryComboBox == null || TaskDatePicker == null)
            {
                DisplayErrorMessage("Ошибка инициализации формы");
                return false;
            }

            return true;
        }

        private bool IsTitleValid() 
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
            {
                DisplayWarningMessage("Введите название задачи");
                TitleTextBox.Focus();
                return false;
            }

            return true;
        }

        private bool IsCategorySelected() 
        {
            if (CategoryComboBox.SelectedItem == null)
            {
                DisplayWarningMessage("Выберите категорию");
                CategoryComboBox.Focus();
                return false;
            }

            return true;
        }

        private bool IsDateSelected() 
        {
            if (!TaskDatePicker.SelectedDate.HasValue)
            {
                DisplayWarningMessage("Выберите дату");
                TaskDatePicker.Focus();
                return false;
            }

            return true;
        }

        private Todo.View.Main.Task GenerateTaskFromForm() 
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

        private string GetSelectedCategoryName() 
        {
            if (CategoryComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Content?.ToString() ?? "Без категории";
            }

            return "Без категории";
        }

        private string GetTaskDescriptionText() 
        {
            return DescriptionTextBox?.Text?.Trim() ?? string.Empty;
        }

        private string GetFormattedTime() 
        {
            var hour = GetSelectedComboBoxText(HoursComboBox) ?? "00";
            var minute = GetSelectedComboBoxText(MinutesComboBox) ?? "00";
            return $"{hour}:{minute}";
        }

        private string GetFormattedDateString() 
        {
            return TaskDatePicker.SelectedDate.Value.ToString("dd MMMM yyyy");
        }

        private string GetSelectedComboBoxText(ComboBox comboBox) 
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

        private void DisplaySuccessMessage(Todo.View.Main.Task task) 
        {
            var message = $"Задача создана!\n" +
                         $"Название: {task.Title}\n" +
                         $"Категория: {task.Category}";

            MessageBox.Show(message, "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DisplayWarningMessage(string message) 
        {
            MessageBox.Show(message, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void DisplayErrorMessage(string message) 
        {
            MessageBox.Show(message, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void HandleCancelButtonClick(object sender, RoutedEventArgs e) 
        {
            CloseCurrentWindow();
        }

        private void CloseCurrentWindow() 
        {
            this.Close();
        }

        public DateTime GetSelectedDateTime() 
        {
            if (TaskDatePicker?.SelectedDate.HasValue == true)
            {
                var time = GetFormattedTime();
                var dateString = TaskDatePicker.SelectedDate.Value.ToString("yyyy-MM-dd");
                return DateTime.Parse($"{dateString} {time}");
            }

            return DateTime.Now;
        }

        private void HandleTextChanged(object sender, TextChangedEventArgs e) 
        {
            
        }
    }
}