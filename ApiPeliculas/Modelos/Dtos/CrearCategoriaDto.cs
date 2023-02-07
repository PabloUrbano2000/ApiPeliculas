﻿using System.ComponentModel.DataAnnotations;

namespace ApiPeliculas.Modelos.Dtos
{
    public class CrearCategoriaDto
    {

        // Esta validación es importante sino se creará vacio el nombre
        [Required(ErrorMessage ="El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage ="El número máximo de caracteres es de 100!")]
        public string Nombre { get; set; }
    }
}
