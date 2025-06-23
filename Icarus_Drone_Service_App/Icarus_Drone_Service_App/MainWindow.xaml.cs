using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Icarus_Drone_Service_App.Models;

namespace Icarus_Drone_Service_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml.
    /// Hosts the UI for adding, editing, processing and removing drone service jobs.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<Drone> FinishedList = new List<Drone>();
        private readonly Queue<Drone> RegularService = new Queue<Drone>();
        private readonly Queue<Drone> ExpressService = new Queue<Drone>();

        private bool _isEditing = false;
        private Drone _editingDrone;

        private bool _pendingRegularConfirmation = false;
        private bool _pendingExpressConfirmation = false;

        private int _nextServiceTag = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class,
        /// hooks up cost validation, and refreshes all views.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            txtCost.PreviewTextInput += ValidateServiceCost;
            UpdateNextTagDisplay();
            RefreshListViews();
        }

        /// <summary>
        /// 6.10: Validates that <paramref name="e"/> only contains digits and up to two decimal places.
        /// Prevents negative input.
        /// </summary>
        /// <param name="sender">The cost TextBox.</param>
        /// <param name="e">Text composition event arguments.</param>
        private void ValidateServiceCost(object sender, TextCompositionEventArgs e)
        {
            if (e.Text == "-" || e.Text == "+")
            {
                e.Handled = true;
                return;
            }
            var tb = (TextBox)sender;
            string proposed = tb.Text.Insert(tb.CaretIndex, e.Text);
            e.Handled = !Regex.IsMatch(proposed, @"^\d+(\.\d{0,2})?$");
        }

        /// <summary>
        /// 6.7: Returns the currently selected service priority.
        /// </summary>
        /// <returns>“Express” if the express radio button is checked; otherwise “Regular”.</returns>
        private string GetServicePriority()
            => rbExpress.IsChecked == true ? "Express" : "Regular";

        /// <summary>
        /// No‐op handler so the XAML Checked event can point here without surcharge.
        /// </summary>
        private void GetServicePriority(object sender, RoutedEventArgs e) { }

        /// <summary>
        /// Updates the <see cref="numServiceTag"/> control to show the next available tag.
        /// </summary>
        private void UpdateNextTagDisplay()
        {
            numServiceTag.Value = _nextServiceTag;
            numServiceTag.IsEnabled = false;
        }

        /// <summary>
        /// 6.8,6.9,6.2–6.4: Rebinds all three collection views:
        /// Regular, Express, and Finished.
        /// </summary>
        private void RefreshListViews()
        {
            lvRegularService.ItemsSource = RegularService.ToList();
            lvExpressService.ItemsSource = ExpressService.ToList();
            lvFinishedList.ItemsSource = FinishedList.ToList();
        }

        /// <summary>
        /// 6.5,6.6,6.17: Handles the Add New Item / Save Changes button click.
        /// Validates all fields, applies 15% surcharge for express on enqueue,
        /// and then clears the form and refreshes views.
        /// </summary>
        /// <param name="sender">The Add/Edit button.</param>
        /// <param name="e">Routed event arguments.</param>
        private void AddNewItem(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtClientName.Text))
            {
                statusBarText.Text = "Client Name required.";
                return;
            }
            if (string.IsNullOrWhiteSpace(txtDroneModel.Text))
            {
                statusBarText.Text = "Drone Model required.";
                return;
            }
            if (string.IsNullOrWhiteSpace(txtProblem.Text))
            {
                statusBarText.Text = "Service Problem required.";
                return;
            }
            if (!double.TryParse(txtCost.Text, out double cost))
            {
                statusBarText.Text = "Cost must be numeric.";
                return;
            }
            if (cost < 0)
            {
                statusBarText.Text = "Cost cannot be negative.";
                return;
            }

            var priority = GetServicePriority();

            if (!_isEditing)
            {
                var drone = new Drone
                {
                    ClientName = txtClientName.Text,
                    DroneModel = txtDroneModel.Text,
                    ServiceTag = _nextServiceTag,
                    ServiceProblem = txtProblem.Text,
                    ServicePriority = priority
                };
                // surcharge only when enqueueing Express
                drone.ServiceCost = priority == "Express"
                    ? Math.Round(cost * 1.15, 2)
                    : cost;

                if (priority == "Express")
                    ExpressService.Enqueue(drone);
                else
                    RegularService.Enqueue(drone);

                statusBarText.Text = $"Added {priority} Tag {drone.ServiceTag}";
                _nextServiceTag += 10;
            }
            else
            {
                var oldPriority = _editingDrone.ServicePriority;
                _editingDrone.ClientName = txtClientName.Text;
                _editingDrone.DroneModel = txtDroneModel.Text;
                _editingDrone.ServiceProblem = txtProblem.Text;
                _editingDrone.ServicePriority = priority;
                _editingDrone.ServiceCost = priority == "Express"
                    ? Math.Round(cost * 1.15, 2)
                    : cost;

                if (oldPriority != priority)
                {
                    if (oldPriority == "Express")
                        RemoveFromQueue(ExpressService, _editingDrone);
                    else
                        RemoveFromQueue(RegularService, _editingDrone);

                    if (priority == "Express")
                        ExpressService.Enqueue(_editingDrone);
                    else
                        RegularService.Enqueue(_editingDrone);
                }

                statusBarText.Text = $"Updated Tag {_editingDrone.ServiceTag}";
                _isEditing = false;
                btnAddNewItem.Content = "Add New Item";
            }

            UpdateNextTagDisplay();
            ClearForm();
            RefreshListViews();
        }

        /// <summary>
        /// 6.12: Resets the regular‐confirmation flag when selection changes.
        /// </summary>
        private void RegularSelectionChanged(object sender, SelectionChangedEventArgs e)
            => _pendingRegularConfirmation = false;

        /// <summary>
        /// 6.13: Resets the express‐confirmation flag when selection changes.
        /// </summary>
        private void ExpressSelectionChanged(object sender, SelectionChangedEventArgs e)
            => _pendingExpressConfirmation = false;

        /// <summary>
        /// Populates the form for editing the given <paramref name="d"/>.
        /// If it’s Express, shows the base cost (ServiceCost / 1.15).
        /// </summary>
        /// <param name="d">The <see cref="Drone"/> to load into the form.</param>
        private void LoadForEdit(Drone d)
        {
            _isEditing = true;
            _editingDrone = d;
            btnAddNewItem.Content = "Save Changes";

            numServiceTag.Value = d.ServiceTag;
            txtClientName.Text = d.ClientName;
            txtDroneModel.Text = d.DroneModel;
            txtProblem.Text = d.ServiceProblem;

            if (d.ServicePriority == "Express")
            {
                double baseCost = Math.Round(d.ServiceCost / 1.15, 2);
                txtCost.Text = baseCost.ToString("F2");
            }
            else
            {
                txtCost.Text = d.ServiceCost.ToString("F2");
            }

            rbExpress.IsChecked = d.ServicePriority == "Express";
            rbRegular.IsChecked = d.ServicePriority == "Regular";
        }

        /// <summary>
        /// 6.12: Handles double-click on a regular queue row to load for edit.
        /// </summary>
        private void RegularService_Click(object sender, MouseButtonEventArgs e)
        {
            if (lvRegularService.SelectedItem is Drone d)
                LoadForEdit(d);
        }

        /// <summary>
        /// 6.13: Handles double-click on an express queue row to load for edit.
        /// </summary>
        private void ExpressService_Click(object sender, MouseButtonEventArgs e)
        {
            if (lvExpressService.SelectedItem is Drone d)
                LoadForEdit(d);
        }

        /// <summary>
        /// 6.14: Two-step confirmation then moves the selected Regular drone to finished.
        /// </summary>
        private void ProcessRegular(object sender, RoutedEventArgs e)
        {
            if (!(lvRegularService.SelectedItem is Drone d))
            {
                statusBarText.Text = "Select a Regular job.";
                return;
            }
            if (!_pendingRegularConfirmation)
            {
                statusBarText.Text = $"Click again to confirm Tag {d.ServiceTag}";
                _pendingRegularConfirmation = true;
                return;
            }

            RemoveFromQueue(RegularService, d);
            FinishedList.Add(d);
            statusBarText.Text = $"Processed Regular Tag {d.ServiceTag}";
            _pendingRegularConfirmation = false;

            RefreshListViews();
            UpdateNextTagDisplay();
        }

        /// <summary>
        /// 6.15: Two-step confirmation then moves the selected Express drone to finished.
        /// </summary>
        private void ProcessExpress(object sender, RoutedEventArgs e)
        {
            if (!(lvExpressService.SelectedItem is Drone d))
            {
                statusBarText.Text = "Select an Express job.";
                return;
            }
            if (!_pendingExpressConfirmation)
            {
                statusBarText.Text = $"Click again to confirm Tag {d.ServiceTag}";
                _pendingExpressConfirmation = true;
                return;
            }

            RemoveFromQueue(ExpressService, d);
            FinishedList.Add(d);
            statusBarText.Text = $"Processed Express Tag {d.ServiceTag}";
            _pendingExpressConfirmation = false;

            RefreshListViews();
            UpdateNextTagDisplay();
        }

        /// <summary>
        /// 6.16: Handles double-click on a finished list row to confirm and delete that drone.
        /// </summary>
        private void FinishedList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(lvFinishedList.SelectedItem is Drone d)) return;

            var res = MessageBox.Show(
                $"Are you sure you want to remove Tag {d.ServiceTag}?",
                "Confirm Delete",
                    MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes)
            {
                FinishedList.Remove(d);
                statusBarText.Text = $"Removed finished Tag {d.ServiceTag}";
                RefreshListViews();
            }
        }

        /// <summary>
        /// 6.17: Clears all input textboxes and resets priority to Regular.
        /// </summary>
        private void ClearForm()
        {
            txtClientName.Clear();
            txtDroneModel.Clear();
            txtProblem.Clear();
            txtCost.Clear();
            rbRegular.IsChecked = true;
        }

        /// <summary>
        /// Helper to remove a single <paramref name="target"/> from a queue in-place.
        /// </summary>
        /// <param name="queue">The queue to remove from.</param>
        /// <param name="target">The <see cref="Drone"/> to remove.</param>
        private void RemoveFromQueue(Queue<Drone> queue, Drone target)
        {
            var temp = queue.Where(x => x != target).ToList();
            queue.Clear();
            temp.ForEach(x => queue.Enqueue(x));
        }

        private void txtCost_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
