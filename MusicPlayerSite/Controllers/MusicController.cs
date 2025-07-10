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
            var separadosPath = Path.Combine(_env.WebRootPath, "separados", "mdx_extra_q");
            var arquivosWav = new List<string>();

            if (Directory.Exists(separadosPath))
            {
                foreach (var pasta in Directory.GetDirectories(separadosPath))
                {
                    var arquivos = Directory.GetFiles(pasta, "*.wav");
                    arquivosWav.AddRange(arquivos.Select(f => Path.GetRelativePath(_env.WebRootPath, f)));
                }
            }

            ViewBag.Files = arquivosWav;
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

                // 👇 Executa o Demucs em segundo plano
                _ = Task.Run(() =>
                {
                    System.Console.WriteLine($"Comando: demucs -n mdx_extra_q -o \"{outputPath}\" \"{filePath}\"");
                    System.Console.WriteLine($"Arquivo existe? {System.IO.File.Exists(filePath)}");

                    var psi = new ProcessStartInfo
                    {
                        FileName = "python3",
                        Arguments = $"-m demucs -n mdx_extra_q -o \"{outputPath}\" \"{filePath}\"",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = new Process { StartInfo = psi };

                    process.OutputDataReceived += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            System.Console.WriteLine("OUT: " + e.Data);
                    };

                    process.ErrorDataReceived += (s, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            System.Console.WriteLine("ERR: " + e.Data);
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    process.WaitForExit();

                    if (process.ExitCode != 0)
                    {
                        System.Console.WriteLine("Erro ao executar demucs. Código de saída: " + process.ExitCode);
                    }
                    else
                    {
                        System.Console.WriteLine("Demucs executado com sucesso!");
                    }
                });

                TempData["FolderName"] = fileNameWithoutExt;

                return Content("Arquivo enviado com sucesso. Separação de instrumentos em andamento.");

                var generatedFolder = Path.Combine(outputPath, "mdx_extra_q", fileNameWithoutExt);
                if (Directory.Exists(generatedFolder))
                {
                    var wavs = Directory.GetFiles(generatedFolder, "*.wav");
                    if (wavs.Length == 0)
                    {
                        Console.WriteLine("Demucs terminou mas nenhum arquivo WAV foi gerado.");
                    }
                    else
                    {
                        Console.WriteLine($"Demucs gerou {wavs.Length} arquivos WAV.");
                    }
                }
                else
                {
                    Console.WriteLine("Pasta esperada não encontrada: " + generatedFolder);
                }


            }

            return BadRequest("Arquivo inválido");
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
