using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Todo;
using Todo.Repository;

namespace Todo.View
{
    /// <summary>
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        private const string UsernamePlaceholder = "Введите имя пользователя";
        private const string EmailPlaceholder = "exam@yandex.ru";
        private const string PasswordPlaceholder = "Введите пароль";
        private const string ConfirmPasswordPlaceholder = "Повторите пароль";

        private readonly UserRepository _userRepository;
        private readonly InputValidator _inputValidator;

        private readonly Brush _placeholderBrush = Brushes.Gray;
        private readonly Brush _activeBrush = Brushes.Black;

        public Registration()
        {
            InitializeComponent();

            _userRepository = new UserRepository();
            _inputValidator = new InputValidator();

            ConfigureEventHandlers();
            InitializePlaceholderText();
        }

        private void ConfigureEventHandlers() 
        {
            RegisterButton.Click += HandleRegisterButtonClick;
            BackButton.Click += HandleBackButtonClick;

            AttachPlaceholderEventHandlers(UsernameTextBox, UsernamePlaceholder);
            AttachPlaceholderEventHandlers(EmailTextBox, EmailPlaceholder);
            AttachPlaceholderEventHandlers(PasswordTextBox, PasswordPlaceholder);
            AttachPlaceholderEventHandlers(ConfirmPasswordTextBox, ConfirmPasswordPlaceholder);
        }

        private void AttachPlaceholderEventHandlers(TextBox textBox, string placeholderText) 
        {
            textBox.GotFocus += (sender, e) => RemovePlaceholderText(textBox, placeholderText);
            textBox.LostFocus += (sender, e) => RestorePlaceholderIfEmpty(textBox, placeholderText);
        }

        private void InitializePlaceholderText() 
        {
            SetTextBoxPlaceholder(UsernameTextBox, UsernamePlaceholder);
            SetTextBoxPlaceholder(EmailTextBox, EmailPlaceholder);
            SetTextBoxPlaceholder(PasswordTextBox, PasswordPlaceholder);
            SetTextBoxPlaceholder(ConfirmPasswordTextBox, ConfirmPasswordPlaceholder);
        }

        private void HandleRegisterButtonClick(object sender, RoutedEventArgs e) 
        {
            try
            {
                var registrationData = CollectRegistrationFormData();

                if (!ValidateRegistrationForm(registrationData))
                {
                    return;
                }

                ProcessUserRegistration(registrationData);
            }
            catch (Exception ex)
            {
                DisplayErrorMessage($"Ошибка регистрации: {ex.Message}");
            }
        }

        private RegistrationData CollectRegistrationFormData() // Собирает данные из формы регистрации
        {
            return new RegistrationData
            {
                Username = UsernameTextBox.Text,
                Email = EmailTextBox.Text,
                Password = PasswordTextBox.Text,
                ConfirmPassword = ConfirmPasswordTextBox.Text
            };
        }

        private bool ValidateRegistrationForm(RegistrationData data) 
        {
            if (AreRequiredFieldsEmpty(data))
            {
                DisplayWarningMessage("Пожалуйста, заполните все поля.");
                return false;
            }

            if (!_inputValidator.IsValidUsername(data.Username))
            {
                DisplayWarningMessage("Имя пользователя должно содержать не менее 3 символов.");
                return false;
            }

            if (!_inputValidator.IsValidEmail(data.Email))
            {
                DisplayWarningMessage("Пожалуйста, введите корректный email.");
                return false;
            }

            if (!_inputValidator.IsValidPassword(data.Password))
            {
                DisplayWarningMessage("Пароль должен содержать не менее 6 символов.");
                return false;
            }

            if (data.Password != data.ConfirmPassword)
            {
                DisplayWarningMessage("Пароли не совпадают.");
                return false;
            }

            return true;
        }

        private bool AreRequiredFieldsEmpty(RegistrationData data) 
        {
            return IsFieldPlaceholderOrEmpty(data.Username, UsernamePlaceholder) ||
                   IsFieldPlaceholderOrEmpty(data.Email, EmailPlaceholder) ||
                   IsFieldPlaceholderOrEmpty(data.Password, PasswordPlaceholder) ||
                   IsFieldPlaceholderOrEmpty(data.ConfirmPassword, ConfirmPasswordPlaceholder);
        }

        private void ProcessUserRegistration(RegistrationData data) 
        {
            bool registrationSuccessful = _userRepository.RegisterUser(data.Username, data.Password, data.Email);

            if (registrationSuccessful)
            {
                DisplayRegistrationSuccessMessage();
                NavigateToMainApplicationWindow();
            }
        }

        private void HandleBackButtonClick(object sender, RoutedEventArgs e) 
        {
            ReturnToLoginWindow();
        }

        private void NavigateToMainApplicationWindow() 
        {
            var mainWindow = new Todo.View.Main_empty();
            mainWindow.Show();

            var currentWindow = Window.GetWindow(this);
            if (currentWindow != null)
            {
                currentWindow.Close();
            }
        }

        private void ReturnToLoginWindow() 
        {
            var loginWindow = new Todo.MainWindow();
            loginWindow.Show();

            var currentWindow = Window.GetWindow(this);
            if (currentWindow != null)
            {
                currentWindow.Close();
            }
        }

        private void RemovePlaceholderText(TextBox textBox, string placeholderText)
        {
            if (textBox.Text == placeholderText)
            {
                textBox.Text = string.Empty;
                textBox.Foreground = _activeBrush;
            }
        }

        private void RestorePlaceholderIfEmpty(TextBox textBox, string placeholderText) 
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                SetTextBoxPlaceholder(textBox, placeholderText);
            }
        }

        private void SetTextBoxPlaceholder(TextBox textBox, string placeholderText) 
        {
            textBox.Text = placeholderText;
            textBox.Foreground = _placeholderBrush;
        }

        private bool IsFieldPlaceholderOrEmpty(string text, string placeholderText) 
        {
            return text == placeholderText || string.IsNullOrWhiteSpace(text);
        }

        private void DisplayWarningMessage(string message) 
        {
            MessageBox.Show(message, "Предупреждение",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void DisplayRegistrationSuccessMessage() 
        {
            MessageBox.Show("Регистрация прошла успешно!", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DisplayErrorMessage(string message) 
        {
            MessageBox.Show(message, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal class RegistrationData // Класс для хранения данных регистрации
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }

   
    public class InputValidator // Валидатор входных данных
    {
        public bool IsValidEmail(string email) 
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var mailAddress = new System.Net.Mail.MailAddress(email);
                return mailAddress.Address == email;
            }
            catch
            {
                return false;
            }
        }

        public bool IsValidPassword(string password) 
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
        }

        public bool IsValidUsername(string username)
        {
            return !string.IsNullOrWhiteSpace(username) && username.Length >= 3;
        }
    }
}