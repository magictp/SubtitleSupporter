import whisper
import io, sys
from argparse import ArgumentParser

sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
#model_size = "medium"  # @param ["base","small", "medium", "large"]
language = "japanese"

try:
    parser = ArgumentParser()
    parser.add_argument("-f", "--file", dest="filePath",
                    help="audio file path")
    parser.add_argument("-ms", "--modelSize", dest="modelSize",
                    help="model_size")
    args = parser.parse_args()

    model = whisper.load_model(args.modelSize)

    result = model.transcribe(audio = f'{args.filePath}', language= language, temperature=0)
    print(result)
except Exception as e:
    print('Error')

