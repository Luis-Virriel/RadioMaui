using System.ComponentModel.DataAnnotations;

namespace mauiApp1Prueba.Models
{
    public class Sponsor
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del patrocinador es requerido")]
        [StringLength(100, ErrorMessage = "El nombre no puede exceder los 100 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "La descripción no puede exceder los 500 caracteres")]
        public string? Description { get; set; }

        public string? LogoPath { get; set; }

        [Required(ErrorMessage = "La dirección es requerida")]
        [StringLength(200, ErrorMessage = "La dirección no puede exceder los 200 caracteres")]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90")]
        public double Latitude { get; set; }

        [Required]
        [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180")]
        public double Longitude { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;
    }
}