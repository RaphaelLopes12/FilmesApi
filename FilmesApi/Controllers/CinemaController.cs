using AutoMapper;
using FilmesApi.Data;
using FilmesApi.Data.Dtos;
using FilmesApi.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace FilmesApi.Controllers;

[ApiController]
[Route("[controller]")]
public class CinemaController : ControllerBase
{
    private FilmeContext _context;
    private IMapper _mapper;

    public CinemaController(FilmeContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Adiciona cinema no banco de dados.
    /// </summary>
    /// <param name="cinemaDto">Objeto com os campos necessários para a criação de um cinema.</param>
    /// <returns></returns>
    /// <response code="201">Caso inserção seja feita com sucesso.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult AdicionaCinema([FromBody] CreateCinemaDto cinemaDto)
    {
        Cinema cinema = _mapper.Map<Cinema>(cinemaDto);
        _context.Cinemas.Add(cinema);
        _context.SaveChanges();
        return CreatedAtAction(nameof(RecuperaCinemaId), new { id = cinema.Id }, cinema);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IEnumerable<ReadCinemaDto> RecuperarCinemas()
    {        
         return _mapper.Map<List<ReadCinemaDto>>(_context.Cinemas.ToList());        
    }

    /// <summary>
    /// Retorna um cinema com base no ID fornecido.
    /// </summary>
    /// <remarks>
    /// Esta operação permite recuperar um cinema específico do banco de dados usando o ID fornecido como parâmetro na URL.
    /// Se o cinema com o ID correspondente for encontrado, será retornado o cinema correspondente juntamente com os detalhes em formato DTO (Data Transfer Object).
    /// Se nenhum cinema for encontrado para o ID fornecido, será retornado um código de status 404 (Not Found).
    /// </remarks>
    /// <param name="id">O ID do cinema a ser recuperado.</param>
    /// <returns>Detalhes do cinema correspondente ao ID fornecido.</returns>
    /// <response code="200">Retorna os detalhes do cinema correspondente ao ID.</response>
    /// <response code="404">Se nenhum cinema for encontrado para o ID fornecido.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult RecuperaCinemaId(int id)
    {
        var cinema = _context.Cinemas.FirstOrDefault(cinemas => cinemas.Id == id);
        if (cinema == null) return NotFound();
        var cinemaDto = _mapper.Map<ReadCinemaDto>(cinema);
        return Ok(cinemaDto);
    }

    /// <summary>
    /// Atualiza as informações de um cinema existente com base no ID fornecido.
    /// </summary>
    /// <remarks>
    /// Esta operação permite atualizar as informações de um cinema existente no banco de dados, utilizando o ID fornecido na URL e os dados fornecidos no corpo da requisição.
    /// O corpo da requisição deve conter os dados no formato UpdatecinemaDto para atualização do cinema correspondente ao ID informado.
    /// Se o cinema com o ID correspondente não for encontrado, será retornado um código de status 404 (Not Found).
    /// Após a atualização bem-sucedida, a resposta será um código de status 204 (No Content).
    /// </remarks>
    /// <param name="id">O ID do cinema a ser atualizado.</param>
    /// <param name="cinemaDto">Os novos dados do cinema no formato UpdatecinemaDto.</param>
    /// <response code="204">Atualização bem-sucedida do cinema correspondente ao ID.</response>
    /// <response code="404">Se nenhum cinema for encontrado para o ID fornecido.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult AtualizaCinema(int id, [FromBody] UpdateCinemaDto cinemaDto)
    {
        var cinema = _context.Cinemas.FirstOrDefault(cinema => cinema.Id == id);
        if (cinema == null) return NotFound();
        _mapper.Map(cinemaDto, cinema);
        _context.SaveChanges();
        return NoContent();
    }

    /// <summary>
    /// Atualiza parcialmente as informações de um cinema existente com base no ID fornecido usando JSON Patch.
    /// </summary>
    /// <remarks>
    /// Esta operação permite atualizar parcialmente as informações de um cinema existente no banco de dados, utilizando o ID fornecido na URL e um objeto JSON Patch no corpo da requisição.
    /// 
    /// Exemplo de operações de patch:
    /// [{ "op": "replace", "path": "/titulo", "value": "Novo Título" }].
    /// 
    /// O corpo da requisição deve conter as operações de patch no formato JSON Patch (RFC 6902) para atualização parcial do cinema correspondente ao ID informado.
    /// 
    /// Se o cinema com o ID correspondente não for encontrado, será retornado um código de status 404 (Not Found).
    /// 
    /// Após a atualização parcial bem-sucedida, a resposta será um código de status 204 (No Content).
    /// 
    /// Caso as operações de patch sejam inválidas ou não aplicáveis, será retornado um código de status 400 (Bad Request) indicando problemas com a requisição.
    /// </remarks>
    /// <param name="id">O ID do cinema a ser atualizado parcialmente.</param>
    /// <param name="patch">O objeto JsonPatchDocument contendo as operações de patch.</param>
    /// <response code="204">Atualização parcial bem-sucedida do cinema correspondente ao ID.</response>
    /// <response code="400">Se o patch não for aplicável ou inválido.</response>
    /// <response code="404">Se nenhum cinema for encontrado para o ID fornecido.</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult AtualizaCinemaParcial(int id, JsonPatchDocument<UpdateCinemaDto> patch)
    {
        var cinema = _context.Cinemas.FirstOrDefault(cinema => cinema.Id == id);
        if (cinema == null) return NotFound();

        var cinemaParaAtualizar = _mapper.Map<UpdateCinemaDto>(cinema);
        patch.ApplyTo(cinemaParaAtualizar, ModelState);

        if (!TryValidateModel(cinemaParaAtualizar))
        {
            return ValidationProblem(ModelState);
        }
        _mapper.Map(cinemaParaAtualizar, cinema);
        _context.SaveChanges();
        return NoContent();
    }

    /// <summary>
    /// Remove um cinema do banco de dados com base no ID fornecido.
    /// </summary>
    /// <remarks>
    /// Esta operação permite excluir um cinema existente no banco de dados utilizando o ID fornecido na URL.
    /// 
    /// Se o cinema com o ID correspondente não for encontrado, será retornado um código de status 404 (Not Found).
    /// 
    /// Após a exclusão bem-sucedida, a resposta será um código de status 204 (No Content).
    /// </remarks>
    /// <param name="id">O ID do cinema a ser excluído.</param>
    /// <response code="204">Exclusão bem-sucedida do cinema correspondente ao ID.</response>
    /// <response code="404">Se nenhum cinema for encontrado para o ID fornecido.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeletaCinema(int id)
    {
        var cinema = _context.Cinemas.FirstOrDefault(cinema => cinema.Id == id);
        if (cinema== null) return NotFound();
        _context.Remove(cinema);
        _context.SaveChanges();
        return NoContent();
    }
}
