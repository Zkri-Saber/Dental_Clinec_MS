using System;
using System.Collections.Generic;

namespace DentalClinic.Dashboard;

public sealed record KpiSummary(
    int AppointmentsCompleted,
    int AppointmentsTotal,
    int CompletedCount,
    int ActiveCount,
    int PendingCount,
    int NoShowCount,
    int ActiveChairs,
    int TotalChairs,
    int ChairUtilizationPercent,
    int PatientsWaiting,
    int AvgWaitMinutes,
    decimal DailyRevenue,
    decimal ProjectedRevenue,
    int TargetPercent,
    int RevenueDeltaPercent
);

public sealed record ChairCard(
    int ChairNumber,
    string BadgeText,
    string BadgeType, // "occupied", "checked-in", "available"
    string? PatientName,
    string? Procedure,
    string? DentistName,
    string? TimeLabel,
    string? TimeValue,
    int? ProgressElapsedMin,
    int? ProgressTotalMin,
    string? NextPatientInfo,
    bool ShowToothIcon
);

public sealed record DentistStatus(
    string Name,
    string StatusText,
    string? DetailText,
    bool IsActive
);

public sealed record QueueRow(
    string Time,
    string Chair,
    string PatientName,
    string Procedure,
    string Dentist,
    string Status,
    string StatusCss
);

public sealed record ProcedureSlice(
    string Label,
    int Percent,
    string ColorHex
);

public sealed record DashboardViewModel(
    string ClinicTitle,
    string Subtitle,
    string ShiftStatus,
    string ClockText,
    string LoggedInUser,
    KpiSummary Kpis,
    IReadOnlyList<ChairCard> Chairs,
    IReadOnlyList<DentistStatus> Dentists,
    IReadOnlyList<QueueRow> Queue,
    IReadOnlyList<ProcedureSlice> Procedures,
    IReadOnlyList<string> Alerts
);

public sealed class DentalDashboardService
{
    public DashboardViewModel GetSampleDashboard()
    {
        return new DashboardViewModel(
            ClinicTitle: "DENTAL_CLINIC_MS",
            Subtitle: "Main Dashboard • Dr. Kareem's Practice • Today, November 25, 2026",
            ShiftStatus: "Open • Shift A",
            ClockText: "03:52 PM",
            LoggedInUser: "Dr. Kareem (Admin)",
            Kpis: new KpiSummary(
                AppointmentsCompleted: 18,
                AppointmentsTotal: 24,
                CompletedCount: 6,
                ActiveCount: 3,
                PendingCount: 12,
                NoShowCount: 3,
                ActiveChairs: 3,
                TotalChairs: 4,
                ChairUtilizationPercent: 75,
                PatientsWaiting: 4,
                AvgWaitMinutes: 14,
                DailyRevenue: 2840.00m,
                ProjectedRevenue: 3650.00m,
                TargetPercent: 78,
                RevenueDeltaPercent: 12
            ),
            Chairs:
            [
                new(1, "OCCUPIED", "occupied", "Layla Hassan", "Root Canal (Tooth #36)", "Dr. Kareem", "Since:", "03:15 PM", 35, 60, null, true),
                new(2, "OCCUPIED", "occupied", "Omar Farooq", "Composite Filling", "Dr. Nora", "Since:", "03:30 PM", 15, 45, null, true),
                new(3, "CHECKED IN", "checked-in", "Sara Mahmood", "Scaling & Polishing", "Dr. Kareem", "Scheduled:", "04:00 PM", null, null, null, false),
                new(4, "AVAILABLE", "available", null, null, null, null, null, null, null, "Next: Zaid Ali (04:15 PM - Crown Prep)", false)
            ],
            Dentists:
            [
                new("Dr. Kareem", "Active", "Chair 1", true),
                new("Dr. Nora", "Active", "Chair 2", true),
                new("Dr. Sarah", "On Break", null, false),
                new("Dr. Ahmed", "On Call", null, false)
            ],
            Queue:
            [
                new("03:15 PM", "Chair 1", "Layla Hassan", "Root Canal (Tooth #36)", "Dr. Kareem", "In Chair", "in-chair"),
                new("03:30 PM", "Chair 2", "Omar Farooq", "Composite Filling", "Dr. Nora", "In Chair", "in-chair"),
                new("03:45 PM", "Chair 3", "Sara Mahmood", "Scaling & Polishing", "Dr. Kareem", "Checked In", "checked-in"),
                new("04:15 PM", "Chair 4", "Zaid Ali", "Crown Prep", "Dr. Nora", "Scheduled", "scheduled"),
                new("04:45 PM", "Chair 1", "Huda Tariq", "Routine Checkup", "Dr. Kareem", "Scheduled", "scheduled")
            ],
            Procedures:
            [
                new("Root Canal", 25, "#3b82f6"),
                new("Cleaning", 35, "#06b6d4"),
                new("Fillings", 20, "#6366f1"),
                new("Crowns", 15, "#f59e0b"),
                new("Extractions", 5, "#ef4444")
            ],
            Alerts:
            [
                "Patient checked in: Sara Mahmood (Chair 3)",
                "Procedure completed: Layla Hassan (Chair 1)",
                "Reminder: Chair 4 sterilization check done"
            ]
        );
    }
}