using AutoMapper;
using FilmesApi.Data.Dtos;
using FilmesApi.Data;
using FilmesApi.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace FilmesApi.Controllers;

[ApiController]
[Route("[controller]")]
public class EnderecoController : ControllerBase
{
    private FilmeContext _context;
    private IMapper _mapper;

    public EnderecoController(FilmeContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    /// <summary>
    /// Adiciona endereco no banco de dados.
    /// </summary>
    /// <param name="enderecoDto">Objeto com os campos necessários para a criação de um endereco.</param>
    /// <returns></returns>
    /// <response code="201">Caso inserção seja feita com sucesso.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult AdicionaEndereco([FromBody] CreateEnderecoDto enderecoDto)
    {
        Endereco endereco = _mapper.Map<Endereco>(enderecoDto);
        _context.Enderecos.Add(endereco);
        _context.SaveChanges();
        return CreatedAtAction(nameof(RecuperaEnderecoId), new { id = endereco.Id }, endereco);
    }

    /// <summary>
    /// Retorna endereco(s) previamente inserido(s) no banco de dados.
    /// </summary>
    /// <remarks>
    /// Esta operação permite recuperar enderecos de forma paginada. Por padrão, caso nenhum parâmetro seja informado, serão retornados até 50 enderecos por página, se houver mais de 50 registros no banco de dados.
    /// 
    /// Use o parâmetro {skip} seguido pelo número da página desejada para acessar uma página específica. Por exemplo, `skip=2` irá pular para a página 2.
    /// 
    /// Use o parâmetro {take} seguido pelo número de registros desejados por página. Por exemplo, `take=20` retornará 20 enderecos por página.
    /// 
    /// A ausência de registros não afetará o código de status de resposta, que será 200 (OK) independentemente da presença de dados.
    /// </remarks>
    /// <param name="skip">Número da página a ser pulada.</param>
    /// <param name="take">Quantidade de itens por página a serem retornados.</param>
    /// <returns>Uma lista dos enderecos recuperados.</returns>
    /// <response code="200">Retorna independentemente da presença de registros.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IEnumerable<ReadEnderecoDto> RecuperarEnderecos([FromQuery] int skip = 0, [FromQuery] int take = 50)
    {
        return _mapper.Map<List<ReadEnderecoDto>>(_context.Enderecos.Skip(skip).Take(take));
    }

    /// <summary>
    /// Retorna um endereco com base no ID fornecido.
    /// </summary>
    /// <remarks>
    /// Esta operação permite recuperar um endereco específico do banco de dados usando o ID fornecido como parâmetro na URL.
    /// Se o endereco com o ID correspondente for encontrado, será retornado o endereco correspondente juntamente com os detalhes em formato DTO (Data Transfer Object).
    /// Se nenhum endereco for encontrado para o ID fornecido, será retornado um código de status 404 (Not Found).
    /// </remarks>
    /// <param name="id">O ID do endereco a ser recuperado.</param>
    /// <returns>Detalhes do endereco correspondente ao ID fornecido.</returns>
    /// <response code="200">Retorna os detalhes do endereco correspondente ao ID.</response>
    /// <response code="404">Se nenhum endereco for encontrado para o ID fornecido.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult RecuperaEnderecoId(int id)
    {
        var endereco = _context.Enderecos.FirstOrDefault(enderecos => enderecos.Id == id);
        if (endereco == null) return NotFound();
        var enderecoDto = _mapper.Map<ReadEnderecoDto>(endereco);
        return Ok(enderecoDto);
    }

    /// <summary>
    /// Atualiza as informações de um endereco existente com base no ID fornecido.
    /// </summary>
    /// <remarks>
    /// Esta operação permite atualizar as informações de um endereco existente no banco de dados, utilizando o ID fornecido na URL e os dados fornecidos no corpo da requisição.
    /// O corpo da requisição deve conter os dados no formato UpdateenderecoDto para atualização do endereco correspondente ao ID informado.
    /// Se o endereco com o ID correspondente não for encontrado, será retornado um código de status 404 (Not Found).
    /// Após a atualização bem-sucedida, a resposta será um código de status 204 (No Content).
    /// </remarks>
    /// <param name="id">O ID do endereco a ser atualizado.</param>
    /// <param name="enderecoDto">Os novos dados do endereco no formato UpdateenderecoDto.</param>
    /// <response code="204">Atualização bem-sucedida do endereco correspondente ao ID.</response>
    /// <response code="404">Se nenhum endereco for encontrado para o ID fornecido.</response>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult AtualizaEndereco(int id, [FromBody] UpdateEnderecoDto enderecoDto)
    {
        var endereco = _context.Enderecos.FirstOrDefault(enderecos => enderecos.Id == id);
        if (endereco == null) return NotFound();
        _mapper.Map(enderecoDto, endereco);
        _context.SaveChanges();
        return NoContent();
    }

    /// <summary>
    /// Atualiza parcialmente as informações de um endereco existente com base no ID fornecido usando JSON Patch.
    /// </summary>
    /// <remarks>
    /// Esta operação permite atualizar parcialmente as informações de um endereco existente no banco de dados, utilizando o ID fornecido na URL e um objeto JSON Patch no corpo da requisição.
    /// 
    /// Exemplo de operações de patch:
    /// [{ "op": "replace", "path": "/titulo", "value": "Novo Título" }].
    /// 
    /// O corpo da requisição deve conter as operações de patch no formato JSON Patch (RFC 6902) para atualização parcial do endereco correspondente ao ID informado.
    /// 
    /// Se o endereco com o ID correspondente não for encontrado, será retornado um código de status 404 (Not Found).
    /// 
    /// Após a atualização parcial bem-sucedida, a resposta será um código de status 204 (No Content).
    /// 
    /// Caso as operações de patch sejam inválidas ou não aplicáveis, será retornado um código de status 400 (Bad Request) indicando problemas com a requisição.
    /// </remarks>
    /// <param name="id">O ID do endereco a ser atualizado parcialmente.</param>
    /// <param name="patch">O objeto JsonPatchDocument contendo as operações de patch.</param>
    /// <response code="204">Atualização parcial bem-sucedida do endereco correspondente ao ID.</response>
    /// <response code="400">Se o patch não for aplicável ou inválido.</response>
    /// <response code="404">Se nenhum endereco for encontrado para o ID fornecido.</response>
    [HttpPatch("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult AtualizaEnderecoParcial(int id, JsonPatchDocument<UpdateEnderecoDto> patch)
    {
        var endereco = _context.Enderecos.FirstOrDefault(enderecos => enderecos.Id == id);
        if (endereco == null) return NotFound();

        var enderecoParaAtualizar = _mapper.Map<UpdateEnderecoDto>(endereco);
        patch.ApplyTo(enderecoParaAtualizar, ModelState);

        if (!TryValidateModel(enderecoParaAtualizar))
        {
            return ValidationProblem(ModelState);
        }
        _mapper.Map(enderecoParaAtualizar, endereco);
        _context.SaveChanges();
        return NoContent();
    }

    /// <summary>
    /// Remove um endereco do banco de dados com base no ID fornecido.
    /// </summary>
    /// <remarks>
    /// Esta operação permite excluir um endereco existente no banco de dados utilizando o ID fornecido na URL.
    /// 
    /// Se o endereco com o ID correspondente não for encontrado, será retornado um código de status 404 (Not Found).
    /// 
    /// Após a exclusão bem-sucedida, a resposta será um código de status 204 (No Content).
    /// </remarks>
    /// <param name="id">O ID do endereco a ser excluído.</param>
    /// <response code="204">Exclusão bem-sucedida do endereco correspondente ao ID.</response>
    /// <response code="404">Se nenhum endereco for encontrado para o ID fornecido.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeletaEndereco(int id)
    {
        var endereco = _context.Enderecos.FirstOrDefault(enderecos => enderecos.Id == id);
        if (endereco == null) return NotFound();
        _context.Remove(endereco);
        _context.SaveChanges();
        return NoContent();
    }
}
