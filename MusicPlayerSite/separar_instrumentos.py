import sys
import subprocess
import os

def separar(mp3_path, output_dir):
    command = [
        "python", "-m", "demucs",
        "--two-stems=vocals",  # ou remova para separar todos os instrumentos
        "-o", output_dir,
        mp3_path
    ]
    subprocess.run(command)

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Uso: python separar_instrumentos.py caminho_arquivo.mp3 pasta_destino")
    else:
        mp3_path = sys.argv[1]
        output_dir = sys.argv[2]
        separar(mp3_path, output_dir)