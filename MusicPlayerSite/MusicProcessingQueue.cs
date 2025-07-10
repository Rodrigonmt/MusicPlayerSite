using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

public class MusicProcessingQueue
{
    private readonly ConcurrentQueue<string> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);

    public void Enqueue(string inputPath, string outputPath)
    {
        _queue.Enqueue(() => ProcessFile(inputPath, outputPath));
    }

    private void ProcessFile(string inputPath, string outputPath)
    {
        // Lógica para rodar o Demucs com inputPath e outputPath
        var process = new Process();
        process.StartInfo.FileName = "demucs";
        process.StartInfo.Arguments = $"-n mdx_extra_q -o \"{outputPath}\" \"{inputPath}\"";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine($"Erro ao processar: {error}");
        }
        else
        {
            Console.WriteLine($"Processado com sucesso: {output}");
        }
    }


    public async Task<string> DequeueAsync(CancellationToken cancellationToken)
    {
        await _signal.WaitAsync(cancellationToken);
        _queue.TryDequeue(out var filePath);
        return filePath;
    }
}
