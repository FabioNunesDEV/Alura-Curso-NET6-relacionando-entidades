using AutoMapper;
using FilmesApi.Data;
using FilmesApi.Data.DTO;
using FilmesApi.Models;
using FilmesApi.Profiles;
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
    /// Adiciona um filme ao banco de dados
    /// </summary>
    /// <param name="filmeDTO">Objeto com os campos necessários para criação de um filme</param>
    /// <returns>IActionResult</returns>
    /// <response code="201">Caso inserção seja feita com sucesso.</response>
    [HttpPost("adicionar")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult Adicionar([FromBody] CreateFilmeDTO filmeDTO)
    {
        Filme filme = _mapper.Map<Filme>(filmeDTO);
        _context.Filmes.Add(filme);
        _context.SaveChanges();
        return CreatedAtAction(nameof(RecuperarPorId), new { id = filme.Id }, filme);
    }

    /// <summary>
    /// Adiciona filmes em lote
    /// </summary>
    /// <param name="filmesDTO">Objeto com coleção de filmes a serem adicionados</param>
    /// <returns>IActionResult</returns>
    /// <response code="201">Caso inserção seja feita com sucesso.</response>
    [HttpPost("adicionarEmLote")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public IActionResult AdicionarEmLote([FromBody] List<CreateFilmeDTO> filmesDTO)
    {
        foreach (var filmeDTO in filmesDTO)
        {
            // Converte o DTO para o modelo
            Filme filme = _mapper.Map<Filme>(filmeDTO);

            _context.Filmes.Add(filme);
            _context.SaveChanges();
            Console.WriteLine($"Id: {filme.Id} - Titulo: {filme.Titulo} - Duração: {filme.Duracao}");
        }

        return CreatedAtAction(nameof(RecuperarTodos), new { }, filmesDTO);
    }

    /// <summary>
    /// Obtem todos os filmes cadastrados
    /// </summary>
    /// <returns>Retorna uma coleção de filmes</returns>
    /// <response code="200">Caso a leitura seja feita com sucesso.</response>
    [HttpGet("recuperarTodos")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IEnumerable<ReadFilmeDTO> RecuperarTodos()
    {
        return _mapper.Map<List<ReadFilmeDTO>>(_context.Filmes);
    }

    /// <summary>
    /// Obtem filmes com paginação
    /// </summary>
    /// <param name="skip">Posição inicial</param>
    /// <param name="take">Quanto filme obtem a partir da posição inicial</param>
    /// <returns>Retorna uma coleção de filmes</returns>
    /// <response code="200">Caso a leitura seja feita com sucesso.</response>
    [HttpGet("paginacao/skip/{skip}/take/{take}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IEnumerable<ReadFilmeDTO> RecuperarPaginacao([FromRoute] int skip = 0, [FromRoute] int take = 10)
    {
        return _mapper.Map<List<ReadFilmeDTO>>(_context.Filmes.Skip(skip).Take(take));
    }


    /// <summary>
    /// Obtem filme por id
    /// </summary>
    /// <param name="id">Id do filme</param>
    /// <returns>Retorna informações de um filme especifico</returns>
    /// <response code="200">Caso a leitura seja feita com sucesso.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult RecuperarPorId(int id)
    {
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);
        if (filme == null) return NotFound();
        var filmeDTO = _mapper.Map<ReadFilmeDTO>(filme);
        return Ok(filmeDTO);
    }

    /// <summary>
    /// Atualiza um filme informando o Id
    /// </summary>
    /// <param name="id">Id do filme</param>
    /// <param name="filmeDTO">Objeto com os campos necessários para criação de um filme</param>
    /// <returns></returns>
    /// <response code="204">No Content</response>
    [HttpPut("atualizarFilme/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult AtualizarFilme(int id, [FromBody] UpdateFilmeDTO filmeDTO)
    {
        // Verifica se o filme existe
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);

        // Se não existir, retorna NotFound
        if (filme == null) return NotFound();

        // Atualiza o filme
        _mapper.Map(filmeDTO, filme);
        _context.SaveChanges();

        // Retorna status code 204 - NoContent
        return NoContent();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id">Id do filme</param>
    /// <param name="patch">Campo que se deseja alterar</param>
    /// <returns></returns>
    /// <response code="204">No Content</response>
    [HttpPatch("atualizarFilmeParcial/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult AtualizarFilmeParcial(int id, JsonPatchDocument<UpdateFilmeDTO> patch)
    {
        // Verifica se o filme existe
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);

        // Se não existir, retorna NotFound
        if (filme == null) return NotFound();

        // Converte o filme para um DTO
        var filmeParaAtualizar = _mapper.Map<UpdateFilmeDTO>(filme);

        // Aplica o patch ao filme para atualizar
        patch.ApplyTo(filmeParaAtualizar, ModelState);

        // Verifica se o modelo é válido
        if (!TryValidateModel(filmeParaAtualizar))
        {
            return ValidationProblem(ModelState);
        }

        // Atualiza o filme
        _mapper.Map(filmeParaAtualizar, filme);
        _context.SaveChanges();

        // Retorna status code 204 - NoContent
        return NoContent();
    }

    /// <summary>
    /// Delete um filme informando o Id
    /// </summary>
    /// <param name="id">Id do filme</param>
    /// <returns></returns>
    /// <response code="204">No Content</response>
    [HttpDelete("deletarFilme/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult DeletarFilme(int id)
    {
        // Verifica se o filme existe
        var filme = _context.Filmes.FirstOrDefault(filme => filme.Id == id);

        // Se não existir, retorna NotFound
        if (filme == null) return NotFound();

        // Remove o filme
        _context.Filmes.Remove(filme);
        _context.SaveChanges();

        // Retorna status code 204 - NoContent
        return NoContent();
    }
}
