using System;
using System.Globalization;

namespace Icarus_Drone_Service_App.Models
{
    /// <summary>
    /// Represents a single drone service request, including client,
    /// model, tag, problem description, cost, and priority.
    /// </summary>
    public class Drone
    {
        private string? _clientName;
        private string? _droneModel;
        private int _serviceTag;
        private string? _serviceProblem;
        private double _serviceCost;
        private string? _servicePriority;

        /// <summary>
        /// Gets or sets the client’s name, formatted as Title Case.
        /// </summary>
        public string ClientName
        {
            get => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(_clientName ?? string.Empty);
            set => _clientName = value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Gets or sets the drone model identifier.
        /// </summary>
        public string DroneModel
        {
            get => _droneModel ?? string.Empty;
            set => _droneModel = value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Gets or sets the numeric service tag (100–900).
        /// </summary>
        public int ServiceTag
        {
            get => _serviceTag;
            set => _serviceTag = value;
        }

        /// <summary>
        /// Gets or sets the problem description, formatted in Sentence Case.
        /// </summary>
        public string ServiceProblem
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_serviceProblem))
                    return string.Empty;
                var s = _serviceProblem.Trim();
                return char.ToUpper(s[0]) + s.Substring(1).ToLower();
            }
            set => _serviceProblem = value?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// Gets or sets the service cost, automatically rounded to two decimals.
        /// </summary>
        public double ServiceCost
        {
            get => _serviceCost;
            set => _serviceCost = Math.Round(value, 2);
        }

        /// <summary>
        /// Gets or sets the priority: “Regular” or “Express”.
        /// </summary>
        public string ServicePriority
        {
            get => _servicePriority ?? string.Empty;
            set => _servicePriority = value;
        }

        /// <summary>
        /// Returns a single-line string summarizing all fields of this Drone.
        /// </summary>
        /// <returns>A formatted string containing Tag, Client, Model, Problem, Cost, and Priority.</returns>
        public string Display() =>
            $"Tag: {ServiceTag}, Client: {ClientName}, Model: {DroneModel}, " +
            $"Problem: {ServiceProblem}, Cost: ${ServiceCost:F2}, Priority: {ServicePriority}";
    }
}
