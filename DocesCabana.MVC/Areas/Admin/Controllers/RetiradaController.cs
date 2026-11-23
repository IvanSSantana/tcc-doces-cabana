using DocesCabana.Application.Contracts.Services;
using DocesCabana.Application.DTOs;
using DocesCabana.Domain.Constants;
using DocesCabana.MVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocesCabana.MVC.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = Papeis.Administrador)]
public class RetiradaController : Controller
{
    private const string PrefixoDaAgenda = nameof(AgendaDeRetiradaViewModel.Agenda);
    private const string PrefixoDoBloqueio = nameof(AgendaDeRetiradaViewModel.NovoBloqueio);

    private readonly IAgendaDeRetiradaService _agendaDeRetiradaService;

    public RetiradaController(IAgendaDeRetiradaService agendaDeRetiradaService)
    {
        _agendaDeRetiradaService = agendaDeRetiradaService;
    }

    [HttpGet]
    public async Task<IActionResult> Index() =>
        View(new AgendaDeRetiradaViewModel { Agenda = await _agendaDeRetiradaService.ObterParaGestao() });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SalvarHorarios([Bind(Prefix = PrefixoDaAgenda)] AgendaDeRetiradaDTO dto)
    {
        if (!ModelState.IsValid)
        {
            // Os dias bloqueados não vêm no formulário dos horários.
            dto.DiasBloqueados = (await _agendaDeRetiradaService.ObterParaGestao()).DiasBloqueados;
            return View(nameof(Index), new AgendaDeRetiradaViewModel { Agenda = dto });
        }

        await _agendaDeRetiradaService.SalvarHorarios(dto);

        TempData["Confirmacao"] = "Horários da retirada salvos.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BloquearDia([Bind(Prefix = PrefixoDoBloqueio)] DiaSemRetiradaDTO dto)
    {
        if (ModelState.IsValid && await _agendaDeRetiradaService.DiaJaBloqueado(dto.Data!.Value))
            ModelState.AddModelError($"{PrefixoDoBloqueio}.{nameof(DiaSemRetiradaDTO.Data)}", "Esta data já está bloqueada.");

        if (!ModelState.IsValid)
            return View(nameof(Index), new AgendaDeRetiradaViewModel
            {
                Agenda = await _agendaDeRetiradaService.ObterParaGestao(),
                NovoBloqueio = dto
            });

        await _agendaDeRetiradaService.BloquearDia(dto);

        TempData["Confirmacao"] = $"Retirada bloqueada em {dto.Data:dd/MM/yyyy}.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesbloquearDia(DateOnly data)
    {
        await _agendaDeRetiradaService.DesbloquearDia(data);

        TempData["Confirmacao"] = $"Retirada liberada em {data:dd/MM/yyyy}.";
        return RedirectToAction(nameof(Index));
    }
}
