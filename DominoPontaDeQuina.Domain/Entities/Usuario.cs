using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DominoPontaDeQuina.Domain.Entities;

[Table("Usuarios")]
public class Usuario
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(120)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    public string HashSenha { get; set; } = string.Empty;

    [Required]
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    public ICollection<Jogador> Jogadores { get; set; } = new List<Jogador>();
}
