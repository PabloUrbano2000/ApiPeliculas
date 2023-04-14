using ApiPeliculas.Modelos;
using ApiPeliculas.Modelos.Dtos;
using ApiPeliculas.Repositorio.IRepositorio;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ApiPeliculas.Controllers
{

    [Route("api/peliculas")]
    [ApiController]
    public class PeliculasController : ControllerBase
    {
        private readonly IPeliculaRepositorio _pelRepo;
        private readonly IMapper _mapper;
        public PeliculasController(IPeliculaRepositorio pelRepo, IMapper mapper)
        {
            _pelRepo = pelRepo;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetPeliculas()
        {
            var listaPeliculas = _pelRepo.GetPeliculas();
            var listaPeliculasDto = new List<PeliculaDto>();
            foreach (var item in listaPeliculas)
            {
                listaPeliculasDto.Add(_mapper.Map<PeliculaDto>(item));
            }

            return Ok(listaPeliculasDto);
        }

        [AllowAnonymous]
        [HttpGet("{peliculaId:int}", Name = "GetPelicula")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetPelicula(int peliculaId)
        {
            var itemPelicula = _pelRepo.GetPelicula(peliculaId);
            if (itemPelicula == null)
            {
                return NotFound();
            }

            var itemPeliculaDto = _mapper.Map<PeliculaDto>(itemPelicula);

            return Ok(itemPeliculaDto);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ProducesResponseType(201, Type = typeof(PeliculaDto))]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult CrearPelicula([FromBody] PeliculaDto peliculaDto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (peliculaDto == null)
            {
                return BadRequest(ModelState);
            }
            if (_pelRepo.ExistePelicula(peliculaDto.Nombre))
            {
                ModelState.AddModelError("", "La película ya existe");
                return StatusCode(404, ModelState);
            }

            var pelicula = _mapper.Map<Pelicula>(peliculaDto);
            if (!_pelRepo.CrearPelicula(pelicula))
            {
                ModelState.AddModelError("", $"Algo salió mal guardando el registro {pelicula.Nombre}");
                return StatusCode(500, ModelState);
            }

            return CreatedAtRoute("GetPelicula", new { peliculaId = pelicula.Id }, pelicula);
        }


        [Authorize(Roles = "admin")]
        [HttpPatch("{peliculaId:int}", Name = "actualizar-patch-pelicula")]
        [ProducesResponseType(200, Type = typeof(PeliculaDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult ActualizarPatchPelicula(int peliculaId, [FromBody] PeliculaDto peliculaDto)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (peliculaDto == null || peliculaId != peliculaDto.Id)
            {
                return BadRequest(ModelState);
            }

            var pelicula = _mapper.Map<Pelicula>(peliculaDto);
            if (!_pelRepo.ExistePelicula(peliculaId))
            {
                return NotFound();
            }
            if (!_pelRepo.ActualizarPelicula(pelicula))
            {
                ModelState.AddModelError("", $"Algo salió mal actualizando el registro {pelicula.Nombre}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{peliculaId:int}", Name = "BorrarPelicula")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public IActionResult BorrarPelicula(int peliculaId)
        {
            if (!_pelRepo.ExistePelicula(peliculaId))
            {
                return NotFound();
            }


            var pelicula = _pelRepo.GetPelicula(peliculaId);
            if (!_pelRepo.BorrarPelicula(pelicula))
            {
                ModelState.AddModelError("", $"Algo salió mal borrando el registro {pelicula.Nombre}");
                return StatusCode(500, ModelState);
            }

            return NoContent();
        }

        [AllowAnonymous]
        [HttpGet("peliculas-categoria/{categoriaId:int}")]
        public IActionResult GetPeliculasPorCategoria(int categoriaId)
        {
            var listaPeliculas = _pelRepo.GetPeliculasPorCategoria(categoriaId);
            if (listaPeliculas == null) { 
                return NotFound();
            }

            var itemPeliculas = new List<PeliculaDto>();
            foreach (var item in listaPeliculas)
            {
                itemPeliculas.Add(_mapper.Map<PeliculaDto>(item));
            }

            return Ok(itemPeliculas);
        }

        [AllowAnonymous]

        [HttpGet("buscar")]
        public IActionResult Buscar(string nombre)
        {
            try
            {
                var resultado = _pelRepo.BuscarPelicula(nombre.Trim());
                if (resultado.Any())
                {
                    return Ok(resultado);
                }
                return NotFound();
            } catch(Exception e)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Error recuperando datos");
            }
        }
    }
}
