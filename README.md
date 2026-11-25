# DENTAL_CLINIC_MS — Dental Clinic Main Dashboard

A C# / .NET dashboard service and console runner for **Dental Clinic Management System** (`DENTAL_CLINIC_MS`), designed to aggregate daily appointments, dental chair occupancy, waiting patients, daily revenue, and chair utilization in a UI-agnostic snapshot.

![Dental Clinic Management System Dashboard Preview](assets/dashboard-preview.png)

## Overview

The dashboard layer separates clinic scheduling and operational metrics from presentation code so you can run it as a console utility or wire it into a desktop or web UI (such as WPF, WinForms, .NET MAUI, or Blazor).

### Key Capabilities
* **Daily KPIs:** Tracks total appointments, completed visits, active in-chair patients, waiting/checked-in patients, cancellations/no-shows, daily revenue, and chair utilization percentage.
* **Dental Chair & Operatory Status:** Maps active `InChair` appointments onto individual dental chairs (`Chair 1` through `Chair N`) alongside available chairs.
* **Active & Upcoming Queue:** Orders scheduled, checked-in, and in-chair appointments chronologically for front-desk and clinical coordination.
* **Testable Clock:** Accepts `TimeProvider` so shift times and occupancy calculations can be tested deterministically.

## Project Structure

```text
Dental_Cinec_MS/
├── docs/
│   └── dashboard-preview.png   # Dashboard UI preview image
├── DentalDashboardService.cs   # Core models, IDentalClinicRepository, and DentalDashboardService
├── Program.cs                  # Sample repository and console dashboard runner
├── README.md                   # Project documentation
└── Dental_Cinec_MS.csproj      # .NET project file
