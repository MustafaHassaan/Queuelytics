using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Commands.Report
{
    public record PageReportDto(
     string Page,
     int TotalUsers,
     int TotalSessions,
     int TotalViews,
     double AvgPerformance,
     DateTime LastUpdatedAt);
}
