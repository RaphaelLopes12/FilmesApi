using AutoMapper;
using FilmesApi.Data.Dtos;
using FilmesApi.Data;
using FilmesApi.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace FilmesApi.Controllers;

[ApiController]
[Route("[controller]")]
public class SessaoController : ControllerBase
{
    private FilmeContext _context;
    private IMapper _mapper;

    public SessaoController(FilmeContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpPost]
    public IActionResult AdicionaSessao([FromBody] CreateSessaoDto sessaoDto)
    {
        Sessao sessao = _mapper.Map<Sessao>(sessaoDto);
        _context.Sessoes.Add(sessao);
        _context.SaveChanges();
        return CreatedAtAction(nameof(RecuperaSessaoId), new { filmeId = sessao.FilmeId, cinemaId = sessao.CinemaId }, sessao);
    }

    [HttpGet]
    public IEnumerable<ReadSessaoDto> RecuperarSessao()
    {
        return _mapper.Map<List<ReadSessaoDto>>(_context.Sessoes.ToList());
    }

    [HttpGet("{filmeId}/{cinemaId}")]
    public IActionResult RecuperaSessaoId(int filmeId, int cinemaId)
    {
        var sessao = _context.Sessoes.FirstOrDefault(sessao => sessao.FilmeId == filmeId && sessao.CinemaId == cinemaId);
        if (sessao == null) return NotFound();
        var sessaoDto = _mapper.Map<ReadSessaoDto>(sessao);
        return Ok(sessaoDto);
    }

}
