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
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Windows.Media.Animation;

namespace GroverEmployees
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        GroverEntities context = new GroverEntities();
        CollectionViewSource employeeViewSource;
        
        public MainWindow()
        {
            InitializeComponent();
            employeeViewSource = ((CollectionViewSource)(FindResource("employeeViewSource")));
            
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            // Load is an extension method on IQueryable,    
            // defined in the System.Data.Entity namespace.   
            // This method enumerates the results of the query,    
            // similar to ToList but without creating a list.   
            // When used with Linq to Entities, this method    
            // creates entity objects and adds them to the context.   
            context.Employees.Load();

            // After the data is loaded, call the DbSet<T>.Local property    
            // to use the DbSet<T> as a binding source.   
            employeeViewSource.Source = context.Employees.Local;
        }
        private void LastCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            employeeViewSource.View.MoveCurrentToLast();
        }

        private void PreviousCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            employeeViewSource.View.MoveCurrentToPrevious();
        }

        private void NextCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            employeeViewSource.View.MoveCurrentToNext();
        }

        private void FirstCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            employeeViewSource.View.MoveCurrentToFirst();
        }
        private void DeleteEmployeeCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            
            var cur = employeeViewSource.View.CurrentItem as Employee;
            var employee = (from c in context.Employees
                        where c.EmployeeID == cur.EmployeeID
                        select c).FirstOrDefault();

            if (employee != null)
            {
                context.Employees.Remove(employee);
            }
            context.SaveChanges();

            updateIndicator.Content = "Employee Deleted";

            DoubleAnimation fadeIn = new DoubleAnimation(0.0, .5, new Duration(TimeSpan.FromSeconds(.5)));
            DoubleAnimation fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(2)));
            updateIndicator.ApplyAnimationClock(Button.OpacityProperty, fadeIn.CreateClock());
            updateIndicator.ApplyAnimationClock(Button.OpacityProperty, fadeOut.CreateClock());

            updateIndicator.Visibility = Visibility.Visible;
            employeeViewSource.View.Refresh();
        }

        private void UpdateCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            if (newEmployeeGrid.IsVisible)
            {
                Employee newEmployee = new Employee
                {
                    FirstName = add_employeeFirstNameTextBox.Text,
                    LastName = add_employeeLastNameTextBox.Text,
                    Title = add_employeeTitleTextBox.Text
                };
            
                int len = context.Employees.Local.Count();
                int pos = len;
                for (int i = 0; i < len; ++i)
                {
                    if (String.CompareOrdinal( Convert.ToString(newEmployee.EmployeeID), Convert.ToString(context.Employees.Local[i].EmployeeID) ) < 0)
                    {
                        pos = i;
                        break;
                    }
                }
                context.Employees.Local.Insert(pos, newEmployee);
                employeeViewSource.View.Refresh();
                employeeViewSource.View.MoveCurrentTo(newEmployee);

                newEmployeeGrid.Visibility = Visibility.Collapsed;
                existingEmployeeGrid.Visibility = Visibility.Visible;
                context.SaveChanges();

                updateIndicator.Content = "New employee created";
                DoubleAnimation fadeIn = new DoubleAnimation(0.0, .5, new Duration(TimeSpan.FromSeconds(.5)));
                DoubleAnimation fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(2)));
                updateIndicator.ApplyAnimationClock(Button.OpacityProperty, fadeIn.CreateClock());
                updateIndicator.ApplyAnimationClock(Button.OpacityProperty, fadeOut.CreateClock());

                updateIndicator.Visibility = Visibility.Visible;

                employeeViewSource.View.Refresh();
            }
            else
            {
                try
                {
                    int employeeID = Convert.ToInt32(employeeIDTextBox.Text);
                    Employee employee = context.Employees.Where(emp => emp.EmployeeID == employeeID).SingleOrDefault();
                    employee.FirstName = employeeFirstNameTextBox.Text;
                    employee.LastName = employeeLastNameTextBox.Text;
                    employee.Title = employeeTitleTextBox.Text;
                    context.SaveChanges();
                    updateIndicator.Content = "Employee Updated";

                    DoubleAnimation fadeIn = new DoubleAnimation(0.0, .5, new Duration(TimeSpan.FromSeconds(.5)));
                    DoubleAnimation fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromSeconds(2)));
                    updateIndicator.ApplyAnimationClock(Button.OpacityProperty, fadeIn.CreateClock());
                    updateIndicator.ApplyAnimationClock(Button.OpacityProperty, fadeOut.CreateClock());

                    updateIndicator.Visibility = Visibility.Visible;
                }
                catch (DbEntityValidationException exception)
                {
                    foreach (var eve in exception.EntityValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                            eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                                ve.PropertyName, ve.ErrorMessage);
                        }
                    }
                    throw;
                }
            }
        }

        
        private void AddCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            existingEmployeeGrid.Visibility = Visibility.Collapsed;
            
            newEmployeeGrid.Visibility = Visibility.Visible;
        
            foreach (var child in newEmployeeGrid.Children)
            {
                var tb = child as TextBox;
                if (tb != null)
                {
                    tb.Text = "";
                }
            }
            updateIndicator.Visibility = Visibility.Collapsed;
        }

        private void CancelCommandHandler(object sender, ExecutedRoutedEventArgs e)
        {
            add_employeeFirstNameTextBox.Text = "";
            add_employeeLastNameTextBox.Text = "";
            add_employeeTitleTextBox.Text = "";

            existingEmployeeGrid.Visibility = Visibility.Visible;
            newEmployeeGrid.Visibility = Visibility.Collapsed;
        }

        

    }
}
