import easyocr
from os import listdir
from os.path import isfile, join
from argparse import ArgumentParser
import io, sys
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

parser = ArgumentParser()
parser.add_argument("-f", "--folder", dest="folderPath",
                    help="image folder path")
args = parser.parse_args()
#print(args.folderPath)
included_extensions = ['jpg','jpeg', 'bmp', 'png', 'gif']
onlyfiles = [f for f in listdir(args.folderPath) if any(f.endswith(ext) for ext in included_extensions) if f.startswith("img")]
reader = easyocr.Reader(['ja','en']) # this needs to run only once to load the model into memory

for file in onlyfiles:
#    print(args.folderPath + file)
    try:
        print(reader.readtext(join(args.folderPath, file),paragraph = False,detail=0))
    except:
        print("[]")
