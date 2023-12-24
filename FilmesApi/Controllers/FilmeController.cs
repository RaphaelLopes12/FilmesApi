using AutoMapper;
using FilmesApi.Data;
using FilmesApi.Data.Dtos;
using FilmesApi.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace FilmesApi.Controllers;

[ApiController]
[Route("[controller]")]
public class FilmeController : ControllerBase
{
    private FilmeContext _context;
    private IMapper _mapper;

    public FilmeController(FilmeContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    /// <summary>
    /// Adiciona filme no banco de dados.
    /// </summary>
    /// <param name="filmeDto">Objeto com os campos necessários para a criação de um filme.</param>
    /// <returns></returns>
    /// <response code="201">Caso inserção seja feita com sucesso.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult AdicionaFilme([FromBody] CreateFilmeDto filmeDto)
    {
        Filme filme = _mapper.Map<Filme>(filmeDto);
        _context.Filmes.Add(filme);
        _context.SaveChanges();
        return CreatedAtAction(nameof(RecuperaFilmeId), new { id = filme.Id}, filme);
    }

    /// <summary>
    /// Retorna filme(s) previamente inserido(s) no banco de dados.
    /// </summary>
    /// <remarks>
    /// Esta operação permite recuperar filmes de forma paginada. Por padrão, caso nenhum parâmetro seja informado, serão retornados até 50 filmes por página, se houver mais de 50 registros no banco de dados.
    /// 
    /// Use o parâmetro {skip} seguido pelo número da página desejada para acessar uma página específica. Por exemplo, `skip=2` irá pular para a página 2.
    /// 
    /// Use o parâmetro {take} seguido pelo número de registros desejados por página. Por exemplo, `take=20` retornará 20 filmes por página.
    /// 
    /// A ausência de registros não afetará o código de status de resposta, que será 200 (OK) independentemente da presença de dados.
    /// </remarks>
    /// <param name="skip">Número da página a ser pulada.</param>
    /// <param name="take">Quantidade de itens por página a serem retornados.</param>
    /// <returns>Uma lista dos filmes recuperados.</returns>
    /// <response code="200">Retorna independentemente da presença de registros.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IEnumerable<ReadFilmeDto> RecuperaFilme([FromQuery] int skip = 0,
        [FromQuery] int take = 50, [FromQuery] string? nomeCinema = null)
    {
        if(nomeCinema == null)
        {
            return _mapper.Map<List<ReadFilmeDto>>(_context.Filmes.Skip(skip).Take(take).ToList());
        }
        return _mapper.Map<List<ReadFilmeDto>>(_context.Filmes.Skip
            (skip).Take(take).Where(filmes => filmes.Sessoes
                  .Any(sessao => sessao.Cinema.Nome == nomeCinema)).ToList());
    }

    /// <summary>
    /// Retorna um filme com base no ID fornecido.
    /// </summary>
    /// <remarks>
    /// Esta operação permite recuperar um filme específico do banco de dados usando o ID fornecido como parâmetro na URL.
    /// Se o filme com o ID correspondente for encontrado, será retornado o filme correspondente juntamente com os detalhes em formato DTO (Data Transfer Object).
    /// Se nenhum filme for encontrado para o ID fornecido, será retornado um código de status 404 (Not Found).
    /// </remarks>
    /// <param name="id">O ID do filme a ser recuperado.</param>
    /// <returns>Detalhes do filme correspondente ao ID fornecido.</returns>
    /// <response code="200">Retorna os detalhes do filme correspondente ao ID.</response>
    /// <response code="404">Se nenhum filme for encontrado para o ID fornecido.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult RecuperaFilmeId(int id)
    {
        var filme = _context.Filmes.FirstOrDefault(filmes => filmes.Id == id);
        if (filme == null) return NotFound();
        var filmeDto = _mapper.Map<ReadFilmeDto>(filme);
        return Ok(filmeDto);
    }

