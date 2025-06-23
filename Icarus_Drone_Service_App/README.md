# Icarus Drone Service App

A robust WPF application built to manage drone maintenance jobs with separate queues for Regular and Express services, two-step confirmations, and in-memory history of finished work.

---

## 📌 Project Overview

**Icarus Drone Service App** enables technicians to log, edit, process and remove drone repair requests. It features:

- Automatic, sequential Service Tags (100–900)  
- Optional 15% surcharge for Express jobs  
- Two-step confirmation for processing  
- Status-bar feedback for all actions  
- Double-click to edit or delete items  

---

## 🛠️ Technologies Used

- **.NET 9.0** / WPF (Windows Presentation Foundation)  
- **C#**  
- **Visual Studio 2022**  
- **DocFX** for API documentation  
- **In-memory Queues & Lists** for data storage  
- **Custom Validation** (regular expressions, two-step flags)  

---

## 🧪 Testing

Manual functional tests were executed to cover all primary UI workflows:

| Test Case No | Test Case Name                 | Test Steps                                                                                                                                     | Test Data                                                                                                           | Expected Result                                                                                                      |
|--------------|--------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------|
| 1            | Add New Regular Service        | 1. Launch app. 2. Verify Service Tag = 100 (disabled). 3. Enter Alice / X1000 / “Battery replacement” / Cost=100. 4. Click Add New Item        | ClientName: Alice<br>DroneModel: X1000<br>Priority: Regular<br>Problem: Battery replacement<br>Cost: 100             | New entry in **Regular** queue: Tag 100, Alice, X1000, Battery replacement, Cost $100.00, Priority Regular             |
| 2            | Add New Express Service        | 1. Enter Bob / Z500. 2. Select Express. 3. Enter “Motor issue” / Cost=200. 4. Click Add New Item                                               | ClientName: Bob<br>DroneModel: Z500<br>Priority: Express<br>Problem: Motor issue<br>Cost: 200                         | New entry in **Express** queue: Tag 110, Bob, Z500, Motor issue, Cost $230.00, Priority Express                          |
| 3            | Edit Existing Service Item     | 1. Double-click “Alice / X1000” in Regular. 2. Verify fields (Tag=100, Cost=100.00, etc.).<br>3. Change to Alice Smith / “Battery and rotor replacement”.<br>4. Click Save Changes | Original Tag: 100<br>Edited ClientName: Alice Smith<br>Edited Problem: Battery and rotor replacement                | Regular queue updated: item still Tag 100 but with new name and problem                                                 |
| 4            | Process Regular Service        | 1. Select “Alice Smith / X1000” in Regular. 2. Click Process Regular (twice to confirm)                                                      | Select Tag 100                                                                                    | Item removed from Regular, added to Finished list with Tag 100                                                        |
| 5            | Process Express Service        | 1. Select “Bob / Z500” in Express. 2. Click Process Express (twice to confirm)                                                                | Select Tag 110                                                                                    | Item removed from Express, added to Finished list with Tag 110                                                        |
| 6            | Remove Finished Service        | 1. Double-click “Tag: 100” in Finished list. 2. Confirm deletion in MessageBox                                                                  | Finished contains Tag 100 and Tag 110                                                           | Tag 100 is removed; Finished list still contains Tag 110                                                               |
| 7            | Service Cost Validation        | 1. Enter Name=TEST, Model=test, Problem=Test, Cost= (blank). 2. Click Add New Item                                                              | ClientName: TEST<br>Model: test<br>Problem: Test<br>Cost: (blank)                                              | Status bar shows “Cost must be numeric.”; no entry added                                                              |
| 8            | Missing Client Name Validation | 1. Leave Client Name blank. 2. Enter Model=test, Problem=Test, Cost=100. 3. Click Add New Item                                                  | ClientName: (blank)<br>Model: test<br>Problem: Test<br>Cost: 100                                                  | Status bar shows “Client Name required.”; no entry added                                                               |
| 9            | Missing Drone Model Validation | 1. Enter ClientName=Test. 2. Leave Drone Model blank. 3. Enter Problem=Test, Cost=100. 4. Click Add New Item                                  | ClientName: Test<br>Model: (blank)<br>Problem: Test<br>Cost: 100                                                | Status bar shows “Drone Model required.”; no entry added                                                               |
| 10           | Missing Problem Validation     | 1. Enter ClientName=Test, Model=Test. 2. Leave Problem blank. 3. Enter Cost=100. 4. Click Add New Item                                          | ClientName: Test<br>Model: Test<br>Problem: (blank)<br>Cost: 100                                                 | Status bar shows “Service Problem required.”; no entry added                                                           |

---

## 📦 Key Features

- ✅ Add, edit, process and remove drone service jobs  
- ✅ Automatic Service Tag generation (100 → 900, wraps)  
- ✅ 15% surcharge for Express jobs on enqueue only  
- ✅ Two-step confirmation for processing Regular/Express  
- ✅ In-memory Finished history with delete-on-double-click  
- ✅ Comprehensive input validation and status-bar feedback  

---

## 🚀 Getting Started

### Prerequisites

- Visual Studio 2022 or later  
- .NET 9.0 SDK  

### Run Instructions

1. Clone the repo:  
   git clone https://github.com/Brumertz/Icarus_Drone_Service_App.git
