using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Todo.View;

namespace Todo
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Константы для текстовых заполнителей
        private const string DefaultEmailText = "Введите почту";
        private const string DefaultPasswordText = "Введите пароль";
        private const string EmailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        // Кисти для оформления текста
        private readonly Brush _defaultTextBrush = Brushes.Gray;
        private readonly Brush _activeTextBrush = Brushes.Black;


        public MainWindow()
        {
            InitializeComponent();
            AttachEventHandlers();
            InitializeTextFields();
        }


        // Привязывает обработчики событий к элементам управления

        private void AttachEventHandlers()
        {
            LoginButton.Click += HandleLogin;
            RegisterButton.Click += HandleRegister;

            EmailTextBox.GotFocus += HandleEmailFocus;
            EmailTextBox.LostFocus += HandleEmailLostFocus;
            PasswordTextBox.GotFocus += HandlePasswordFocus;
            PasswordTextBox.LostFocus += HandlePasswordLostFocus;
        }


        // Инициализирует текстовые поля заполнителями по умолчанию

        private void InitializeTextFields()
        {
            SetPlaceholderText(EmailTextBox, DefaultEmailText);
            SetPlaceholderText(PasswordTextBox, DefaultPasswordText);
        }


        // Обработчик события нажатия кнопки "Войти"

        private void HandleLogin(object sender, RoutedEventArgs e)
        {
            if (!ValidateLoginInput())
            {
                return;
            }

            ProcessSuccessfulLogin();
        }


        // Проверяет корректность введенных данных для входа

        private bool ValidateLoginInput()
        {
            var email = EmailTextBox.Text;
            var password = PasswordTextBox.Text;

            // Проверка на заполненность полей
            if (IsPlaceholderText(email, DefaultEmailText) ||
                IsPlaceholderText(password, DefaultPasswordText))
            {
                ShowMessage("Пожалуйста, заполните все поля.", "Ошибка");
                return false;
            }

            // Проверка длины пароля
            if (!IsPasswordValid(password))
            {
                ShowMessage("Пароль должен содержать не менее 6 символов!", "Ошибка");
                return false;
            }

            // Проверка формата email
            if (!IsEmailValid(email))
            {
                ShowMessage("Email имеет неверный формат.", "Ошибка");
                return false;
            }

            return true;
        }


        // Проверяет валидность пароля

        private bool IsPasswordValid(string password)
        {
            return password.Length >= 6;
        }

        // Проверяет валидность email с использованием регулярного выражения

        private bool IsEmailValid(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            return Regex.IsMatch(email, EmailPattern);
        }

        // Обрабатывает успешный вход в систему

        private void ProcessSuccessfulLogin()
        {
            ShowMessage("Вход выполнен успешно!", "Успех");
            NavigateToMainWindow();
        }

        // Обработчик события нажатия кнопки "Регистрация"

        private void HandleRegister(object sender, RoutedEventArgs e)
        {
            NavigateToRegistrationWindow();
        }

        // Переходит к главному окну приложения

        private void NavigateToMainWindow()
        {
            var mainWindow = new Todo.View.Main();
            mainWindow.Show();
            CloseCurrentWindow();
        }

        // Переходит к окну регистрации

        private void NavigateToRegistrationWindow()
        {
            var registrationWindow = new Todo.View.Registration();
            registrationWindow.Show();
            CloseCurrentWindow();
        }

        private void CloseCurrentWindow()
        {
            Close();
        }

        // Обработчик получения фокуса полем email

        private void HandleEmailFocus(object sender, RoutedEventArgs e)
        {
            ClearPlaceholderIfNeeded(EmailTextBox, DefaultEmailText);
        }

        // Обработчик потери фокуса полем email

        private void HandleEmailLostFocus(object sender, RoutedEventArgs e)
        {
            RestorePlaceholderIfEmpty(EmailTextBox, DefaultEmailText);
        }

        // Обработчик получения фокуса полем пароля

        private void HandlePasswordFocus(object sender, RoutedEventArgs e)
        {
            ClearPlaceholderIfNeeded(PasswordTextBox, DefaultPasswordText);
        }

        // Обработчик потери фокуса полем пароля

        private void HandlePasswordLostFocus(object sender, RoutedEventArgs e)
        {
            RestorePlaceholderIfEmpty(PasswordTextBox, DefaultPasswordText);
        }

        // Очищает текст заполнителя если он отображается

        private void ClearPlaceholderIfNeeded(TextBox textBox, string placeholderText)
        {
            if (textBox.Text == placeholderText)
            {
                textBox.Text = string.Empty;
                textBox.Foreground = _activeTextBrush;
            }
        }
        // Восстанавливает текст заполнителя если поле пустое

        private void RestorePlaceholderIfEmpty(TextBox textBox, string placeholderText)
        {
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                SetPlaceholderText(textBox, placeholderText);
            }
        }

        // Устанавливает текст заполнителя в текстовое поле

        private void SetPlaceholderText(TextBox textBox, string placeholderText)
        {
            textBox.Text = placeholderText;
            textBox.Foreground = _defaultTextBrush;
        }

        // Проверяет, является ли текст заполнителем


        private bool IsPlaceholderText(string text, string placeholderText)
        {
            return text == placeholderText;
        }

        // Отображает сообщение в диалоговом окне

        private void ShowMessage(string message, string title)
        {
            MessageBox.Show(message, title, MessageBoxButton.OK,
                title == "Ошибка" ? MessageBoxImage.Warning : MessageBoxImage.Information);
        }
    }
}