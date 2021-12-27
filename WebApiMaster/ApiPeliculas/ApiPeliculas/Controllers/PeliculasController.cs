using ApiPeliculas.Models;
using ApiPeliculas.Models.DTOs;
using ApiPeliculas.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPeliculas.Controllers
{
    [Route("api/Peliculas")]
    [ApiController]
    public class PeliculasController : ControllerBase
    {
        private readonly IPeliculaRepository _pelRepository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _hostEnvironment;

        public PeliculasController(IPeliculaRepository pelRepository, IMapper mapper, IWebHostEnvironment hostEnvironment)
        {
            _pelRepository = pelRepository;
            _mapper = mapper;
            _hostEnvironment = hostEnvironment;
        }

        [HttpGet]
        public IActionResult GetPeliculas()
        {
            var listaPeliculas = _pelRepository.GetPeliculas();
            var listaPeliculasDTO = new List<PeliculaDTO>();

            foreach (var lista in listaPeliculas)
            {
                listaPeliculasDTO.Add(_mapper.Map<PeliculaDTO>(lista));
            }
            return Ok(listaPeliculasDTO);
        }

        [HttpGet("{peliculaId:int}", Name = "GetPelicula")]
        public IActionResult GetPelicula(int peliculaId)
        {
            var itemPelicula = _pelRepository.GetPelicula(peliculaId);

            if (itemPelicula == null)
                return NotFound();

            var itemPeliculaDTO = _mapper.Map<PeliculaDTO>(itemPelicula);
            return Ok(itemPeliculaDTO);
        }

        [HttpPost]
        public IActionResult CrearPelicula([FromForm] PeliculaCreateDTO PeliculaDTO)
        {
            if (PeliculaDTO == null)
                return BadRequest(ModelState);

            if (_pelRepository.ExistePelicula(PeliculaDTO.Nombre))
            {
                ModelState.AddModelError("", "La pelicula ya existe!");
                return StatusCode(404, ModelState);
            }

            // subida de archivos
            var archivo = PeliculaDTO.Foto;
            string rutaPrincipal = _hostEnvironment.WebRootPath;
            var archivos = HttpContext.Request.Form.Files;

            if (archivo.Length > 0)
            {
                // nueva imagen
                var nombreFoto = Guid.NewGuid().ToString();
                var subidas = Path.Combine(rutaPrincipal, @"fotos");
                var extension = Path.GetExtension(archivos[0].FileName);

                using (var fileStreams = new FileStream(Path.Combine(subidas, nombreFoto + extension), FileMode.Create))
                {
                    archivos[0].CopyTo(fileStreams);
                }
                PeliculaDTO.RutaImagen = @"\fotos\" + nombreFoto + extension;
            }
            var pelicula = _mapper.Map<Pelicula>(PeliculaDTO);

            if (!_pelRepository.CrearPelicula(pelicula))
            {
                ModelState.AddModelError("", $"hubo un error, guardando el registro {pelicula.Nombre}");
                return StatusCode(500, ModelState);
            }
            return CreatedAtRoute("GetPelicula", new { peliculaId = pelicula.Id }, pelicula);
        }

        [HttpPatch("{peliculaId:int}", Name = "ActualizarPelicula")]
        public IActionResult ActualizarPelicula(int peliculaId, [FromBody] PeliculaDTO peliculaDTO)
        {
            if (peliculaDTO == null || peliculaId != peliculaDTO.Id)
                return BadRequest(ModelState);

            var pelicula = _mapper.Map<Pelicula>(peliculaDTO);

            if (!_pelRepository.ActualizarPelicula(pelicula))
            {
                ModelState.AddModelError("", $"Hubo un error, actualizando el registro {pelicula.Nombre}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{peliculaId:int}", Name = "BorrarPelicula")]
        public IActionResult BorrarPelicula(int peliculaId)
        {
            if (!_pelRepository.ExistePelicula(peliculaId))
                return NotFound();

            var pelicula = _pelRepository.GetPelicula(peliculaId);

            if (!_pelRepository.BorrarPelicula(pelicula))
            {
                ModelState.AddModelError("", $"Hubo un error, borrando el registro {pelicula.Nombre}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
    }
}
