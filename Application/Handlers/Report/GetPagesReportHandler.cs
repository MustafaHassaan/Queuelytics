using Application.Commands.Report;
using Application.Interfaces;
using Application.Queries;
using Application.Security;
using MediatR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Handlers.Report
{
    public class GetPagesReportHandler : IRequestHandler<GetPagesReportQuery, List<PageReportDto>>
    {
        private readonly IUnitOfWork _uow;
        private readonly IRedisCacheService _cache;

        public GetPagesReportHandler(IUnitOfWork uow, IRedisCacheService cache)
        {
            _uow = uow;
            _cache = cache;
        }

        public async Task<List<PageReportDto>> Handle(GetPagesReportQuery request, CancellationToken cancellationToken)
        {
            const string cacheKey = "report:pages";
            var cached = await _cache.GetAsync(cacheKey);
            if (cached is not null)
                return JsonConvert.DeserializeObject<List<PageReportDto>>(cached);

            var rawData = await _uow.RawDatas.GetAllAsync();
            var grouped = rawData
                .GroupBy(r => r.Pages)
                .Select(g => new PageReportDto(
                    Page: g.Key,
                    TotalUsers: g.Sum(x => x.Users),
                    TotalSessions: g.Sum(x => x.Sessions),
                    TotalViews: g.Sum(x => x.Views),
                    AvgPerformance: g.Average(x => x.PerformanceScore),
                    LastUpdatedAt: DateTime.UtcNow
                ))
                .ToList();

            await _cache.SetAsync(cacheKey, JsonConvert.SerializeObject(grouped), TimeSpan.FromMinutes(5));
            return grouped;
        }
    }
}
