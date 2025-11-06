using Application.Commands.Report;
using Application.Interfaces;
using Application.Queries;
using Application.Security;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace API.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IRedisCacheService _cache;
        private readonly IUnitOfWork _uow;
        private readonly IMediator _IM;

        public ReportsController(IRedisCacheService cache, IUnitOfWork uow, IMediator mediator)
        {
            _cache = cache;
            _uow = uow;
            _IM = mediator;

        }
        [Authorize]
        [HttpGet("pages")]
        public async Task<IActionResult> GetPagesReport()
        {
            var result = await _IM.Send(new GetPagesReportQuery());
            return Ok(result);
        }

    }
}
