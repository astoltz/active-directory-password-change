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
using System.DirectoryServices.AccountManagement;
using System.Threading;

namespace PasswordChanger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UserName.Text = Environment.UserName;
        }

        private void DoItButton_Click(object sender, RoutedEventArgs e)
        {
            ActionLog.Text += "UserName is " + UserName.Text + "\n";
            ActionLog.Text += "Existing Password is " + ExistingPassword.Password + "\n";
            ActionLog.Text += "Number of changes is " + NumberOfChanges.Text + "\n";
            ActionLog.Text += "====================\n\n";

            Worker worker = new Worker(UserName.Text, ExistingPassword.Password, int.Parse(NumberOfChanges.Text), ActionLog);
            Thread thread = new Thread(worker.DoIt);
            thread.Start();
        }
    }

    class Worker
    {
        private string userName = "";
        private string fromPassword = "";
        private int numberOfChanges = 10;
        private TextBox log;
        
        public Worker(string userName, string fromPassword, int numberOfChanges, TextBox log)
        {
            this.userName = userName;
            this.fromPassword = fromPassword;
            this.numberOfChanges = numberOfChanges;
            this.log = log;
        }

        private void logText(string text)
        {
            log.Dispatcher.Invoke((Action)(() =>
            {
                log.Text += text + "\n";
            }));
        }

        public void DoIt()
        {
            string oldPassword = fromPassword;
            string newPassword = "";
            string passwordPrefix = "!PwRotate!" + DateTime.Now.ToString("MMddHH");
            byte passwordCounter = 0;

            try
            {
                for (int x = 0; x < numberOfChanges; x++)
                {
                    passwordCounter++;
                    newPassword = passwordPrefix + passwordCounter.ToString();
                    logText("Changing password from " + oldPassword + " to " + newPassword);
                    ChangePassword(oldPassword, newPassword);
                    oldPassword = newPassword;
                }

                passwordCounter++;
                newPassword = fromPassword;
                logText("Changing password from " + oldPassword + " to " + newPassword);
                ChangePassword(oldPassword, newPassword);
                oldPassword = newPassword;
            }
            catch(Exception ex)
            {
                logText("An exception was thrown while changing the password. Your password should currently be " + oldPassword);
                logText(ex.Message);
            }

            logText("Done!");
        }

        public void ChangePassword(string fromPassword, string toPassword)
        {
            using (PrincipalContext context = new PrincipalContext(ContextType.Domain))
            {
                using (UserPrincipal user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName))
                {
                    user.ChangePassword(fromPassword, toPassword);
                }
            }
        }
    }
}
