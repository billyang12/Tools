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
using System.Windows.Shapes;

namespace HierachicalNotes
{
    /// <summary>
    /// Interaction logic for PasswordDialog.xaml
    /// </summary>
    public partial class PasswordDialog : Window
    {
        public PasswordDialog()
        {
            InitializeComponent();
            SetDialogType();
        }
        PasswordDialogType _dialogType = PasswordDialogType.NeedPassword;
        public PasswordDialogType DialogType
        {
            get
            {
                return _dialogType;
            }
            set
            {
                _dialogType = value;
                SetDialogType();
            }
        }
        void SetDialogType()
        {
            switch(_dialogType)
            {
                case PasswordDialogType.NeedPassword:
                    lbTitle.Content = "Please input your password";
                    lbOldPassword.Content = "Password:";
                    lbOldPassword.Visibility = Visibility.Visible;
                    pwdOld.Visibility = Visibility.Visible;

                    lbNewPassword.Visibility = Visibility.Hidden;
                    lbConfirmPassword.Visibility = Visibility.Hidden; 
                    pwdNew.Visibility = Visibility.Hidden;
                    pwdConfirm.Visibility = Visibility.Hidden;
                    break;
                case PasswordDialogType.SetNewPassword:
                    lbTitle.Content = "Please set your password";
                    //lbOldPassword.Content = "Password:";
                    lbOldPassword.Visibility = Visibility.Hidden;
                    pwdOld.Visibility = Visibility.Hidden;

                    lbNewPassword.Visibility = Visibility.Visible;
                    lbConfirmPassword.Visibility = Visibility.Visible;
                    pwdNew.Visibility = Visibility.Visible;
                    pwdConfirm.Visibility = Visibility.Visible;
                    break;
                case PasswordDialogType.UpdatePassword:
                    lbTitle.Content = "Please set your password";
                    lbOldPassword.Content = "Old Password:";
                    lbOldPassword.Visibility = Visibility.Visible;
                    pwdOld.Visibility = Visibility.Visible;

                    lbNewPassword.Visibility = Visibility.Visible;
                    lbConfirmPassword.Visibility = Visibility.Visible;
                    pwdNew.Visibility = Visibility.Visible;
                    pwdConfirm.Visibility = Visibility.Visible;
                    break;
            }
        }
        string CheckPasswordInput()
        {
            string result = "";
            switch(_dialogType)
            {
                case PasswordDialogType.NeedPassword:
                    if(Password.Length < 1)
                    {
                        result = "Password cannot be empty";
                    }
                    break;
                case PasswordDialogType.SetNewPassword:
                    if(NewPassword.Length<8 || ConfirmPassword.Length < 8)
                    {
                        result = "Password length should be > 8; ";
                    }
                    if(NewPassword != ConfirmPassword)
                    {
                        result += "New password and confirmed passowrd don't match;";
                    }
                    break;
                case PasswordDialogType.UpdatePassword:
                    if (Password.Length < 1)
                    {
                        result = "Old Password cannot be empty";
                    }
                    if (NewPassword.Length < 8 || ConfirmPassword.Length < 8)
                    {
                        result = "Password length should be > 8; ";
                    }
                    if (NewPassword != ConfirmPassword)
                    {
                        result += "New password and confirmed passowrd don't match;";
                    }
                    break;
            }
            return result;
        }
        public string Password
        {
            get
            {
                return pwdOld.Password;
            }
        }
        public string OldPassword { 
            get
            {
                return pwdOld.Password;
            } 
        }
        public string NewPassword
        {
            get
            {
                return pwdNew.Password;
            }
        }
        public string ConfirmPassword
        {
            get
            {
                return pwdConfirm.Password;
            }
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            var result = CheckPasswordInput();
            if (result != "")
            {
                MessageBox.Show(result);
            }
            else
            {
                DialogResult = true;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult= false;
        }
        public enum PasswordDialogType
        {
            NeedPassword,
            SetNewPassword,
            UpdatePassword
        }
    }
}
