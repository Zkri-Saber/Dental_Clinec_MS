using System.Globalization;
using System.Text;
using DentalClinic.Dashboard;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<DentalDashboardService>();

var app = builder.Build();

app.MapGet("/", (DentalDashboardService service) =>
{
    var vm = service.GetSampleDashboard();
    string html = RenderHtml(vm);
    return Results.Content(html, "text/html; charset=utf-8");
});

app.Run();

static string RenderHtml(DashboardViewModel vm)
{
    var k = vm.Kpis;
    var sb = new StringBuilder();

    // Build donut gradient from procedure percentages
    double acc = 0;
    var stops = new List<string>();
    foreach (var p in vm.Procedures)
    {
        double start = acc;
        double end = acc + p.Percent;
        stops.Add($"{p.ColorHex} {start.ToString(CultureInfo.InvariantCulture)}% {end.ToString(CultureInfo.InvariantCulture)}%");
        acc = end;
    }
    string donutGradient = $"conic-gradient({string.Join(", ", stops)})";

    sb.Append($$"""
    <!DOCTYPE html>
    <html lang="en">
    <head>
      <meta charset="UTF-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <title>Dental Clinic Management System</title>
      <style>
        * { box-sizing: border-box; margin: 0; padding: 0; font-family: 'Segoe UI', system-ui, sans-serif; }
        body { background: #090d16; color: #e2e8f0; padding: 16px; }
        .window { max-width: 1440px; margin: 0 auto; background: #0f172a; border: 1px solid #1e293b; border-radius: 12px; overflow: hidden; box-shadow: 0 20px 50px rgba(0,0,0,.6); }
        .titlebar { background: #0b1120; padding: 8px 16px; font-size: 12px; color: #94a3b8; border-bottom: 1px solid #1e293b; display: flex; justify-content: space-between; }
        .topbar { padding: 14px 20px; display: flex; flex-wrap: wrap; align-items: center; justify-content: space-between; gap: 12px; border-bottom: 1px solid #1e293b; background: #111827; }
        .brand h1 { font-size: 18px; letter-spacing: .5px; color: #f8fafc; }
        .brand p { font-size: 12px; color: #94a3b8; margin-top: 2px; }
        .controls { display: flex; align-items: center; gap: 10px; flex-wrap: wrap; }
        .search { background: #0b1120; border: 1px solid #263248; color: #e2e8f0; padding: 7px 12px; border-radius: 8px; width: 220px; font-size: 13px; }
        .pill { background: #062e26; color: #34d399; border: 1px solid #065f46; padding: 5px 10px; border-radius: 999px; font-size: 12px; font-weight: 600; }
        .clock { font-size: 13px; color: #cbd5e1; padding: 5px 10px; background: #1e293b; border-radius: 8px; }
        .user { font-size: 12px; text-align: right; }
        .user strong { display: block; color: #f8fafc; font-size: 13px; }
        .user span { color: #94a3b8; }

        .content { padding: 18px 20px; display: grid; gap: 16px; }
        .kpi-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 14px; }
        .card { background: #131c31; border: 1px solid #1f2d47; border-radius: 10px; padding: 14px 16px; }
        .kpi-title { font-size: 12px; color: #94a3b8; margin-bottom: 8px; }
        .kpi-value { font-size: 24px; font-weight: 700; color: #f8fafc; }
        .kpi-value span { font-size: 14px; font-weight: 500; color: #94a3b8; }
        .bar-wrap { display: flex; align-items: center; gap: 8px; margin: 10px 0 8px; }
        .bar { flex: 1; height: 6px; background: #1e293b; border-radius: 999px; overflow: hidden; }
        .bar-fill { height: 100%; background: #3b82f6; }
        .kpi-sub { font-size: 11px; color: #94a3b8; display: flex; justify-content: space-between; }
        .pos { color: #34d399; font-weight: 600; }

        .main-grid { display: grid; grid-template-columns: 2.2fr 1fr; gap: 16px; align-items: start; }
        .left-col, .right-col { display: grid; gap: 16px; }
        .section-title { font-size: 15px; font-weight: 600; color: #f1f5f9; margin-bottom: 12px; }

        .chairs-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 12px; }
        .chair { background: #0f172a; border: 1px solid #23324d; border-radius: 10px; padding: 12px; display: flex; flex-direction: column; justify-content: space-between; min-height: 155px; }
        .chair-top { display: flex; justify-content: space-between; align-items: center; margin-bottom: 10px; }
        .chair-name { font-weight: 700; font-size: 13px; color: #f8fafc; }
        .badge { font-size: 10px; font-weight: 700; padding: 3px 7px; border-radius: 6px; text-transform: uppercase; }
        .badge.occupied { background: rgba(16,185,129,.15); color: #34d399; border: 1px solid rgba(16,185,129,.35); }
        .badge.checked-in { background: rgba(59,130,246,.15); color: #60a5fa; border: 1px solid rgba(59,130,246,.35); }
        .badge.available { background: rgba(148,163,184,.14); color: #cbd5e1; border: 1px solid rgba(148,163,184,.25); }
        .meta { font-size: 12px; color: #94a3b8; line-height: 1.55; }
        .meta strong { color: #e2e8f0; font-weight: 500; }
        .prog { margin-top: 8px; }
        .prog-label { display: flex; justify-content: space-between; font-size: 11px; color: #94a3b8; margin-bottom: 4px; }
        .prog-track { height: 5px; background: #1e293b; border-radius: 999px; overflow: hidden; }
        .prog-fill { height: 100%; background: #10b981; }

        table { width: 100%; border-collapse: collapse; font-size: 12.5px; }
        th { text-align: left; color: #94a3b8; font-weight: 600; padding: 8px 10px; border-bottom: 1px solid #1f2d47; }
        td { padding: 9px 10px; border-bottom: 1px solid #182338; color: #e2e8f0; }
        .status-pill { padding: 3px 8px; border-radius: 999px; font-size: 11px; font-weight: 600; }
        .status-pill.in-chair { background: rgba(16,185,129,.15); color: #34d399; }
        .status-pill.checked-in { background: rgba(245,158,11,.15); color: #fbbf24; }
        .status-pill.scheduled { background: rgba(59,130,246,.15); color: #60a5fa; }

        .dentists-grid { display: grid; grid-template-columns: repeat(4, 1fr); gap: 8px; text-align: center; }
        .dentist { background: #0f172a; border: 1px solid #1f2d47; border-radius: 8px; padding: 10px 6px; }
        .avatar { width: 34px; height: 34px; border-radius: 50%; margin: 0 auto 6px; display: grid; place-items: center; font-weight: 700; font-size: 12px; background: #1e293b; color: #93c5fd; }
        .dentist .name { font-size: 12px; font-weight: 600; color: #f8fafc; }
        .dentist .st { font-size: 11px; color: #34d399; margin-top: 2px; }
        .dentist .st.off { color: #94a3b8; }
        .dentist .sub { font-size: 10px; color: #64748b; }

        .insights-body { display: flex; align-items: center; gap: 16px; }
        .donut { width: 104px; height: 104px; border-radius: 50%; background: {{donutGradient}}; position: relative; flex-shrink: 0; }
        .donut::after { content: ""; position: absolute; inset: 24px; background: #131c31; border-radius: 50%; }
        .legend { flex: 1; font-size: 12px; }
        .legend-title { font-weight: 600; color: #cbd5e1; margin-bottom: 6px; }
        .legend-row { display: flex; justify-content: space-between; align-items: center; margin: 4px 0; color: #cbd5e1; }
        .dot { width: 8px; height: 8px; border-radius: 50%; display: inline-block; margin-right: 6px; }

        .alerts { list-style: none; display: grid; gap: 8px; font-size: 12px; color: #cbd5e1; }
        .alerts li { background: #0f172a; border: 1px solid #1e293b; padding: 8px 10px; border-radius: 8px; }

        @media (max-width: 1100px) {
          .kpi-grid, .chairs-grid, .dentists-grid { grid-template-columns: repeat(2, 1fr); }
          .main-grid { grid-template-columns: 1fr; }
        }
      </style>
    </head>
    <body>
      <div class="window">
        <div class="titlebar">
          <span>Dental Clinic Management System</span>
          <span>ASP.NET Core • C# Dashboard</span>
        </div>
        <div class="topbar">
          <div class="brand">
            <h1>{{vm.ClinicTitle}}</h1>
            <p>{{vm.Subtitle}}</p>
          </div>
          <div class="controls">
            <input class="search" type="text" placeholder="Quick search..." />
            <span class="pill">{{vm.ShiftStatus}}</span>
            <span class="clock">{{vm.ClockText}}</span>
            <div class="user">
              <strong>{{vm.LoggedInUser.Split(' ')[0]}} {{vm.LoggedInUser.Split(' ')[1]}}</strong>
              <span>(Admin)</span>
            </div>
          </div>
        </div>

        <div class="content">
          <div class="kpi-grid">
            <div class="card">
              <div class="kpi-title">Appointments Today</div>
              <div class="kpi-value">{{k.AppointmentsCompleted}} <span>/ {{k.AppointmentsTotal}}</span></div>
              <div class="bar-wrap">
                <div class="bar"><div class="bar-fill" style="width:75%"></div></div>
                <span style="font-size:11px;color:#94a3b8;">75%</span>
              </div>
              <div class="kpi-sub"><span>{{k.CompletedCount}} Completed • {{k.ActiveCount}} Active • {{k.PendingCount}} Pending • {{k.NoShowCount}} No-Show</span></div>
            </div>

            <div class="card">
              <div class="kpi-title">Chair Occupancy</div>
              <div class="kpi-value">{{k.ActiveChairs}} <span>/ {{k.TotalChairs}} Active</span></div>
              <div style="margin:12px 0 8px;font-size:12px;color:#34d399;">Chairs 1–3 Active • Chair 4 Ready</div>
              <div class="kpi-sub"><span>{{k.ChairUtilizationPercent}}% Chair Utilization</span></div>
            </div>

            <div class="card">
              <div class="kpi-title">Patients Waiting</div>
              <div class="kpi-value">{{k.PatientsWaiting}} <span>Checked In</span></div>
              <div style="margin:12px 0 8px;font-size:12px;color:#60a5fa;">Front desk queue active</div>
              <div class="kpi-sub"><span>Avg. wait time: {{k.AvgWaitMinutes}} mins</span></div>
            </div>

            <div class="card">
              <div class="kpi-title">Daily Revenue</div>
              <div class="kpi-value">${{k.DailyRevenue.ToString("N2", CultureInfo.InvariantCulture)}}</div>
              <div style="margin:12px 0 8px;font-size:12px;color:#34d399;">+{{k.RevenueDeltaPercent}}% vs yesterday</div>
              <div class="kpi-sub"><span>Projected ${{k.ProjectedRevenue.ToString("N0", CultureInfo.InvariantCulture)}} • {{k.TargetPercent}}% of Target</span><span><strong class="pos">+{{k.RevenueDeltaPercent}}%</strong> vs yesterday</span></div>
            </div>
          </div>

          <div class="main-grid">
            <div class="left-col">
              <div class="card">
                <div class="section-title">Dental Chair Status &amp; Operatories</div>
                <div class="chairs-grid">
    """);

    foreach (var c in vm.Chairs)
    {
        sb.Append($$"""
                  <div class="chair">
                    <div>
                      <div class="chair-top">
                        <span class="chair-name">Chair {{c.ChairNumber}}</span>
                        <span class="badge {{c.BadgeType}}">{{c.BadgeText}}</span>
                      </div>
        """);

        if (c.PatientName is not null)
        {
            sb.Append($$"""
                      <div class="meta">
                        <div>Patient: <strong>{{c.PatientName}}</strong></div>
                        <div>Procedure: <strong>{{c.Procedure}}</strong></div>
                        <div>Dentist: <strong>{{c.DentistName}}</strong></div>
                        <div>{{c.TimeLabel}} <strong>{{c.TimeValue}}</strong></div>
                      </div>
            """);

            if (c.ProgressElapsedMin.HasValue && c.ProgressTotalMin.HasValue && c.ProgressTotalMin > 0)
            {
                int pct = (int)Math.Round((double)c.ProgressElapsedMin.Value / c.ProgressTotalMin.Value * 100);
                sb.Append($$"""
                      <div class="prog">
                        <div class="prog-label"><span>Progress:</span><span>{{c.ProgressElapsedMin}}/{{c.ProgressTotalMin}} min</span></div>
                        <div class="prog-track"><div class="prog-fill" style="width:{{pct}}%"></div></div>
                      </div>
                """);
            }
        }
        else
        {
            sb.Append($$"""
                      <div class="meta">
                        <div class="pos">● Ready for Patient</div>
                        <div style="margin-top:8px;">{{c.NextPatientInfo}}</div>
                      </div>
            """);
        }

        sb.Append("</div></div>");
    }

    sb.Append("""
                </div>
              </div>

              <div class="card">
                <div class="section-title">Active &amp; Upcoming Queue</div>
                <table>
                  <thead>
                    <tr>
                      <th>Time</th>
                      <th>Chair</th>
                      <th>Patient Name</th>
                      <th>Procedure</th>
                      <th>Dentist</th>
                      <th>Status</th>
                    </tr>
                  </thead>
                  <tbody>
    """);

    foreach (var row in vm.Queue)
    {
        sb.Append($$"""
                    <tr>
                      <td>{{row.Time}}</td>
                      <td>{{row.Chair}}</td>
                      <td>{{row.PatientName}}</td>
                      <td>{{row.Procedure}}</td>
                      <td>{{row.Dentist}}</td>
                      <td><span class="status-pill {{row.StatusCss}}">{{row.Status}}</span></td>
                    </tr>
        """);
    }

    sb.Append("""
                  </tbody>
                </table>
              </div>
            </div>

            <div class="right-col">
              <div class="card">
                <div class="section-title">Dentist Availability &amp; Schedule</div>
                <div class="dentists-grid">
    """);

    foreach (var d in vm.Dentists)
    {
        string initials = string.Concat(d.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(w => w[0]));
        string stClass = d.IsActive ? "st" : "st off";
        sb.Append($$"""
                  <div class="dentist">
                    <div class="avatar">{{initials}}</div>
                    <div class="name">{{d.Name}}</div>
                    <div class="{{stClass}}">{{d.StatusText}}</div>
                    <div class="sub">{{d.DetailText ?? ""}}</div>
                  </div>
        """);
    }

    sb.Append("""
                </div>
              </div>

              <div class="card">
                <div class="section-title">Quick Dental Insights</div>
                <div class="insights-body">
                  <div class="donut"></div>
                  <div class="legend">
                    <div class="legend-title">Procedures Today</div>
    """);

    foreach (var p in vm.Procedures)
    {
        sb.Append($$"""
                    <div class="legend-row">
                      <span><span class="dot" style="background:{{p.ColorHex}}"></span>{{p.Label}}</span>
                      <strong>{{p.Percent}}%</strong>
                    </div>
        """);
    }

    sb.Append("""
                  </div>
                </div>
              </div>

              <div class="card">
                <div class="section-title">Recent Activity / Alerts</div>
                <ul class="alerts">
    """);

    foreach (var alert in vm.Alerts)
    {
        sb.Append($"<li>{alert}</li>");
    }

    sb.Append("""
                </ul>
              </div>
            </div>
          </div>
        </div>
      </div>
    </body>
    </html>
    """);

    return sb.ToString();
}