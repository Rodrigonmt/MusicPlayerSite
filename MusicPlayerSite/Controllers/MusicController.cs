using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MusicPlayerSite.Controllers
{
    public class MusicController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public MusicController(IWebHostEnvironment env)
        {
            _env = env;
        }

        public IActionResult Index(string folder)
        {
            if (!string.IsNullOrWhiteSpace(folder))
                TempData["FolderName"] = folder;

            ViewBag.FolderName = TempData["FolderName"]?.ToString();
            ViewBag.Files = new List<string>();
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file, [FromForm] string destino)
        {
            if (file == null || Path.GetExtension(file.FileName).ToLower() != ".mp3")
                return Json(new { sucesso = false, mensagem = "Arquivo inválido ou não informado." });

            try
            {
                // Salvar o .mp3 no servidor
                var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsPath);

                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
                var filePath = Path.Combine(uploadsPath, file.FileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Criar pasta final escolhida pelo usuário
                var destinoFinal = Path.Combine(_env.WebRootPath, "separados", "mdx_extra_q", destino, fileNameWithoutExt);
                Directory.CreateDirectory(destinoFinal);

                // Iniciar processo de separação com demucs
                var psi = new ProcessStartInfo
                {
                    FileName = @"C:\Users\Rodrigo\AppData\Local\Programs\Python\Python39\Scripts\demucs.exe",
                    Arguments = $"-n mdx_extra_q -o \"{Path.Combine(_env.WebRootPath, "separados", "mdx_extra_q")}\" \"{filePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                // Adiciona ffmpeg ao PATH
                psi.Environment["PATH"] += @";C:\ffmpeg\bin";

                using var process = new Process();
                process.StartInfo = psi;

                string stdOut = "", stdErr = "";
                process.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) stdOut += e.Data + "\n"; };
                process.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) stdErr += e.Data + "\n"; };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    return Json(new { sucesso = false, mensagem = "Erro na conversão", erro = stdErr });
                }

                TempData["FolderName"] = Path.Combine(destino, fileNameWithoutExt); // Para uso na view

                return Json(new { sucesso = true, pasta = TempData["FolderName"], log = stdOut });
            }
            catch (Exception ex)
            {
                return Json(new { sucesso = false, mensagem = "Erro ao converter o arquivo", erro = ex.Message });
            }
        }



        [HttpPost]
        public async Task<IActionResult> UploadSeparatedWavs([FromForm] List<IFormFile> files, [FromForm] string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
                folderName = "upload_manual";

            var outputPath = Path.Combine(_env.WebRootPath, "separados", "mdx_extra_q", folderName);
            Directory.CreateDirectory(outputPath);

            var knownNames = new[] { "vocals.wav", "drums.wav", "bass.wav", "other.wav" }; // removido brass.wav
            var fileMap = new Dictionary<string, string>(); // para JS: id do player => caminho do arquivo

            foreach (var file in files)
            {
                var filename = Path.GetFileName(file.FileName).ToLower();

                if (knownNames.Contains(filename))
                {
                    var destPath = Path.Combine(outputPath, filename);
                    using var stream = new FileStream(destPath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    var id = Path.GetFileNameWithoutExtension(filename); // ex: vocals
                    fileMap[id] = $"/separados/mdx_extra_q/{folderName}/{filename}";
                }
            }

            return Json(fileMap);
        }

        [HttpPost]
        public async Task<IActionResult> ConverterMusica(IFormFile arquivo)
        {
            if (arquivo == null || arquivo.Length == 0)
                return Content("Arquivo inválido");

            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(arquivo.FileName);

            var caminhoArquivo = Path.Combine("wwwroot", "uploads", arquivo.FileName);
            using (var stream = new FileStream(caminhoArquivo, FileMode.Create))
            {
                await arquivo.CopyToAsync(stream);
            }

            var processo = new Process();
            processo.StartInfo.FileName = "python";
            processo.StartInfo.Arguments = $"Scripts/separar_instrumentos.py \"{caminhoArquivo}\"";
            processo.StartInfo.UseShellExecute = false;
            processo.StartInfo.RedirectStandardOutput = true;
            processo.StartInfo.RedirectStandardError = true;
            processo.StartInfo.CreateNoWindow = true;
            processo.Start();

            string output = await processo.StandardOutput.ReadToEndAsync();
            string erro = await processo.StandardError.ReadToEndAsync();

            processo.WaitForExit();

            if (!string.IsNullOrEmpty(erro))
                return Content("Erro na conversão: " + erro);

            return Json(new { success = true, nomeArquivo = fileNameWithoutExt });
        }




    }
}
