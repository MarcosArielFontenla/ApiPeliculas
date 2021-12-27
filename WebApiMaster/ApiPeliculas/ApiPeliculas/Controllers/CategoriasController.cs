using ApiPeliculas.Models;
using ApiPeliculas.Models.DTOs;
using ApiPeliculas.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiPeliculas.Controllers
{
    [Route("api/Categorias")]
    [ApiController]
    public class CategoriasController : Controller
    {
        private readonly ICategoriaRepository _ctRepository;
        private readonly IMapper _mapper;

        public CategoriasController(ICategoriaRepository ctRepository, IMapper mapper)
        {
            _ctRepository = ctRepository;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetCategorias()
        {
            var listaCategorias = _ctRepository.GetCategorias();
            var listaCategoriasDTO = new List<CategoriaDTO>();

            foreach(var lista in listaCategorias)
            {
                listaCategoriasDTO.Add(_mapper.Map<CategoriaDTO>(lista));
            }
            return Ok(listaCategoriasDTO);
        }

        [HttpGet("{categoriaId:int}", Name = "GetCategoria")]
        public IActionResult GetCategoria(int categoriaId)
        {
            var itemCategoria = _ctRepository.GetCategoria(categoriaId);

            if (itemCategoria == null)
                return NotFound();
            
            var itemCategoriaDTO = _mapper.Map<CategoriaDTO>(itemCategoria);
            return Ok(itemCategoriaDTO);
        }

        [HttpPost]
        public IActionResult CrearCategoria([FromBody] CategoriaDTO categoriaDTO)
        {
            if (categoriaDTO == null)
                return BadRequest(ModelState);

            if (_ctRepository.ExisteCategoria(categoriaDTO.Nombre))
            {
                ModelState.AddModelError("", "La categoria ya existe!");
                return StatusCode(404, ModelState);
            }
            var categoria = _mapper.Map<Categoria>(categoriaDTO);

            if (!_ctRepository.CrearCategoria(categoria))
            {
                ModelState.AddModelError("", $"hubo un error, guardando el registro {categoria.Nombre}");
                return StatusCode(500, ModelState);
            }
            return CreatedAtRoute("GetCategoria", new { categoriaId = categoria.Id }, categoria);
        }

        [HttpPatch("{categoriaId:int}", Name = "ActualizarCategoria")]
        public IActionResult ActualizarCategoria(int categoriaId, [FromBody] CategoriaDTO categoriaDTO)
        {
            if (categoriaDTO == null || categoriaId != categoriaDTO.Id)
                return BadRequest(ModelState);

            var categoria = _mapper.Map<Categoria>(categoriaDTO);

            if (!_ctRepository.ActualizarCategoria(categoria))
            {
                ModelState.AddModelError("", $"Hubo un error, actualizando el registro {categoria.Nombre}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }

        [HttpDelete("{categoriaId:int}", Name = "BorrarCategoria")]
        public IActionResult BorrarCategoria(int categoriaId)
        {
            if (!_ctRepository.ExisteCategoria(categoriaId))
                return NotFound();

            var categoria = _ctRepository.GetCategoria(categoriaId);

            if (!_ctRepository.BorrarCategoria(categoria))
            {
                ModelState.AddModelError("", $"Hubo un error, borrando el registro {categoria.Nombre}");
                return StatusCode(500, ModelState);
            }
            return NoContent();
        }
    }
}