    /// <summary>
    /// Atualiza as informações de um filme existente com base no ID fornecido.
    /// </summary>
    /// <remarks>
    /// Esta operação permite atualizar as informações de um filme existente no banco de dados, utilizando o ID fornecido na URL e os dados fornecidos no corpo da requisição.
    /// O corpo da requisição deve conter os dados no formato UpdateFilmeDto para atualização do filme correspondente ao ID informado.
    /// Se o filme com o ID correspondente não for encontrado, será retornado um código de status 404 (Not Found).
    /// Após a atualização bem-sucedida, a resposta será um código de status 204 (No Content).
    /// </remarks>
    /// <param name="id">O ID do filme a ser atualizado.</param>
    /// <param name="filmeDto">Os novos dados do filme no formato UpdateFilmeDto.</param>
    /// <response code="204">Atualização bem-sucedida do filme correspondente ao ID.</response>
    /// <response code="404">Se nenhum filme for encontrado para o ID fornecido.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult AtualizaFilme(int id, [FromBody] UpdateFilmeDto filmeDto)
    {
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);
        if (filme == null) return NotFound();
        _mapper.Map(filmeDto, filme);
        _context.SaveChanges();
        return NoContent();
    }

    /// <summary>
    /// Atualiza parcialmente as informações de um filme existente com base no ID fornecido usando JSON Patch.
    /// </summary>
    /// <remarks>
    /// Esta operação permite atualizar parcialmente as informações de um filme existente no banco de dados, utilizando o ID fornecido na URL e um objeto JSON Patch no corpo da requisição.
    /// 
    /// Exemplo de operações de patch:
    /// [{ "op": "replace", "path": "/titulo", "value": "Novo Título" }].
    /// 
    /// O corpo da requisição deve conter as operações de patch no formato JSON Patch (RFC 6902) para atualização parcial do filme correspondente ao ID informado.
    /// 
    /// Se o filme com o ID correspondente não for encontrado, será retornado um código de status 404 (Not Found).
    /// 
    /// Após a atualização parcial bem-sucedida, a resposta será um código de status 204 (No Content).
    /// 
    /// Caso as operações de patch sejam inválidas ou não aplicáveis, será retornado um código de status 400 (Bad Request) indicando problemas com a requisição.
    /// </remarks>
    /// <param name="id">O ID do filme a ser atualizado parcialmente.</param>
    /// <param name="patch">O objeto JsonPatchDocument contendo as operações de patch.</param>
    /// <response code="204">Atualização parcial bem-sucedida do filme correspondente ao ID.</response>
    /// <response code="400">Se o patch não for aplicável ou inválido.</response>
    /// <response code="404">Se nenhum filme for encontrado para o ID fornecido.</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult AtualizaFilmeParcial(int id, JsonPatchDocument<UpdateFilmeDto> patch)
    {
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);
        if (filme == null) return NotFound();

        var filmeParaAtualizar = _mapper.Map<UpdateFilmeDto>(filme);
        patch.ApplyTo(filmeParaAtualizar, ModelState);

        if(!TryValidateModel(filmeParaAtualizar))
        {
            return ValidationProblem(ModelState);
        }
        _mapper.Map(filmeParaAtualizar, filme);
        _context.SaveChanges();
        return NoContent();
    }

    /// <summary>
    /// Remove um filme do banco de dados com base no ID fornecido.
    /// </summary>
    /// <remarks>
    /// Esta operação permite excluir um filme existente no banco de dados utilizando o ID fornecido na URL.
    /// 
    /// Se o filme com o ID correspondente não for encontrado, será retornado um código de status 404 (Not Found).
    /// 
    /// Após a exclusão bem-sucedida, a resposta será um código de status 204 (No Content).
    /// </remarks>
    /// <param name="id">O ID do filme a ser excluído.</param>
    /// <response code="204">Exclusão bem-sucedida do filme correspondente ao ID.</response>
    /// <response code="404">Se nenhum filme for encontrado para o ID fornecido.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeletaFilme(int id)
    {
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);
        if (filme == null) return NotFound();
        _context.Remove(filme);
        _context.SaveChanges();
        return NoContent();
    }
}
