// Services/MusicProcessingService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class MusicProcessingService : BackgroundService
{
    private readonly MusicProcessingQueue _queue;
    private readonly ILogger<MusicProcessingService> _logger;

    public MusicProcessingService(MusicProcessingQueue queue, ILogger<MusicProcessingService> logger)
    {
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Serviço de processamento iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var (filePath, outputPath) = await _queue.DequeueAsync(stoppingToken);

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "python3",
                    Arguments = $"-m demucs -n mdx_extra_q -o \"{outputPath}\" \"{filePath}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var process = new Process { StartInfo = psi };

                process.OutputDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) _logger.LogInformation("OUT: " + e.Data); };
                process.ErrorDataReceived += (s, e) => { if (!string.IsNullOrEmpty(e.Data)) _logger.LogError("ERR: " + e.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                await process.WaitForExitAsync(stoppingToken);

                if (process.ExitCode == 0)
                    _logger.LogInformation($"Demucs processou com sucesso: {filePath}");
                else
                    _logger.LogError($"Erro na execução do Demucs. Código: {process.ExitCode}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao processar arquivo {filePath}");
            }
        }

        _logger.LogInformation("Serviço de processamento finalizado.");
    }
}
