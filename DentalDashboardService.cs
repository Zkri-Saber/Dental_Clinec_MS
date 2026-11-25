using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DentalClinic.Dashboard;

public enum AppointmentStatus
{
    Scheduled,
    CheckedIn,
    InChair,
    Completed,
    Cancelled,
    NoShow
}

public enum ChairState
{
    Available,
    Occupied,
    Sanitizing,
    OutOfService
}

public sealed record DentalAppointment(
    Guid Id,
    string PatientName,
    string DentistName,
    string Procedure,
    int ChairNumber,
    DateTime StartTime,
    TimeSpan Duration,
    decimal EstimatedFee,
    AppointmentStatus Status
);

public sealed record DentalChairStatus(
    int ChairNumber,
    ChairState State,
    string? CurrentPatientName,
    string? CurrentDentistName,
    DateTime? OccupiedSince
);

public sealed record DashboardKpis(
    int TotalAppointmentsToday,
    int CompletedToday,
    int ActiveInChair,
    int PendingCheckIn,
    int NoShowsOrCancelled,
    int AvailableChairs,
    int TotalChairs,
    decimal CollectedOrScheduledRevenueToday,
    double AverageChairUtilizationPercent
);

public sealed record DentalDashboardSnapshot(
    DateOnly Date,
    DashboardKpis Kpis,
    IReadOnlyList<DentalChairStatus> Chairs,
    IReadOnlyList<DentalAppointment> UpcomingAppointments,
    IReadOnlyList<DentalAppointment> CompletedAppointments
);

public interface IDentalClinicRepository
{
    Task<IReadOnlyList<DentalAppointment>> GetAppointmentsForDateAsync(DateOnly date, CancellationToken ct = default);
    Task<int> GetTotalChairsAsync(CancellationToken ct = default);
}

public sealed class DentalDashboardService
{
    private readonly IDentalClinicRepository _repository;
    private readonly TimeProvider _timeProvider;

    public DentalDashboardService(IDentalClinicRepository repository, TimeProvider? timeProvider = null)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<DentalDashboardSnapshot> GetDashboardAsync(DateOnly? targetDate = null, CancellationToken ct = default)
    {
        DateTime now = _timeProvider.GetLocalNow().DateTime;
        DateOnly date = targetDate ?? DateOnly.FromDateTime(now);

        int totalChairs = await _repository.GetTotalChairsAsync(ct);
        IReadOnlyList<DentalAppointment> appointments = await _repository.GetAppointmentsForDateAsync(date, ct);

        List<DentalAppointment> orderedAppointments = appointments
            .OrderBy(a => a.StartTime)
            .ToList();

        List<DentalChairStatus> chairs = BuildChairStatuses(totalChairs, orderedAppointments, now);

        int completedCount = orderedAppointments.Count(a => a.Status == AppointmentStatus.Completed);
        int inChairCount = orderedAppointments.Count(a => a.Status == AppointmentStatus.InChair);
        int pendingCheckInCount = orderedAppointments.Count(a =>
            a.Status is AppointmentStatus.Scheduled or AppointmentStatus.CheckedIn);
        int missedCount = orderedAppointments.Count(a =>
            a.Status is AppointmentStatus.Cancelled or AppointmentStatus.NoShow);

        decimal revenueToday = orderedAppointments
            .Where(a => a.Status is not AppointmentStatus.Cancelled and not AppointmentStatus.NoShow)
            .Sum(a => a.EstimatedFee);

        int availableChairsCount = chairs.Count(c => c.State == ChairState.Available);

        // Utilization based on scheduled/completed active chair minutes vs. an 8-hour (480 min) clinic day per chair
        double activeMinutes = orderedAppointments
            .Where(a => a.Status is AppointmentStatus.Completed or AppointmentStatus.InChair or AppointmentStatus.CheckedIn)
            .Sum(a => a.Duration.TotalMinutes);

        double totalAvailableMinutes = Math.Max(1, totalChairs * 480.0);
        double utilizationPercent = Math.Clamp((activeMinutes / totalAvailableMinutes) * 100.0, 0.0, 100.0);

        DashboardKpis kpis = new(
            TotalAppointmentsToday: orderedAppointments.Count,
            CompletedToday: completedCount,
            ActiveInChair: inChairCount,
            PendingCheckIn: pendingCheckInCount,
            NoShowsOrCancelled: missedCount,
            AvailableChairs: availableChairsCount,
            TotalChairs: totalChairs,
            CollectedOrScheduledRevenueToday: revenueToday,
            AverageChairUtilizationPercent: Math.Round(utilizationPercent, 1)
        );

        List<DentalAppointment> upcoming = orderedAppointments
            .Where(a => a.Status is AppointmentStatus.Scheduled or AppointmentStatus.CheckedIn or AppointmentStatus.InChair)
            .ToList();

        List<DentalAppointment> completed = orderedAppointments
            .Where(a => a.Status == AppointmentStatus.Completed)
            .ToList();

        return new DentalDashboardSnapshot(
            Date: date,
            Kpis: kpis,
            Chairs: chairs,
            UpcomingAppointments: upcoming,
            CompletedAppointments: completed
        );
    }

    private static List<DentalChairStatus> BuildChairStatuses(
        int totalChairs,
        IReadOnlyList<DentalAppointment> appointments,
        DateTime now)
    {
        var result = new List<DentalChairStatus>(totalChairs);

        for (int chairNum = 1; chairNum <= totalChairs; chairNum++)
        {
            DentalAppointment? activeAppt = appointments.FirstOrDefault(a =>
                a.ChairNumber == chairNum && a.Status == AppointmentStatus.InChair);

            if (activeAppt is not null)
            {
                result.Add(new DentalChairStatus(
                    ChairNumber: chairNum,
                    State: ChairState.Occupied,
                    CurrentPatientName: activeAppt.PatientName,
                    CurrentDentistName: activeAppt.DentistName,
                    OccupiedSince: activeAppt.StartTime <= now ? activeAppt.StartTime : now
                ));
            }
            else
            {
                result.Add(new DentalChairStatus(
                    ChairNumber: chairNum,
                    State: ChairState.Available,
                    CurrentPatientName: null,
                    CurrentDentistName: null,
                    OccupiedSince: null
                ));
            }
        }

        return result;
    }
}