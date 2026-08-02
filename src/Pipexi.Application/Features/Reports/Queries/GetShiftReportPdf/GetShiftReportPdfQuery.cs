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

namespace Pipexi.Application.Features.Reports.Queries.GetShiftReportPdf;

public sealed record GetShiftReportPdfQuery(Guid OrganizationId, DateTime FromDate, DateTime ToDate)
    : IQuery<Result<byte[]>>;

public sealed class Handler : IRequestHandler<GetShiftReportPdfQuery, Result<byte[]>>
{
    private readonly IOrganizationRepository _organizationRepository;
    private readonly IShiftRepository _shiftRepository;
    private readonly IOrganizationMemberRepository _organizationMemberRepository;
    private readonly IUserRepository _userRepository;

    public Handler(
        IOrganizationRepository organizationRepository,
        IShiftRepository shiftRepository,
        IOrganizationMemberRepository organizationMemberRepository,
        IUserRepository userRepository)
    {
        _organizationRepository = organizationRepository;
        _shiftRepository = shiftRepository;
        _organizationMemberRepository = organizationMemberRepository;
        _userRepository = userRepository;
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

        var allShifts = await _shiftRepository.ListByOrganizationIdAsync(request.OrganizationId, cancellationToken);
        
        var filteredShifts = allShifts
            .Where(s => s.StartAt.Date >= request.FromDate.Date && s.StartAt.Date <= request.ToDate.Date)
            .OrderBy(s => s.StartAt)
            .ToList();

        // Load member names
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

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, QuestPDF.Infrastructure.Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily(Fonts.Arial));

                page.Header().Element(header => ComposeHeader(header, organization.Name, request.FromDate, request.ToDate));
                page.Content().Element(content => ComposeContent(content, filteredShifts, memberNames));
                page.Footer().Element(ComposeFooter);
            });
        });

        var pdfBytes = document.GeneratePdf();
        return Result<byte[]>.Success(pdfBytes);
    }

    private void ComposeHeader(IContainer container, string orgName, DateTime fromDate, DateTime toDate)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text($"{orgName} Shift Report").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text($"Period: {fromDate:dd MMM yyyy} to {toDate:dd MMM yyyy}").FontSize(14).FontColor(Colors.Grey.Darken2);
            });
        });
    }

    private void ComposeContent(IContainer container, List<Shift> shifts, Dictionary<Guid, string> memberNames)
    {
        if (shifts.Count == 0)
        {
            container.PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre).Text("No shifts found in this period.").FontSize(14).Italic();
            return;
        }

        container.PaddingVertical(1, QuestPDF.Infrastructure.Unit.Centimetre).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.RelativeColumn(3); // Employee
                columns.RelativeColumn(2); // Date
                columns.RelativeColumn(2); // Time
                columns.RelativeColumn(2); // Duration
            });

            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("Employee");
                header.Cell().Element(CellStyle).Text("Date");
                header.Cell().Element(CellStyle).Text("Time");
                header.Cell().Element(CellStyle).AlignRight().Text("Duration (hrs)");

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            foreach (var shift in shifts)
            {
                var duration = shift.EndAt - shift.StartAt;
                var employeeName = shift.OrganizationMemberId.HasValue && memberNames.TryGetValue(shift.OrganizationMemberId.Value, out var name) 
                    ? name 
                    : "Unassigned";

                table.Cell().Element(CellStyle).Text(employeeName);
                table.Cell().Element(CellStyle).Text(shift.StartAt.ToString("dd MMM yyyy"));
                table.Cell().Element(CellStyle).Text($"{shift.StartAt:HH:mm} - {shift.EndAt:HH:mm}");
                table.Cell().Element(CellStyle).AlignRight().Text(duration.TotalHours.ToString("0.00"));

                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
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
