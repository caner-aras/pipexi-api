using System.Net;
using MediatR;
using Pipexi.Application.Abstractions.Persistence;
using Pipexi.Application.Common;
using Pipexi.Application.Common.Models;
using Pipexi.Domain.Entities;
using Pipexi.Shared.Errors;
using Pipexi.Shared.Results;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TimeZoneConverter;

namespace Pipexi.Application.Features.Reports.Queries.GetShiftReportPdf;

public sealed record GetShiftReportPdfQuery(
    Guid OrganizationId, 
    DateTime FromDate, 
    DateTime ToDate, 
    Guid[]? MemberIds = null, 
    bool IncludeSummary = false)
    : IQuery<Result<byte[]>>;

public sealed class Handler : IRequestHandler<GetShiftReportPdfQuery, Result<byte[]>>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IShiftRepository _shiftRepository;
    private readonly IOrganizationMemberRepository _organizationMemberRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITimeEntryRepository _timeEntryRepository;
    private readonly ITimeEntryBreakRepository _timeEntryBreakRepository;
    private readonly IMemberPositionHistoryRepository _memberPositionHistoryRepository;
    private readonly IPositionRepository _positionRepository;

    public Handler(
        IOrganizationRepository organizationRepository,
        IShiftRepository shiftRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserRepository userRepository,
        ITimeEntryRepository timeEntryRepository,
        ITimeEntryBreakRepository timeEntryBreakRepository,
        IMemberPositionHistoryRepository memberPositionHistoryRepository,
        IPositionRepository positionRepository)
    {
        _organizationRepository = organizationRepository;
        _shiftRepository = shiftRepository;
        _organizationMemberRepository = organizationMemberRepository;
        _userRepository = userRepository;
        _timeEntryRepository = timeEntryRepository;
        _timeEntryBreakRepository = timeEntryBreakRepository;
        _memberPositionHistoryRepository = memberPositionHistoryRepository;
        _positionRepository = positionRepository;
    }

    public async Task<Result<byte[]>> Handle(GetShiftReportPdfQuery request, CancellationToken cancellationToken)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var organization = await _organizationRepository.GetByIdAsync(request.OrganizationId, cancellationToken);
        if (organization is null)
        {
            return Result<byte[]>.Failure(
                new AppError("general.not_found", $"Organization {request.OrganizationId} not found."),
                (int)HttpStatusCode.NotFound);
        }

        TimeZoneInfo tzi = TZConvert.GetTimeZoneInfo(organization.Timezone);

        var allShifts = await _shiftRepository.ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        
        var filteredShifts = allShifts
            .Where(s => s.StartAt.Date >= request.FromDate.Date && s.StartAt.Date <= request.ToDate.Date)
            .ToList();

        if (request.MemberIds != null && request.MemberIds.Length > 0)
        {
            filteredShifts = filteredShifts.Where(s => s.OrganizationMemberId.HasValue && request.MemberIds.Contains(s.OrganizationMemberId.Value)).ToList();
        }

        // Fetch time entries and breaks
        var shiftIds = filteredShifts.Select(s => s.Id).ToList();
        var timeEntries = shiftIds.Count > 0 
            ? await _timeEntryRepository.ListByShiftIdsAsync(shiftIds, cancellationToken)
            : new List<TimeEntry>();
            
        // Filter out shifts that don't have any time entries
        filteredShifts = filteredShifts
            .Where(s => timeEntries.Any(te => te.ShiftId == s.Id))
            .OrderBy(s => s.StartAt)
            .ToList();

        var timeEntryIds = timeEntries.Select(te => te.Id).ToList();
        var breaks = timeEntryIds.Count > 0
            ? await _timeEntryBreakRepository.ListByTimeEntryIdsAsync(timeEntryIds, cancellationToken)
            : new List<TimeEntryBreak>();

        // Load member names & wages
        var members = await _organizationMemberRepository.ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        var userIds = members.Select(m => m.UserId).Distinct().ToList();
        var users = new Dictionary<Guid, User>();
        foreach(var userId in userIds)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user != null) users[userId] = user;
        }

        var memberNames = members.ToDictionary(
            m => m.Id,
            m => users.TryGetValue(m.UserId, out var u) ? $"{u.FirstName} {u.LastName}".Trim() : "Unknown");

        var memberWages = new Dictionary<Guid, decimal>();
        foreach (var memberId in members.Select(m => m.Id))
        {
            var activeHistory = await _memberPositionHistoryRepository.GetActiveByOrganizationMemberIdAsync(memberId, cancellationToken);
            memberWages[memberId] = activeHistory?.HourlyRate ?? 0m;
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                page.Header().Element(header => ComposeHeader(header, organization.Name, request.FromDate, request.ToDate));
                page.Content().Element(content => ComposeContent(content, filteredShifts, memberNames, timeEntries, breaks, memberWages, organization.Currency, tzi));
                page.Footer().Element(ComposeFooter);
            });

            if (request.IncludeSummary && filteredShifts.Any())
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                    page.Header().Element(header => ComposeHeader(header, organization.Name, request.FromDate, request.ToDate, "Summary"));
                    page.Content().Element(content => ComposeSummary(content, filteredShifts, memberNames, timeEntries, breaks, memberWages, organization.Currency, tzi));
                    page.Footer().Element(ComposeFooter);
                });
            }
        });

        var pdfBytes = document.GeneratePdf();
        return Result<byte[]>.Success(pdfBytes);
    }

    private void ComposeHeader(IContainer container, string orgName, DateTime fromDate, DateTime toDate, string suffix = "Shift Report")
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text($"{orgName} {suffix}").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text($"Period: {fromDate:dd MMM yyyy} to {toDate:dd MMM yyyy}").FontSize(14).FontColor(Colors.Grey.Darken2);
            });
        });
    }

    private string GetCurrencySymbol(string currency)
    {
        return currency switch {
            "USD" => "$",
            "EUR" => "€",
            "GBP" => "£",
            "TRY" => "₺",
            _ => currency + " "
        };
    }

    private void ComposeContent(
        IContainer container, 
        List<Shift> shifts, 
        Dictionary<Guid, string> memberNames, 
        IReadOnlyCollection<TimeEntry> timeEntries, 
        IReadOnlyCollection<TimeEntryBreak> breaks, 
        Dictionary<Guid, decimal> memberWages, 
        string currency,
        TimeZoneInfo tzi)
    {
        if (shifts.Count == 0)
        {
            container.PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre).Text("No shifts found in this period.").FontSize(14).Italic();
            return;
        }

        var currencySymbol = GetCurrencySymbol(currency);

        container.PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre).Column(col =>
        {
            col.Spacing(20);

            foreach (var shift in shifts)
            {
                var employeeName = shift.OrganizationMemberId.HasValue && memberNames.TryGetValue(shift.OrganizationMemberId.Value, out var name) 
                    ? name 
                    : "Unassigned";

                var hourlyRate = shift.OrganizationMemberId.HasValue && memberWages.TryGetValue(shift.OrganizationMemberId.Value, out var wage)
                    ? wage
                    : 0m;

                var shiftEntries = timeEntries.Where(te => te.ShiftId == shift.Id).OrderBy(te => te.ClockInAt).ToList();
                var scheduledDuration = (shift.EndAt - shift.StartAt).TotalHours;

                col.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(3); // Action / Employee
                        columns.RelativeColumn(3); // Date
                        columns.RelativeColumn(2); // In / Actual Date
                        columns.RelativeColumn(2); // Out / Actual Time
                    });

                    // Header Row (Scheduled Shift)
                    table.Header(header =>
                    {
                        header.Cell().Element(HeaderStyle).Text("Employee");
                        header.Cell().Element(HeaderStyle).Text("Date");
                        header.Cell().Element(HeaderStyle).AlignRight().Text("Scheduled Time");
                        header.Cell().Element(HeaderStyle).AlignRight().Text("Scheduled Hrs");
                        
                        static IContainer HeaderStyle(IContainer container)
                        {
                            return container.Background(Colors.Grey.Lighten3).Padding(5).DefaultTextStyle(x => x.SemiBold());
                        }
                    });

                    var shiftStartTz = TimeZoneInfo.ConvertTimeFromUtc(shift.StartAt.UtcDateTime, tzi);
                    var shiftEndTz = TimeZoneInfo.ConvertTimeFromUtc(shift.EndAt.UtcDateTime, tzi);

                    // Shift Main Row
                    table.Cell().Element(ShiftMainStyle).Text(employeeName).SemiBold();
                    table.Cell().Element(ShiftMainStyle).Text(shiftStartTz.ToString("dd MMM yyyy"));
                    table.Cell().Element(ShiftMainStyle).AlignRight().Text($"{shiftStartTz:HH:mm} - {shiftEndTz:HH:mm}");
                    table.Cell().Element(ShiftMainStyle).AlignRight().Text(scheduledDuration.ToString("0.00"));

                    static IContainer ShiftMainStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Black).Padding(5);
                    }

                    // Entries Header
                    if (shiftEntries.Any())
                    {
                        table.Cell().Element(EntryStyle).Text("Action").SemiBold();
                        table.Cell().Element(EntryStyle).Text("");
                        table.Cell().Element(EntryStyle).AlignRight().Text("In").SemiBold();
                        table.Cell().Element(EntryStyle).AlignRight().Text("Out").SemiBold();
                    }

                    double totalActualDuration = 0;

                    foreach (var entry in shiftEntries)
                    {
                        var entryBreaks = breaks.Where(b => b.TimeEntryId == entry.Id).OrderBy(b => b.StartAt).ToList();
                        
                        var clockOutTime = entry.ClockOutAt ?? shift.EndAt;
                        var entryDuration = (clockOutTime - entry.ClockInAt).TotalHours;
                        
                        // Subtract unpaid breaks
                        var unpaidBreaksDuration = entryBreaks.Where(b => !b.IsPaid).Sum(b => (b.EndAt - b.StartAt).TotalHours);
                        var actualEntryDuration = Math.Max(0, entryDuration - unpaidBreaksDuration);
                        
                        totalActualDuration += actualEntryDuration;

                        var clockInTz = TimeZoneInfo.ConvertTimeFromUtc(entry.ClockInAt.UtcDateTime, tzi);
                        var clockOutTz = TimeZoneInfo.ConvertTimeFromUtc(clockOutTime.UtcDateTime, tzi);

                        // Entry row
                        table.Cell().Element(EntryStyle).Text("Entry");
                        table.Cell().Element(EntryStyle).Text("");
                        table.Cell().Element(EntryStyle).AlignRight().Text(clockInTz.ToString("HH:mm"));
                        table.Cell().Element(EntryStyle).AlignRight().Text(clockOutTz.ToString("HH:mm"));

                        // Breaks
                        foreach (var b in entryBreaks)
                        {
                            var breakStartTz = TimeZoneInfo.ConvertTimeFromUtc(b.StartAt.UtcDateTime, tzi);
                            var breakEndTz = TimeZoneInfo.ConvertTimeFromUtc(b.EndAt.UtcDateTime, tzi);

                            table.Cell().Element(EntryStyle).Text($"Break {(b.IsPaid ? "(Paid)" : "(Unpaid)")}");
                            table.Cell().Element(EntryStyle).Text("");
                            table.Cell().Element(EntryStyle).AlignRight().Text(breakStartTz.ToString("HH:mm"));
                            table.Cell().Element(EntryStyle).AlignRight().Text(breakEndTz.ToString("HH:mm"));
                        }
                    }

                    static IContainer EntryStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(5);
                    }

                    // Footer Calculations
                    var overtime = Math.Max(0, totalActualDuration - scheduledDuration);
                    var totalWage = (decimal)totalActualDuration * hourlyRate;

                    table.Cell().Element(FooterStyle).Text("Total").SemiBold();
                    table.Cell().Element(FooterStyle).Text("");
                    table.Cell().Element(FooterStyle).Text("");
                    table.Cell().Element(FooterStyle).AlignRight().Text(totalActualDuration.ToString("0.00")).SemiBold();

                    table.Cell().Element(FooterStyle).Text("Overtime").SemiBold();
                    table.Cell().Element(FooterStyle).Text("");
                    table.Cell().Element(FooterStyle).Text("");
                    table.Cell().Element(FooterStyle).AlignRight().Text(overtime.ToString("0.00"));

                    table.Cell().Element(FooterStyle).Text("Earnings").SemiBold();
                    table.Cell().Element(FooterStyle).Text("");
                    table.Cell().Element(FooterStyle).Text("");
                    table.Cell().Element(FooterStyle).AlignRight().Text($"{currencySymbol}{totalWage:N2}");

                    static IContainer FooterStyle(IContainer container)
                    {
                        return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten4).Padding(5);
                    }
                });
            }
        });
    }

    private void ComposeSummary(
        IContainer container, 
        List<Shift> shifts, 
        Dictionary<Guid, string> memberNames, 
        IReadOnlyCollection<TimeEntry> timeEntries, 
        IReadOnlyCollection<TimeEntryBreak> breaks, 
        Dictionary<Guid, decimal> memberWages, 
        string currency,
        TimeZoneInfo tzi)
    {
        var currencySymbol = GetCurrencySymbol(currency);
        
        // Group by day using the organization's timezone
        var groupedShifts = shifts.GroupBy(s => TimeZoneInfo.ConvertTimeFromUtc(s.StartAt.UtcDateTime, tzi).Date).OrderBy(g => g.Key).ToList();

        container.PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Date
                columns.RelativeColumn(2); // Sched Hrs
                columns.RelativeColumn(2); // Actual Hrs
                columns.RelativeColumn(2); // Earnings
            });

            table.Header(header =>
            {
                header.Cell().Element(HeaderStyle).Text("Date");
                header.Cell().Element(HeaderStyle).AlignRight().Text("Sched Hrs");
                header.Cell().Element(HeaderStyle).AlignRight().Text("Actual Hrs");
                header.Cell().Element(HeaderStyle).AlignRight().Text("Earnings");

                static IContainer HeaderStyle(IContainer container)
                {
                    return container.Background(Colors.Grey.Darken3).Padding(5).DefaultTextStyle(x => x.FontColor(Colors.White).SemiBold());
                }
            });

            double grandTotalSched = 0;
            double grandTotalActual = 0;
            decimal grandTotalEarnings = 0;

            foreach (var group in groupedShifts)
            {
                double dailySched = 0;
                double dailyActual = 0;
                decimal dailyEarnings = 0;

                foreach (var shift in group)
                {
                    var hourlyRate = shift.OrganizationMemberId.HasValue && memberWages.TryGetValue(shift.OrganizationMemberId.Value, out var wage) ? wage : 0m;
                    var scheduledDuration = (shift.EndAt - shift.StartAt).TotalHours;
                    
                    var shiftEntries = timeEntries.Where(te => te.ShiftId == shift.Id).ToList();
                    double actualDuration = 0;
                    
                    foreach (var entry in shiftEntries)
                    {
                        var entryBreaks = breaks.Where(b => b.TimeEntryId == entry.Id).ToList();
                        var clockOutTime = entry.ClockOutAt ?? shift.EndAt;
                        var entryDuration = (clockOutTime - entry.ClockInAt).TotalHours;
                        var unpaidBreaksDuration = entryBreaks.Where(b => !b.IsPaid).Sum(b => (b.EndAt - b.StartAt).TotalHours);
                        actualDuration += Math.Max(0, entryDuration - unpaidBreaksDuration);
                    }

                    dailySched += scheduledDuration;
                    dailyActual += actualDuration;
                    dailyEarnings += (decimal)actualDuration * hourlyRate;
                }

                grandTotalSched += dailySched;
                grandTotalActual += dailyActual;
                grandTotalEarnings += dailyEarnings;

                table.Cell().Element(CellStyle).Text(group.Key.ToString("dd MMM yyyy")).SemiBold();
                table.Cell().Element(CellStyle).AlignRight().Text(dailySched.ToString("0.00"));
                table.Cell().Element(CellStyle).AlignRight().Text(dailyActual.ToString("0.00"));
                table.Cell().Element(CellStyle).AlignRight().Text($"{currencySymbol}{dailyEarnings:N2}");
            }
            
            // Grand Total
            table.Cell().Element(GrandTotalStyle).Text("Grand Total").SemiBold();
            table.Cell().Element(GrandTotalStyle).AlignRight().Text(grandTotalSched.ToString("0.00")).SemiBold();
            table.Cell().Element(GrandTotalStyle).AlignRight().Text(grandTotalActual.ToString("0.00")).SemiBold();
            table.Cell().Element(GrandTotalStyle).AlignRight().Text($"{currencySymbol}{grandTotalEarnings:N2}").SemiBold();

            static IContainer CellStyle(IContainer container)
            {
                return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(8).PaddingHorizontal(5);
            }
            static IContainer GrandTotalStyle(IContainer container)
            {
                return container.Background(Colors.Grey.Lighten4).BorderTop(1).BorderColor(Colors.Black).PaddingVertical(10).PaddingHorizontal(5);
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(x =>
        {
            x.Span("Page ");
            x.CurrentPageNumber();
            x.Span(" of ");
            x.TotalPages();
        });
    }
}
