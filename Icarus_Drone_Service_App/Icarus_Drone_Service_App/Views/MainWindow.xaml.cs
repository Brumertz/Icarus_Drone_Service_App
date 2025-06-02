using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Icarus_Drone_Service_App.Models;

namespace Icarus_Drone_Service_App.Views
{
    /// <summary>
    /// Code‐behind for MainWindow.xaml. Implements in‐memory queues and feedback pop‐ups:
    ///   • Add/Edit new service items
    ///   • Auto‐calculate cost based on priority
    ///   • Auto‐generate service tags
    ///   • Display and edit Regular/Express queues
    ///   • Process (dequeue) into FinishedList with confirmation
    ///   • Remove from FinishedList with confirmation
    ///   • Provide MessageBox feedback on each user action
    /// </summary>
    public partial class MainWindow : Window
    {
        // In‐memory collections for active and completed services
        private readonly List<Drone> FinishedList = new List<Drone>();
        private readonly Queue<Drone> RegularService = new Queue<Drone>();
        private readonly Queue<Drone> ExpressService = new Queue<Drone>();

        private const double BaseCost = 100.0;
        private bool _isEditing = false;
        private Drone _editingDrone;

        /// <summary>
        /// Initializes the MainWindow and sets initial cost text.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            CalculateCost();
        }

        /// <summary>
        /// Updates the service cost text box when priority changes.
        /// Regular = BaseCost; Express = BaseCost * 1.15.
        /// </summary>
        private void Priority_Checked(object sender, RoutedEventArgs e)
        {
            CalculateCost();
        }

        /// <summary>
        /// Calculates and displays cost based on selected priority.
        /// </summary>
        private void CalculateCost()
        {
            if (rbExpress == null || txtCost == null) return;

            if (rbExpress.IsChecked == true)
                txtCost.Text = (BaseCost * 1.15).ToString("F2");
            else
                txtCost.Text = BaseCost.ToString("F2");
        }
        /// <summary>
        /// Determines the next service tag by scanning all existing tags
        /// in RegularService, ExpressService, and FinishedList.
        /// Wraps from 900 back to 100.
        /// </summary>
        /// <returns>The next available service tag (multiple of 10).</returns>
        private int GenerateNextTag()
        {
            var allTags = RegularService.Select(d => d.ServiceTag)
                .Concat(ExpressService.Select(d => d.ServiceTag))
                .Concat(FinishedList.Select(d => d.ServiceTag));
            int maxTag = allTags.Any() ? allTags.Max() : 90;
            int next = maxTag + 10;
            return next <= 900 ? next : 100;
        }

