using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DentalClinic.Dashboard;

public sealed class SampleDentalRepository : IDentalClinicRepository
{
    public Task<int> GetTotalChairsAsync(CancellationToken ct = default) => Task.FromResult(4);

    public Task<IReadOnlyList<DentalAppointment>> GetAppointmentsForDateAsync(DateOnly date, CancellationToken ct = default)
    {
        DateTime baseTime = date.ToDateTime(new TimeOnly(9, 0));

        IReadOnlyList<DentalAppointment> items =
        [
            new(Guid.NewGuid(), "Layla Hassan", "Dr. Kareem", "Scaling & Polishing", 1, baseTime, TimeSpan.FromMinutes(45), 90m, AppointmentStatus.Completed),
            new(Guid.NewGuid(), "Omar Farooq", "Dr. Kareem", "Root Canal (Tooth #36)", 1, baseTime.AddMinutes(60), TimeSpan.FromMinutes(90), 350m, AppointmentStatus.InChair),
            new(Guid.NewGuid(), "Sara Mahmood", "Dr. Nora", "Composite Filling", 2, baseTime.AddMinutes(30), TimeSpan.FromMinutes(45), 120m, AppointmentStatus.CheckedIn),
            new(Guid.NewGuid(), "Zaid Ali", "Dr. Nora", "Zirconia Crown Prep", 3, baseTime.AddMinutes(120), TimeSpan.FromMinutes(60), 420m, AppointmentStatus.Scheduled),
            new(Guid.NewGuid(), "Huda Tariq", "Dr. Kareem", "Routine Checkup", 4, baseTime.AddMinutes(180), TimeSpan.FromMinutes(30), 60m, AppointmentStatus.Cancelled)
        ];

        return Task.FromResult(items);
    }
}

public static class Program
{
    public static async Task Main()
    {
        var repo = new SampleDentalRepository();
        var dashboardService = new DentalDashboardService(repo);

        DentalDashboardSnapshot snapshot = await dashboardService.GetDashboardAsync();

        Console.WriteLine($"=== Dental Clinic Dashboard ({snapshot.Date:yyyy-MM-dd}) ===");
        Console.WriteLine($"Appointments Today : {snapshot.Kpis.TotalAppointmentsToday} (Completed: {snapshot.Kpis.CompletedToday}, In Chair: {snapshot.Kpis.ActiveInChair}, Waiting: {snapshot.Kpis.PendingCheckIn})");
        Console.WriteLine($"Chairs Available   : {snapshot.Kpis.AvailableChairs}/{snapshot.Kpis.TotalChairs} | Utilization: {snapshot.Kpis.AverageChairUtilizationPercent}%");
        Console.WriteLine($"Projected Revenue  : ${snapshot.Kpis.CollectedOrScheduledRevenueToday:N2}");
        Console.WriteLine();

        Console.WriteLine("--- Chair Status ---");
        foreach (DentalChairStatus chair in snapshot.Chairs)
        {
            string details = chair.State == ChairState.Occupied
                ? $"{chair.CurrentPatientName} with {chair.CurrentDentistName} (since {chair.OccupiedSince:HH:mm})"
                : "Ready for next patient";
            Console.WriteLine($"Chair {chair.ChairNumber}: [{chair.State}] - {details}");
        }

        Console.WriteLine();
        Console.WriteLine("--- Active & Upcoming Queue ---");
        foreach (DentalAppointment appt in snapshot.UpcomingAppointments)
        {
            Console.WriteLine($"{appt.StartTime:HH:mm} | Chair {appt.ChairNumber} | {appt.PatientName} - {appt.Procedure} [{appt.Status}]");
        }
    }
}