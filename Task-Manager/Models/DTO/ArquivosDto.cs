using Microsoft.AspNetCore.Components.Forms;

namespace Task_Manager.Models.DTO;

public record ArquivosDto(int ChamadoId, IReadOnlyList<IBrowserFile> Arquivos);