        /// <summary>
        /// Handles the Add New Item / Save Changes button click.
        /// If not editing, creates a new Drone and enqueues based on priority.
        /// If editing, updates the existing Drone without changing its tag.
        /// Provides a MessageBox feedback after each action.
        /// </summary>
        private void btnAddNewItem_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditing)
            {
                // Create and enqueue new Drone
                var newDrone = new Drone
                {
                    ClientName = txtClientName.Text,
                    DroneModel = txtDroneModel.Text,
                    ServiceTag = GenerateNextTag(),
                    ServiceProblem = txtProblem.Text,
                    ServicePriority = rbExpress.IsChecked == true ? "Express" : "Regular"
                };
                newDrone.ServiceCost = newDrone.ServicePriority == "Express"
                    ? BaseCost * 1.15
                    : BaseCost;

                if (newDrone.ServicePriority == "Express")
                    ExpressService.Enqueue(newDrone);
                else
                    RegularService.Enqueue(newDrone);

                DisplayRegularQueue();
                DisplayExpressQueue();

                MessageBox.Show(
                    $"New service added: Tag Service\n{newDrone.ServiceTag}",
                    "Service Added",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                // Save edits on existing Drone
                _editingDrone.ClientName = txtClientName.Text;
                _editingDrone.DroneModel = txtDroneModel.Text;
                _editingDrone.ServiceProblem = txtProblem.Text;
                var newPriority = rbExpress.IsChecked == true ? "Express" : "Regular";

                // If priority changed, move between queues
                if (_editingDrone.ServicePriority != newPriority)
                {
                    if (_editingDrone.ServicePriority == "Express")
                        RemoveFromQueue(ExpressService, _editingDrone);
                    else
                        RemoveFromQueue(RegularService, _editingDrone);

                    _editingDrone.ServicePriority = newPriority;
                    _editingDrone.ServiceCost = newPriority == "Express"
                        ? BaseCost * 1.15
                        : BaseCost;

                    if (newPriority == "Express")
                        ExpressService.Enqueue(_editingDrone);
                    else
                        RegularService.Enqueue(_editingDrone);
                }
                else
                {
                    // Recalculate cost if priority unchanged
                    _editingDrone.ServiceCost = newPriority == "Express"
                        ? BaseCost * 1.15
                        : BaseCost;
                }

                DisplayRegularQueue();
                DisplayExpressQueue();

                MessageBox.Show(

                    $"Service updated: Tag Service\n{_editingDrone.ServiceTag}",
                    "Service Updated",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                   
                    

                // Exit edit mode
                _isEditing = false;
                _editingDrone = null;
                btnAddNewItem.Content = "Add New Item";
                numServiceTag.IsEnabled = false;
            }

            ClearTextBoxes();
            CalculateCost();
        }

        /// <summary>
        /// Removes a specific <see cref="Drone"/> from the given queue.
        /// Used when editing changes an item’s priority.
        /// </summary>
        /// <param name="queue">The queue to remove from.</param>
        /// <param name="target">The <see cref="Drone"/> instance to remove.</param>
        private void RemoveFromQueue(Queue<Drone> queue, Drone target)
        {
            var temp = new Queue<Drone>(queue.Where(d => d != target));
            queue.Clear();
            foreach (var d in temp)
                queue.Enqueue(d);
        }

        /// <summary>
        /// Handles double-click on an item in either Regular or Express ListView.
        /// Enters edit mode, populates input fields, and disables tag editing.
        /// </summary>
        private void lvService_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listView = sender == lvRegularService
                ? lvRegularService
                : lvExpressService;
            var queue = listView == lvRegularService
                ? RegularService
                : ExpressService;

            if (listView.SelectedItem is Drone selected)
            {
                _isEditing = true;
                _editingDrone = selected;
                btnAddNewItem.Content = "Save Changes";
                numServiceTag.IsEnabled = false;

                txtClientName.Text = selected.ClientName;
                txtDroneModel.Text = selected.DroneModel;
                numServiceTag.Value = selected.ServiceTag;
                txtProblem.Text = selected.ServiceProblem;
                if (selected.ServicePriority == "Express")
                    rbExpress.IsChecked = true;
                else
                    rbRegular.IsChecked = true;
                txtCost.Text = selected.ServiceCost.ToString("F2");
            }
        }

        /// <summary>
        /// Processes (dequeues) the next Drone in the RegularService queue and
        /// adds it to FinishedList. Provides a confirmation MessageBox.
        /// </summary>
        private void btnProcessRegular_Click(object sender, RoutedEventArgs e)
        {
            if (RegularService.Count == 0) return;
            var d = RegularService.Dequeue();
            FinishedList.Add(d);

            DisplayRegularQueue();
            DisplayFinishedList();

            MessageBox.Show(
                $"Regular service processed: Tag Service\n{d.ServiceTag}",
                "Regular Processed",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Processes (dequeues) the next Drone in the ExpressService queue and
        /// adds it to FinishedList. Provides a confirmation MessageBox.
        /// </summary>
        private void btnProcessExpress_Click(object sender, RoutedEventArgs e)
        {
            if (ExpressService.Count == 0) return;
            var d = ExpressService.Dequeue();
            FinishedList.Add(d);

            DisplayExpressQueue();
            DisplayFinishedList();

            MessageBox.Show(
               $"Express service processed: Tag Service\n{d.ServiceTag}",
               "Express Processed",
               MessageBoxButton.OK,
               MessageBoxImage.Information);
        }

        /// <summary>
        /// Removes the selected finished service on double-click.
        /// Provides confirmation with a MessageBox.
        /// </summary>
       private void lbFinishedList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lbFinishedList.SelectedItem is string disp)
            {
                var toRemove = FinishedList.Find(d => d.Display() == disp);
                if (toRemove != null)
                {
                    // Ask for confirmation before removing
                    var result = MessageBox.Show(
                        $"Are you sure you want to remove this finished service? Tag Service\n\n{toRemove.ServiceTag}",
                       "Confirm Removal",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        FinishedList.Remove(toRemove);
                        DisplayFinishedList();

                        MessageBox.Show(
                            $"Removed from finished list: Tag Service\n\n{toRemove.ServiceTag}",
                            "Finished Removed",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
        }


        /// <summary>
        /// Clears and refreshes the RegularService ListView.
        /// </summary>
        private void DisplayRegularQueue()
        {
            lvRegularService.Items.Clear();
            foreach (var d in RegularService)
                lvRegularService.Items.Add(d);
        }

        /// <summary>
        /// Clears and refreshes the ExpressService ListView.
        /// </summary>
        private void DisplayExpressQueue()
        {
            lvExpressService.Items.Clear();
            foreach (var d in ExpressService)
                lvExpressService.Items.Add(d);
        }

        /// <summary>
        /// Clears and refreshes the FinishedList ListBox.
        /// </summary>
        private void DisplayFinishedList()
        {
            lbFinishedList.Items.Clear();
            foreach (var d in FinishedList)
                lbFinishedList.Items.Add(d.Display());
        }

        /// <summary>
        /// Clears all input textboxes (ClientName, DroneModel, Problem).
        /// Leaves tag disabled and resets cost display.
        /// </summary>
        private void ClearTextBoxes()
        {
            txtClientName.Clear();
            txtDroneModel.Clear();
            txtProblem.Clear();
            // Tag is auto-generated and remains disabled
            CalculateCost();
        }
    }
}
