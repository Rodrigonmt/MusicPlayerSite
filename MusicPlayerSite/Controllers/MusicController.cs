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

        public IActionResult Index()
        {
            var separadosPath = Path.Combine(_env.WebRootPath, "separados");

            var files = Directory.Exists(separadosPath)
                ? Directory.GetFiles(separadosPath, "*.mp3").Select(f => Path.GetFileName(f)).ToList()
                : new System.Collections.Generic.List<string>();

            ViewBag.Files = files;

            // 👇 Aqui você pega o nome da pasta gerada (sem extensão)
            ViewBag.FolderName = TempData["FolderName"]?.ToString();

            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file != null && Path.GetExtension(file.FileName).ToLower() == ".mp3")
            {
                var uploadsPath = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsPath);

                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
                var filePath = Path.Combine(uploadsPath, file.FileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                var outputPath = Path.Combine(_env.WebRootPath, "separados");
                Directory.CreateDirectory(outputPath);

                System.Console.WriteLine($"Comando: demucs -n mdx_extra_q -o \"{outputPath}\" \"{filePath}\"");
                System.Console.WriteLine($"Arquivo existe? {System.IO.File.Exists(filePath)}");



                var psi = new ProcessStartInfo
                {
                    FileName = @"C:\Users\Rodrigo\AppData\Local\Programs\Python\Python39\Scripts\demucs.exe",
                    Arguments = $"-n mdx_extra_q -o \"{outputPath}\" \"{filePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true

                };


                psi.Environment["PATH"] += @";C:\ffmpeg\bin";

                using var process = Process.Start(psi);
                string output = await process.StandardOutput.ReadToEndAsync();
                string errors = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    System.Console.WriteLine("Erro ao executar demucs:");
                    System.Console.WriteLine("STDOUT:");
                    System.Console.WriteLine(output);
                    System.Console.WriteLine("STDERR:");
                    System.Console.WriteLine(errors);
                }
                else
                {
                    System.Console.WriteLine("Demucs executado com sucesso:");
                    System.Console.WriteLine(output);
                }

                // 👇 Salva nome da pasta gerada para a View usar
                TempData["FolderName"] = fileNameWithoutExt;

                // === EXTRAÇÃO DE SOPROS A PARTIR DO other.wav ===
                var pathOther = Path.Combine(outputPath, "mdx_extra_q", fileNameWithoutExt, "other.wav");
                var pathBrass = Path.Combine(outputPath, "mdx_extra_q", fileNameWithoutExt, "brass.wav");

                var scriptPath = Path.Combine(_env.ContentRootPath, "Scripts", "extrair_sopros.py");

                var soprosProcess = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"\"{scriptPath}\" \"{pathOther}\" \"{pathBrass}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var processSopros = Process.Start(soprosProcess);
                string soprosOut = await processSopros.StandardOutput.ReadToEndAsync();
                string soprosErr = await processSopros.StandardError.ReadToEndAsync();
                await processSopros.WaitForExitAsync();

                if (processSopros.ExitCode != 0)
                {
                    System.Console.WriteLine("Erro ao extrair sopros:");
                    System.Console.WriteLine(soprosErr);
                }
                else
                {
                    System.Console.WriteLine("Extração de sopros realizada com sucesso:");
                    System.Console.WriteLine(soprosOut);
                }



            }

            return Content("Conversão concluída com sucesso!");
        }

        [HttpPost]
        public async Task<IActionResult> UploadSeparatedWavs([FromForm] List<IFormFile> files, [FromForm] string folderName)
        {
            if (string.IsNullOrWhiteSpace(folderName))
                folderName = "upload_manual";

            var outputPath = Path.Combine(_env.WebRootPath, "separados", "mdx_extra_q", folderName);
            Directory.CreateDirectory(outputPath);

            var knownNames = new[] { "vocals.wav", "drums.wav", "bass.wav", "other.wav", "brass.wav" };
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

            return Content("Conversão concluída com sucesso!");
        }


    }
}
