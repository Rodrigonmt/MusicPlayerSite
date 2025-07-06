import librosa
import soundfile as sf
import sys
import numpy as np
import scipy.signal

def extrair_sopros(path_other, output_path):
    y, sr = librosa.load(path_other, sr=None)

    # Transformada de Fourier
    y_fft = librosa.stft(y)
    freqs = librosa.fft_frequencies(sr=sr)

    # Foco mais preciso na faixa típica do trompete (fundamental e harmônicos)
    mask = (freqs >= 450) & (freqs <= 2800)
    y_fft[~mask, :] = 0

    # Inversão e suavização
    y_filtered = librosa.istft(y_fft)
    y_filtered = scipy.signal.savgol_filter(y_filtered, 11, 3)

    # Normaliza para evitar estouro
    y_filtered = y_filtered / np.max(np.abs(y_filtered)) * 0.99

    # Salva o áudio filtrado
    sf.write(output_path, y_filtered, sr)

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Uso: python extrair_sopros.py caminho_other.wav destino_brass.wav")
        sys.exit(1)

    path_other = sys.argv[1]
    path_dest = sys.argv[2]
    extrair_sopros(path_other, path_dest)
