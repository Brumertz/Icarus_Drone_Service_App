using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Icarus_Drone_Service_App.Helpers;
using Icarus_Drone_Service_App.Models;

namespace Icarus_Drone_Service_App.ViewModels
{
    /// <summary>
    /// ViewModel that exposes in‐memory collections of <see cref="Drone"/> items.
    /// Provides commands to add, complete, and remove services via MVVM binding.
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private Drone _selectedActive;
        private Drone _selectedCompleted;

        /// <summary>
        /// Gets the collection of active (incomplete) service items.
        /// </summary>
        public ObservableCollection<Drone> ActiveServices { get; }

        /// <summary>
        /// Gets the collection of completed service items.
        /// </summary>
        public ObservableCollection<Drone> CompletedServices { get; }

        /// <summary>
        /// Gets or sets the currently selected active service.
        /// Changing this will re‐evaluate command enablement.
        /// </summary>
        public Drone SelectedActive
        {
            get => _selectedActive;
            set
            {
                _selectedActive = value;
                OnPropertyChanged();
                ((RelayCommand)CompleteServiceCommand).RaiseCanExecuteChanged();
                ((RelayCommand)RemoveServiceCommand).RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Gets or sets the currently selected completed service.
        /// </summary>
        public Drone SelectedCompleted
        {
            get => _selectedCompleted;
            set { _selectedCompleted = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Command for adding a new service. Bound in XAML to the “Add New Item” button.
        /// </summary>
        public ICommand AddServiceCommand { get; }

        /// <summary>
        /// Command for marking an active service as complete. Disabled if no active item is selected.
        /// Bound in XAML to the “Process Regular/Express” buttons.
        /// </summary>
        public ICommand CompleteServiceCommand { get; }

        /// <summary>
        /// Command for removing the selected active service without completing. Disabled if no active item.
        /// </summary>
        public ICommand RemoveServiceCommand { get; }

        /// <summary>
        /// Initializes a new instance of <see cref="MainViewModel"/>.
        /// Creates empty collections and wires up commands.
        /// </summary>
        public MainViewModel()
        {
            Trace.WriteLine("[MainViewModel] Initializing in‐memory collections and commands.");
            ActiveServices = new ObservableCollection<Drone>();
            CompletedServices = new ObservableCollection<Drone>();

            AddServiceCommand = new RelayCommand(_ => AddService());
            CompleteServiceCommand = new RelayCommand(_ => CompleteService(), _ => SelectedActive != null);
            RemoveServiceCommand = new RelayCommand(_ => RemoveService(), _ => SelectedActive != null);
        }

        /// <summary>
        /// Adds a new <see cref="Drone"/> with placeholder data to <see cref="ActiveServices"/>.
        /// Actual properties are set from the view’s code‐behind before calling this.
        /// </summary>
        private void AddService()
        {
            Trace.WriteLine("[MainViewModel] AddService invoked.");
            var newDrone = new Drone
            {
                ClientName = "New Client",  // To be replaced by view‐data
                DroneModel = string.Empty,
                ServiceTag = 100,
                ServiceProblem = string.Empty,
                ServiceCost = 0.0,
                ServicePriority = "Regular"
            };
            ActiveServices.Add(newDrone);
        }

        /// <summary>
        /// Moves <see cref="SelectedActive"/> from <see cref="ActiveServices"/> to <see cref="CompletedServices"/>.
        /// </summary>
        private void CompleteService()
        {
            if (SelectedActive == null) return;
            Trace.WriteLine($"[MainViewModel] Completing service Tag={SelectedActive.ServiceTag}.");
            ActiveServices.Remove(SelectedActive);
            CompletedServices.Add(SelectedActive);
        }

        /// <summary>
        /// Removes <see cref="SelectedActive"/> from <see cref="ActiveServices"/> without adding to completed.
        /// </summary>
        private void RemoveService()
        {
            if (SelectedActive == null) return;
            Trace.WriteLine($"[MainViewModel] Removing service Tag={SelectedActive.ServiceTag}.");
            ActiveServices.Remove(SelectedActive);
        }

        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event for the specified property name.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
